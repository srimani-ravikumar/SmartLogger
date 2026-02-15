using SmartLogger.Appenders;
using System;

namespace SmartLogger.Core;

public class LoggerFactory
{
    private volatile LogConfigurationHolder _configuration;

    public LoggerFactory(ILogConfigurationProvider provider)
    {
        _configuration = provider.Load();
    }

    public ISmartLogger CreateLogger(string name)
    {
        var logger = new LoggerImplementation(
            name: name,
            logLevel: ResolveLogLevel(name),
            enableDefaultAppender: false);

        foreach (AppenderConfiguration appenderConfig in _configuration.Appenders)
        {
            ILogAppender appender = CreateAppender(appenderConfig);
            logger.AddAppender(appender);
        }

        return logger;
    }

    private LogLevel ResolveLogLevel(string loggerName)
    {
        if (_configuration.LoggerOverrides.TryGetValue(loggerName, out var level))
            return level;

        return _configuration.RootLogLevel;
    }

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

    public void UpdateConfiguration(LogConfigurationHolder newConfig)
    {
        if (newConfig == null)
            throw new ArgumentNullException(nameof(newConfig));

        _configuration = newConfig; // atomic reference swap
    }
}