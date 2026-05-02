using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the correlation identifier associated with a log message.
/// </summary>
/// <remarks>
/// Used to trace related log entries across a single execution flow
/// (e.g., request, background job).
/// 
/// Falls back to <c>"N/A"</c> when no correlation ID is present.
/// </remarks>
internal class CorrelationToken : ITokenRendererStrategy
{
    /// <inheritdoc/>
    public string Token => "%CORRELATION";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Return correlation ID if present; otherwise fallback
        return string.IsNullOrWhiteSpace(message.CorrelationId)
            ? "N/A"
            : message.CorrelationId;
    }
}