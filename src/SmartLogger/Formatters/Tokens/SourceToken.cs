using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the source of a <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// The source typically represents the logger name
/// (e.g., class or namespace) from which the log was emitted.
/// </remarks>
internal class SourceToken : ITokenRendererStrategy
{
    /// <inheritdoc/>
    public string Token => "%SOURCE";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Returns the originating logger/source name
        return message.Source;
    }
}