using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

internal class SourceToken : ITokenRendererStrategy
{
    public string Token => "%SOURCE";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        return message.Source;
    }
}