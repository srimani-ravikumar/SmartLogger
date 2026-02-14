using SmartLogger.Appenders;

namespace SmartLogger.Core;

public class LoggerFactory
{
    private readonly LogConfigurationHolder _configuration;

    public LoggerFactory(ILogConfigurationProvider provider)
    {
        _configuration = provider.Load();
    }

    public ISmartLogger CreateLogger(string name)
    {
        var logger = new LoggerImplementation(
            name,
            ResolveLogLevel(name),
            enableDefaultAppender: false);

        foreach (var appenderConfig in _configuration.Appenders)
        {
            var appender = CreateAppender(appenderConfig);
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
        return config.Type switch
        {
            "Console" => new ConsoleAppender(config.Threshold),
            "File" => new FileAppender(
                config.Settings["filePath"],
                config.Threshold),
            _ => throw new NotSupportedException()
        };
    }
}