using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Configurations;

/// <summary>
/// Provides SmartLogger configuration from an in-memory
/// configuration object. Useful for testing or programmatic setup.
/// </summary>
public sealed class InMemoryConfigurationProvider : ILogConfigurationProvider
{
    private readonly LogConfigurationHolder _configuration;

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryConfigurationProvider"/>
    /// with a pre-constructed configuration object.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the configuration is null.
    /// </exception>
    public InMemoryConfigurationProvider(LogConfigurationHolder configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public LogConfigurationHolder Load()
    {
        Validate(_configuration);

        return _configuration;
    }

    /// <summary>
    ///  Optional helper factory method for quick setup
    /// </summary>
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
}