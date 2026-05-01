using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

internal class ThreadToken : ITokenRendererStrategy
{
    public string Token => "%THREAD";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        return message.ThreadId.ToString();
    }
}