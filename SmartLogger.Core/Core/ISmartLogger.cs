namespace SmartLogger.Core;

/// <summary>
/// Defines the primary logging operations and configuration capabilities for the SmartLogger framework.
/// </summary>
/// <remarks>
/// This interface supports multi-level logging, message filtering, and extensible output through appenders.
/// </remarks>
public interface ISmartLogger
{
    /// <summary>
    /// Processes a log entry at the specified <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="message">The text content to be logged.</param>
    void Log(LogLevel level, string message);

    /// <summary>Logs a message at the <see cref="LogLevel.DEBUG"/> level for detailed diagnostic information.</summary>
    /// <param name="message">The diagnostic message.</param>
    void Debug(string message);

    /// <summary>Logs a message at the <see cref="LogLevel.INFO"/> level for general operational entries.</summary>
    /// <param name="message">The informational message.</param>
    void Info(string message);

    /// <summary>Logs a message at the <see cref="LogLevel.WARNING"/> level to highlight potential issues.</summary>
    /// <param name="message">The warning message.</param>
    void Warning(string message);

    /// <summary>Logs a message at the <see cref="LogLevel.ERROR"/> level when an operation fails.</summary>
    /// <param name="message">The error message.</param>
    void Error(string message);

    /// <summary>Logs a message at the <see cref="LogLevel.FATAL"/> level for critical system crashes.</summary>
    /// <param name="message">The fatal error message.</param>
    void Fatal(string message);

    /// <summary>
    /// Updates the minimum <see cref="LogLevel"/> required for messages to be processed.
    /// </summary>
    /// <param name="level">The new minimum threshold level.</param>
    void SetLogLevel(LogLevel level);

    /// <summary>
    /// Attaches an output destination (appender) to the logger.
    /// </summary>
    /// <param name="appender">The implementation of <see cref="ILogAppender"/> to add.</param>
    void AddAppender(ILogAppender appender);

    /// <summary>
    /// Detaches an output destination (appender) from the logger.
    /// </summary>
    /// <param name="appender">The appender to remove.</param>
    void RemoveAppender(ILogAppender appender);

    /// <summary>
    /// Adds a logic-based filter to the logging pipeline.
    /// </summary>
    /// <param name="filter">The <see cref="ILogFilter"/> to evaluate before logging.</param>
    void AddFilter(ILogFilter filter);

    /// <summary>
    /// Removes a logic-based filter from the logging pipeline.
    /// </summary>
    /// <param name="filter">The filter to remove.</param>
    void RemoveFilter(ILogFilter filter);

    /// <summary>
    /// Retrieves a read-only list of all currently attached appenders.
    /// </summary>
    /// <returns>A collection of <see cref="ILogAppender"/> instances.</returns>
    IList<ILogAppender> GetLogAppenders();

    /// <summary>
    /// Retrieves a read-only list of all currently active filters.
    /// </summary>
    /// <returns>A collection of <see cref="ILogFilter"/> instances.</returns>
    IList<ILogFilter> GetLogFilters();
}