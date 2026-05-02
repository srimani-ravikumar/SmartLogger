using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the message content of a <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// Returns the raw message text provided during logging.
/// Falls back to an empty string when the message is null to ensure
/// safe rendering without exceptions.
/// </remarks>
internal class MessageToken : ITokenRendererStrategy
{
    /// <inheritdoc/>
    public string Token => "%MESSAGE";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Return message content or fallback to empty string
        return message.Message ?? string.Empty;
    }
}