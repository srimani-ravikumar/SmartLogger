using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

internal class MessageToken : ITokenRendererStrategy
{
    public string Token => "%MESSAGE";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        return message.Message ?? string.Empty;
    }
}