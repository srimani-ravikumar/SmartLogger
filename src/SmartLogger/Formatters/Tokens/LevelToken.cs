using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the log level of a <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// Produces a padded string representation to ensure visual alignment
/// across log entries (e.g., <c>INFO </c>, <c>ERROR</c>).
/// </remarks>
internal class LevelToken : ITokenRendererStrategy
{
    /// <inheritdoc/>
    public string Token => "%LEVEL";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Pad for consistent alignment in log output
        // Example: [INFO ] vs [ERROR]
        return message.LogLevel.ToString().PadRight(5);
    }
}