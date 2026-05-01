using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

internal class CorrelationToken : ITokenRendererStrategy
{
    public string Token => "%CORRELATION";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        return string.IsNullOrWhiteSpace(message.CorrelationId)
            ? "N/A"
            : message.CorrelationId;
    }
}