namespace SmartLogger.Core;

/// <summary>
/// Defines the contract for rendering a <see cref="LogMessage"/> into a formatted string layout.
/// </summary>
/// <remarks>
/// Responsible for structuring the final log output (e.g., timestamp, level, message).
/// 
/// This abstraction allows multiple layout styles (e.g., simple, detailed, custom patterns)
/// without impacting formatter or appender implementations.
/// </remarks>
internal interface ILogLayoutStrategy
{
    /// <summary>
    /// Renders the given <see cref="LogMessage"/> into its final string representation.
    /// </summary>
    /// <param name="message">The log message to render.</param>
    /// <returns>A formatted string representing the log entry.</returns>
    string Render(LogMessage message);
}