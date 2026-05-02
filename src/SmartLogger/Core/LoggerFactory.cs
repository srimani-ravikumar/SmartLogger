using SmartLogger.Appenders;
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

            // 3. Populate the logger with appenders defined in the current configuration
            foreach (var appenderConfig in config.Appenders)
            {
                ILogAppender appender;

                // 3.1. Enable asyncchronous logging if configured
                if (appenderConfig.Destination.Type == LogOutputDestination.FileSystem)
                {
                    appender = FileAppenderRegistry.GetOrCreate(
                        appenderConfig,
                        appenderConfig.AppenderLogLevel.HasValue ? appenderConfig.AppenderLogLevel.Value : config.RootLogLevel,
                        FormatterFactory.Create(appenderConfig),
                        RollingFactory.Create(appenderConfig.Destination.File),
                        config.EnableAsyncLoggingProcess
                    );
                }
                else
                {
                    appender = CreateAppender(appenderConfig, config);

                    if (config.EnableAsyncLoggingProcess)
                    {
                        appender = new AsyncAppenderWrapper(appender);
                    }
                }

                logger.AddAppender(appender);
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
    private List<ILogAppender> BuildAppenders(LogConfigurationHolder config)
    {
        var list = new List<ILogAppender>();

        foreach (var appenderConfig in config.Appenders)
        {
            var appender = CreateAppender(appenderConfig, config);

            list.Add(config.EnableAsyncLoggingProcess
                ? new AsyncAppenderWrapper(appender)
                : appender);
        }

        return list;
    }

    /// <summary>
    /// Resolves effective log level using overrides or defaults.
    /// </summary>
    private LogLevel ResolveLogLevel(string loggerName, LogConfigurationHolder config)
    {
        if (config.LoggerOverrides.TryGetValue(loggerName, out var level))
            return level;

        return config.Appenders.Any()
            ? config.Appenders.Min(a => a.AppenderLogLevel ?? config.RootLogLevel)
            : config.RootLogLevel;
    }

    /// <summary>
    /// Creates an appender based on configuration.
    /// </summary>
    private ILogAppender CreateAppender(AppenderConfiguration config, LogConfigurationHolder globalConfig)
    {
        var appenderLogLevel = config.AppenderLogLevel ?? globalConfig.RootLogLevel;

        return config.Destination.Type switch
        {
            LogOutputDestination.Console =>
                new ConsoleAppender(appenderLogLevel, FormatterFactory.Create(config)),

            LogOutputDestination.FileSystem =>
                new FileAppender(
                    config.Destination.File,
                    appenderLogLevel,
                    FormatterFactory.Create(config),
                    RollingFactory.Create(config.Destination.File)),

            // ToDo
            //LogOutputDestination.DatabaseSystem =>
            //    new DatabaseAppender(
            //        config.Settings["connectionString"],
            //        config.Threshold),


            _ => throw new NotSupportedException(
                $"Unsupported destination: {config.Destination.Type}")
        };
    }
}