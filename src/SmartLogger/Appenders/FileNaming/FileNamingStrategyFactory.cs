using SmartLogger.Core;
using System;

namespace SmartLogger.Appenders.FileNaming;

/// <summary>
/// Creates file naming strategies.
/// </summary>
internal static class FileNamingStrategyFactory
{
    /// <summary>
    /// Creates the configured naming strategy.
    /// </summary>
    public static IFileNamingStrategy Create(FileConfiguration configuration)
    {
        return configuration.Naming.Strategy switch
        {
            FileNamingStrategyType.Date =>
                new DateFileNamingStrategy(configuration),

            // Future
            // FileNamingStrategyType.Timestamp =>
            //     new TimestampFileNamingStrategy(configuration),

            _ => throw new NotSupportedException(
                $"Unsupported file naming strategy '{configuration.Naming.Strategy}'.")
        };
    }
}