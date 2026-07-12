using SmartLogger.Appenders;
using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Formatters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Core;

/// <summary>
/// Factory responsible for creating, configuring, and caching <see cref="ISmartLogger"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Acts as the central composition root for logger instances:
/// </para>
/// <list type="bullet">
/// <item><description>Resolves effective log levels</description></item>
/// <item><description>Constructs appenders based on configuration</description></item>
/// <item><description>Caches loggers for reuse</description></item>
/// <item><description>Supports dynamic configuration updates</description></item>
/// </list>
/// 
/// <para>
/// Thread Safety:
/// </para>
/// <list type="bullet">
/// <item><description>Logger cache is managed via <see cref="ConcurrentDictionary{TKey, TValue}"/></description></item>
/// <item><description>Configuration updates use atomic reference replacement</description></item>
/// </list>
/// </remarks>
internal class LoggerFactory
{
    /// <summary>
    /// Active configuration snapshot.
    /// </summary>
    /// <remarks>
    /// Marked as <c>volatile</c> to ensure visibility across threads.
    /// Updated via atomic reference swap.
    /// </remarks>
    private volatile LogConfigurationHolder _configuration;

    /// <summary>
    /// Cache of loggers keyed by logger name.
    /// </summary>
    private readonly ConcurrentDictionary<string, ISmartLogger> _loggers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerFactory"/> class.
    /// </summary>
    /// <param name="provider">Configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when provider is null.</exception>
    internal LoggerFactory(ILogConfigurationProvider provider)
    {
        _configuration = provider?.Load() ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Retrieves an existing logger from the cache or creates, configures, 
    /// and registers a new one if it does not exist.
    /// </summary>
    /// <param name="name">The name of the logger (e.g., class or namespace).</param>
    /// <returns>A configured <see cref="ISmartLogger"/> instance.</returns>
    internal ISmartLogger GetOrCreateLogger(string name)
    {
        // Here "GetOrAdd" to ensure thread-safe access to the logger cache.
        // The factory lambda only executes if the logger doesn't already exist.
        ISmartLogger instance = _loggers.GetOrAdd(name, loggerName =>
        {
            var config = _configuration; // snapshot for consistency

            // 1. Determine the log level based on the name (e.g., namespace overrides and appender specific)
            LogLevel initialLevel = ResolveLogLevel(loggerName, config);

            // 2. Instantiate the implementation
            // We disable the default appender because we are manually attaching them from config
            var logger = new LoggerImplementation(
                name: loggerName,
                effectiveMinLogLevel: initialLevel,
                enableDefaultConsoleAppender: config.EnableDefaultConsoleAppender);

            foreach (var appenderConfig in config.Appenders)
            {
                logger.AddAppender(
                    CreateConfiguredAppender(
                        appenderConfig,
                        config));
            }

            return logger;
        });

        return instance;
    }

    /// <summary>
    /// Atomically updates the active configuration and propagates changes to existing loggers.
    /// </summary>
    /// <param name="newConfig">New configuration.</param>
    internal void UpdateConfiguration(LogConfigurationHolder newConfig)
    {
        if (newConfig == null)
            throw new ArgumentNullException(nameof(newConfig));

        _configuration = newConfig; // atomic swap

        // Soft reload existing loggers
        foreach (var kvp in _loggers)
        {
            if (kvp.Value is not LoggerImplementation logger)
                continue;

            var loggerName = kvp.Key;

            var newLevel = ResolveLogLevel(loggerName, newConfig);
            var newAppenders = BuildAppenders(newConfig);

            logger.UpdateConfiguration(newLevel, newAppenders);
        }
    }

    /// <summary>
    /// Builds appenders from configuration.
    /// </summary>
    private static List<ILogAppender> BuildAppenders(LogConfigurationHolder config)
    {
        var appenders = new List<ILogAppender>();

        foreach (var appenderConfig in config.Appenders)
        {
            appenders.Add(
                CreateConfiguredAppender(
                    appenderConfig,
                    config));
        }

        return appenders;
    }

    /// <summary>
    /// Resolves effective log level using overrides or defaults.
    /// </summary>
    private static LogLevel ResolveLogLevel(string loggerName, LogConfigurationHolder config)
    {
        if (config.LoggerOverrides?.LastOrDefault(x => x.LoggerName == loggerName) is LoggerOverrideConfiguration overrideConfiguration)
            return overrideConfiguration.LogLevel;

        return config.Appenders.Any()
            ? config.Appenders.Min(a => a.AppenderLogLevel ?? config.RootLogLevel)
            : config.RootLogLevel;
    }

    /// <summary>
    /// Creates a fully configured appender based on configuration.
    /// </summary>
    /// <remarks>
    /// Centralizes appender composition so both initial logger creation
    /// and configuration reload follow the same creation path.
    /// </remarks>
    private static ILogAppender CreateConfiguredAppender(AppenderConfiguration config, LogConfigurationHolder globalConfig)
    {
        var appenderLogLevel =
            config.AppenderLogLevel ?? globalConfig.RootLogLevel;

        switch (config.Destination.Type)
        {
            case LogOutputDestination.Console:
                {
                    ILogAppender appender =
                        new ConsoleAppender(
                            appenderLogLevel,
                            FormatterFactory.Create(config));

                    return globalConfig.EnableAsyncLoggingProcess
                        ? new AsyncAppenderWrapper(appender)
                        : appender;
                }

            case LogOutputDestination.FileSystem:
                {
                    return FileAppenderRegistry.GetOrCreate(
                        config,
                        appenderLogLevel,
                        FormatterFactory.Create(config),
                        RollingStrategyFactory.Create(config.Destination.File),
                        FileNamingStrategyFactory.Create(config.Destination.File),
                        globalConfig.EnableAsyncLoggingProcess);
                }

            default:
                throw new NotSupportedException(
                    $"Unsupported destination: {config.Destination.Type}");
        }
    }
}