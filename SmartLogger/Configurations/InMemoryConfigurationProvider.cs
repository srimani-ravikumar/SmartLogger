using SmartLogger.Core;

namespace SmartLogger.Configurations;

public class InMemoryConfigurationProvider : ILogConfigurationProvider
{
    private readonly LogConfigurationHolder _configuration;

    public InMemoryConfigurationProvider(LogConfigurationHolder configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));        
    }

    public LogConfigurationHolder Load()
    {
        Validate(_configuration);

        return _configuration;
    }

    private static void Validate(LogConfigurationHolder config)
    {
        if (config.Appenders is null || !config.Appenders.Any())
        {
            throw new InvalidOperationException(
                "At least one appender must be configured.");
        }

        foreach (var appender in config.Appenders)
        {
            if (appender.Destination == LogOutputDestination.Unknown)
            {
                throw new InvalidOperationException(
                    "Appender destination must be specified.");
            }
        }
    }

    // Optional helper factory method for quick setup
    public static InMemoryConfigurationProvider CreateDefault()
    {
        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
            Appenders = new List<AppenderConfiguration>
            {
                new AppenderConfiguration
                {
                    Destination = LogOutputDestination.Console,
                    Threshold = LogLevel.DEBUG
                }
            }
        };

        return new InMemoryConfigurationProvider(config);
    }
}