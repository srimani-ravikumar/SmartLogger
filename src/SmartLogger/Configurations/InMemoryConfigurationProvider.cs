using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Configurations;

/// <summary>
/// Provides SmartLogger configuration from an in-memory object.
/// </summary>
/// <remarks>
/// Useful for:
/// <list type="bullet">
/// <item><description>Unit testing</description></item>
/// <item><description>Programmatic configuration</description></item>
/// <item><description>Bootstrapping without external config sources</description></item>
/// </list>
/// 
/// The configuration is validated before being returned to ensure correctness.
/// </remarks>
public sealed class InMemoryConfigurationProvider : ILogConfigurationProvider
{
    /// <summary>
    /// Backing configuration instance.
    /// </summary>
    private readonly LogConfigurationHolder _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryConfigurationProvider"/> class.
    /// </summary>
    /// <param name="configuration">Pre-constructed configuration object.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public InMemoryConfigurationProvider(LogConfigurationHolder configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public LogConfigurationHolder Load()
    {
        // Ensure configuration is valid before usage
        Validate(_configuration);

        return _configuration;
    }

    /// <summary>
    /// Creates a minimal default configuration for quick setup.
    /// </summary>
    /// <returns>A ready-to-use <see cref="InMemoryConfigurationProvider"/> instance.</returns>
    /// <remarks>
    /// Default behavior:
    /// <list type="bullet">
    /// <item><description>Root level: <see cref="LogLevel.DEBUG"/></description></item>
    /// <item><description>Single console appender</description></item>
    /// </list>
    /// </remarks>
    public static InMemoryConfigurationProvider CreateDefault()
    {
        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
            Appenders = new List<AppenderConfiguration>
            {
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    },
                    AppenderLogLevel = LogLevel.DEBUG
                }
            }
        };

        return new InMemoryConfigurationProvider(config);
    }

    /// <summary>
    /// Validates the provided configuration for structural correctness.
    /// </summary>
    /// <param name="config">Configuration to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration is invalid or incomplete.
    /// </exception>
    private static void Validate(LogConfigurationHolder config)
    {
        // Ensure at least one appender is configured
        if (config.Appenders is null || !config.Appenders.Any())
        {
            throw new InvalidOperationException(
                "At least one appender must be configured.");
        }

        // Validate each appender destination
        foreach (var appender in config.Appenders)
        {
            if (appender.Destination.Type == LogOutputDestination.Unknown)
            {
                throw new InvalidOperationException(
                    "Appender destination must be specified.");
            }
        }
    }
}