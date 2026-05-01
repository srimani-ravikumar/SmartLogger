using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

internal class LevelToken : ITokenRendererStrategy
{
    public string Token => "%LEVEL";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        // Pad for alignment: [INFO ] vs [ERROR]
        return message.LogLevel.ToString().PadRight(5);
    }
}