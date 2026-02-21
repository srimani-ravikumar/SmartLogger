using SmartLogger.Appenders;
using System;
using System.Collections.Concurrent;

namespace SmartLogger.Core;

/// <summary>
/// Responsible for creating configured <see cref="ISmartLogger"/> instances
/// based on the active <see cref="LogConfigurationHolder"/>.
/// </summary>
internal class LoggerFactory
{
    /* Marked as volatile to ensure that configuration updates made by one thread
    ** are immediately visible to all other threads without caching.
    ** Since configuration is replaced atomically via reference swap,
    ** volatile guarantees memory visibility while avoiding locks.
    */
    private volatile LogConfigurationHolder _configuration;

    private readonly ConcurrentDictionary<string, ISmartLogger> _loggers = new();

    /// <summary>
    /// Initializes a new instance of <see cref="LoggerFactory"/>
    /// using the specified configuration provider.
    /// </summary>
    /// <param name="provider">
    /// The configuration provider used to load logging settings.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provider is null.
    /// </exception>
    internal LoggerFactory(ILogConfigurationProvider provider)
    {
        _configuration = provider.Load();
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
            // 1. Determine the log level based on the name (e.g., namespace overrides)
            LogLevel initialLevel = ResolveLogLevel(loggerName);

            // 2. Instantiate the implementation
            // We disable the default appender because we are manually attaching them from config
            var logger = new LoggerImplementation(
                name: loggerName,
                logLevel: initialLevel,
                enableDefaultAppender: false);

            // 3. Populate the logger with appenders defined in the current configuration
            foreach (var appenderConfig in _configuration.Appenders)
            {
                var appender = CreateAppender(appenderConfig);
                logger.AddAppender(appender);
            }

            return logger;
        });

        return instance;
    }

    /// <summary>
    /// Atomically updates the active logging configuration.
    /// Newly created loggers will use the updated configuration.
    /// </summary>
    /// <param name="newConfig">The new configuration to apply.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the new configuration is null.
    /// </exception>
    internal void UpdateConfiguration(LogConfigurationHolder newConfig)
    {
        if (newConfig == null)
            throw new ArgumentNullException(nameof(newConfig));

        _configuration = newConfig; // atomic reference swap
    }

    /// <summary>
    /// Resolves the effective log level for the specified logger name,
    /// checking overrides before falling back to the root level.
    /// </summary>
    /// <param name="loggerName">The logger name to resolve.</param>
    /// <returns>The effective <see cref="LogLevel"/>.</returns>
    private LogLevel ResolveLogLevel(string loggerName)
    {
        if (_configuration.LoggerOverrides.TryGetValue(loggerName, out var level))
            return level;

        return _configuration.RootLogLevel;
    }

    /// <summary>
    /// Creates an appropriate <see cref="ILogAppender"/> instance
    /// based on the provided configuration.
    /// </summary>
    /// <param name="config">The appender configuration.</param>
    /// <returns>An initialized <see cref="ILogAppender"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the destination type is not supported.
    /// </exception>
    private ILogAppender CreateAppender(AppenderConfiguration config)
    {
        return config.Destination switch
        {
            LogOutputDestination.Console =>
                new ConsoleAppender(config.Threshold),

            LogOutputDestination.FileSystem =>
                new FileAppender(
                    config.Settings["filePath"],
                    config.Threshold),

            // ToDo
            //LogOutputDestination.DatabaseSystem =>
            //    new DatabaseAppender(
            //        config.Settings["connectionString"],
            //        config.Threshold),

            _ => throw new NotSupportedException(
                    $"Unsupported destination: {config.Destination}")
        };
    }
}