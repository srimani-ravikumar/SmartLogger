using SmartLogger.Core;
using System;
using System.Globalization;

namespace SmartLogger.Formatters.Tokens;

internal class TimestampToken : ITokenRendererStrategy
{
    private readonly string _dateFormat;

    public TimestampToken(string dateFormat = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _dateFormat = dateFormat;
    }

    public string Token => "%TIMESTAMP";

    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException($"{nameof(message)} cannot be null.");

        // Use InvariantCulture for consistent logs across environments
        return message.Timestamp.ToString(_dateFormat, CultureInfo.InvariantCulture);
    }
}