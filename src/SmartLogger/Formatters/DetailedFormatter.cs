using SmartLogger.Core;
using System.Globalization;

namespace SmartLogger.Formatters;

/// <summary>
/// A token-based string formatter that transforms <see cref="LogMessage"/> objects into formatted strings.
/// </summary>
/// <remarks>
/// Supported tokens:
/// <list type="bullet">
/// <item><description><c>%LEVEL</c>: The severity level (padded for alignment)</description></item>
/// <item><description><c>%TIMESTAMP</c>: The date and time of the log</description></item>
/// <item><description><c>%MESSAGE</c>: The log message content</description></item>
/// <item><description><c>%SOURCE</c>: The calling class and method</description></item>
/// <item><description><c>%THREAD</c>: The managed thread ID</description></item>
/// <item><description><c>%CORRELATION</c>: The unique request/trace ID</description></item>
/// </list>
/// </remarks>
public class DetailedFormatter : ILogFormatter
{
    private string _pattern;
    private string _dateFormat = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// Initializes a new instance of <see cref="DetailedFormatter"/> with a professional default pattern.
    /// </summary>
    public DetailedFormatter()
        : this("[%TIMESTAMP] [%LEVEL] [T#%THREAD] [%CORRELATION] %SOURCE - %MESSAGE") { }

    /// <summary>
    /// Initializes a new instance with a custom formatting pattern.
    /// </summary>
    /// <param name="pattern">The string pattern containing replacement tokens.</param>
    public DetailedFormatter(string pattern)
    {
        _pattern = pattern;
    }

    /// <summary>
    /// Formats the provided <see cref="LogMessage"/> into a human-readable string.
    /// </summary>
    /// <param name="message">The log message data to format.</param>
    /// <returns>A formatted string ready for output.</returns>
    public string Format(LogMessage message)
    {
        if (message is null) return string.Empty;

        // Fallback safety if pattern is removed at runtime
        if (string.IsNullOrWhiteSpace(_pattern))
        {
            // Use the existing _dateFormat for consistency, fallback to "G" (General) if all else fails
            string timeStamp = message.Timestamp.ToString(_dateFormat, CultureInfo.InvariantCulture);

            // Include the essential "Pro" fields so you don't lose context during a failure
            return $"[{timeStamp}] [{message.LogLevel,-5}] [T#{message.ThreadId}] [{message.CorrelationId ?? "N/A"}] {message.Source} - {message.Message}";
        }

        // 1. Level Padding: Pads to 5 chars so [INFO ] and [ERROR] align perfectly.
        string paddedLevel = message.LogLevel.ToString().PadRight(5);

        // 2. Date Formatting: Use InvariantCulture for consistent log parsing across different servers.
        string timestampString = message.Timestamp.ToString(_dateFormat, CultureInfo.InvariantCulture);

        // 3. Correlation Handling: Provide a visual indicator if no ID is present.
        string correlationId = message.CorrelationId ?? "N/A";

        // 4. Token Replacement
        return _pattern
            .Replace("%LEVEL", paddedLevel)
            .Replace("%TIMESTAMP", timestampString)
            .Replace("%THREAD", message.ThreadId.ToString())
            .Replace("%CORRELATION", correlationId)
            .Replace("%SOURCE", message.Source ?? "System")
            .Replace("%MESSAGE", message.Message);
    }

    /// <summary>Updates the token pattern used for formatting.</summary>
    /// <param name="pattern">The new pattern string.</param>
    public void SetPattern(string pattern) => _pattern = pattern;

    /// <summary>Gets the current token pattern.</summary>
    /// <returns>The pattern string.</returns>
    public string GetPattern() => _pattern;

    /// <summary>Updates the date format string used for the %TIMESTAMP token.</summary>
    /// <param name="dateFormat">A standard .NET date/time format string.</param>
    public void SetDateFormat(string dateFormat) => _dateFormat = dateFormat;
}