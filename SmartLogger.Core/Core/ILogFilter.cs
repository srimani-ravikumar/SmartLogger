namespace SmartLogger.Core;

/// <summary>
/// Defines a contract for filtering log messages before they are processed by appenders.
/// </summary>
public interface ILogFilter
{
    /// <summary>
    /// Evaluates whether a specific <see cref="LogMessage"/> should be logged.
    /// </summary>
    /// <param name="message">The log entry to evaluate.</param>
    /// <returns><c>true</c> if the message passes the filter; otherwise, <c>false</c>.</returns>
    bool ShouldLog(LogMessage message);

    /// <summary>
    /// Sets the <see cref="LogLevel"/> threshold for this filter.
    /// </summary>
    /// <param name="level">The threshold level.</param>
    void SetLogLevel(LogLevel level);

    /// <summary>
    /// Gets the current <see cref="LogLevel"/> threshold assigned to this filter.
    /// </summary>
    /// <returns>The active <see cref="LogLevel"/>.</returns>
    LogLevel GetLogLevel();
}