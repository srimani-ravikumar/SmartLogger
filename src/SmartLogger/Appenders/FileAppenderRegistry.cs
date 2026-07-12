using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;
using System.Collections.Concurrent;

namespace SmartLogger.Appenders;

/// <summary>
/// Registry responsible for managing and reusing <see cref="ILogAppender"/> instances for file-based logging.
/// </summary>
/// <remarks>
/// Ensures that only one appender instance exists per unique file target,
/// avoiding duplicate writers and reducing resource contention.
/// 
/// Caching Strategy:
/// <list type="bullet">
/// <item><description>Keyed by file identity (base path + extension)</description></item>
/// <item><description>Thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/></description></item>
/// <item><description>Lazy initialization using <c>GetOrAdd</c></description></item>
/// </list>
/// 
/// Async Behavior:
/// <list type="bullet">
/// <item><description>Async wrapper is applied only once per file</description></item>
/// <item><description>Prevents double-wrapping and redundant worker threads</description></item>
/// </list>
/// </remarks>
/// TODO: Revisit This assumes: Same file → same behavior. 
/// But in reality our design should be: Different formatter, Different log level, Different rolling strategy
/// will be silently ignored after first creation
internal static class FileAppenderRegistry
{
    /// <summary>
    /// Cache of file appenders keyed by file identity.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ILogAppender> _cache = new();

    /// <summary>
    /// Retrieves an existing appender for the given configuration or creates a new one.
    /// </summary>
    /// <param name="config">Appender configuration containing destination details.</param>
    /// <param name="logLevel">Minimum log level threshold.</param>
    /// <param name="formatter">Formatter used for message rendering.</param>
    /// <param name="rollingStrategy">Rolling strategy for file rotation.</param>
    /// <param name="asyncEnabled">Indicates whether asynchronous wrapping should be applied.</param>
    /// <returns>A cached or newly created <see cref="ILogAppender"/> instance.</returns>
    /// <remarks>
    /// If an appender already exists for the same file key, the cached instance is returned,
    /// ignoring differences in formatter, log level, or async flag.
    /// </remarks>
    public static ILogAppender GetOrCreate(
        AppenderConfiguration config,
        LogLevel logLevel,
        ILogOutputFormatterStrategy formatter,
        IRollingStrategy rollingStrategy,
        IFileNamingStrategy namingStrategy,
        bool asyncEnabled)
    {
        var fileConfig = config.Destination.File!;

        // Key represents the logical file identity (shared appender per file)
        var key = $"{fileConfig.FileName}.{fileConfig.Extension}";

        var appender = _cache.GetOrAdd(key, _ =>
        {
            // Create base file appender
            ILogAppender created = new FileAppender(
                fileConfig,
                logLevel,
                formatter,
                rollingStrategy,
                namingStrategy
                );

            // Apply async wrapper only once during creation
            return asyncEnabled
                ? new AsyncAppenderWrapper(created)
                : created;
        });

        /* TODO: Review design decision regarding refresh triggers.
        * Current behavior: Refreshes on initial creation and config reload.
        * Desired behavior: Refresh only upon configuration reload.
        */
        RefreshConfiguration(
            appender,
            logLevel,
            formatter,
            rollingStrategy);

        return appender;
    }

    /// <summary>
    /// Refreshes the configuration of an existing cached file appender.
    /// </summary>
    private static void RefreshConfiguration(
        ILogAppender appender,
        LogLevel logLevel,
        ILogOutputFormatterStrategy formatter,
        IRollingStrategy? rollingStrategy)
    {
        switch (appender)
        {
            case FileAppender fileAppender:
                fileAppender.UpdateConfiguration(
                    logLevel,
                    formatter);
                break;

            case AsyncAppenderWrapper asyncWrapper
                when asyncWrapper.InnerAppender is FileAppender fileAppender:

                fileAppender.UpdateConfiguration(
                    logLevel,
                    formatter);
                break;
        }
    }
}