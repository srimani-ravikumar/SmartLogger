using SmartLogger.Core;

namespace SmartLogger.Filters;

/// <summary>
/// A filter that allows or denies log messages based on their <see cref="LogLevel"/>.
/// </summary>
public sealed class LevelFilter : ILogFilter
{
    private LogLevel _logLevel;

    /// <summary>
    /// Initializes a new instance of <see cref="LevelFilter"/> defaulting to <see cref="LogLevel.DEBUG"/>.
    /// </summary>
    public LevelFilter() : this(LogLevel.DEBUG) { }

    /// <summary>
    /// Initializes a new instance of <see cref="LevelFilter"/> with a specific threshold.
    /// </summary>
    /// <param name="logLevel">The minimum level required to pass this filter.</param>
    public LevelFilter(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    /// <inheritdoc />
    public bool ShouldLog(LogMessage message)
    {
        // Safety: If the message is null, we reject it by default.
        if (message is null) return false;

        // Logic: Pass if the message level is equal to or higher than our threshold.
        return message.LogLevel.IsGreaterOrEqual(_logLevel);
    }

    /// <inheritdoc />
    public void SetLogLevel(LogLevel level) => _logLevel = level;

    /// <inheritdoc />
    public LogLevel GetLogLevel() => _logLevel;
}