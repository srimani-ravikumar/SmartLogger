namespace SmartLogger.Core;

/// <summary>
/// Defines a contract for logging messages with various severity levels in the SmartLogger framework.
/// </summary>
public interface ISmartLogger
{
    /// <summary>
    /// Processes a log entry at the specified <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logLevel">The severity level of the message.</param>
    /// <param name="message">The text content to be logged.</param>
    void Log(LogLevel logLevel, string message);

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
}