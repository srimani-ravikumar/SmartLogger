namespace SmartLogger.Core;

/// <summary>
/// Defines a destination for log messages within the SmartLogger framework.
/// </summary>
public interface ILogAppender
{
    /// <summary>Formats and writes a log entry to the destination.</summary>
    /// <param name="message">The log entry to append.</param>
    void Append(LogMessage message);

    /// <summary>Sets the minimum log level threshold for this appender.</summary>
    /// <param name="logLevel">The threshold level.</param>
    void SetLogLevel(LogLevel logLevel);

    /// <summary>Gets the current log level threshold.</summary>
    /// <param name="logLevel">The current level (interface requirement).</param>
    /// <returns>The active <see cref="LogLevel"/>.</returns>
    LogLevel GetLogLevel(LogLevel logLevel);

    /// <summary>Checks if a specific level meets the appender's threshold.</summary>
    /// <param name="logLevel">The level to check.</param>
    /// <returns>True if the level should be logged; otherwise, false.</returns>
    bool IsEnabled(LogLevel logLevel);

    /// <summary>Sets the formatter used to stringify log messages.</summary>
    /// <param name="formatter">An implementation of <see cref="ILogFormatter"/>.</param>
    void SetFormatter(ILogFormatter formatter);

    /// <summary>Gets the currently assigned formatter.</summary>
    /// <returns>The <see cref="ILogFormatter"/> instance.</returns>
    ILogFormatter GetFormatter();
}