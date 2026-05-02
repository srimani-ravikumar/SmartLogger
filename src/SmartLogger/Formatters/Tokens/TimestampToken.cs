using SmartLogger.Core;
using System;
using System.Globalization;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the timestamp of a <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// Formats the timestamp using a configurable date pattern.
/// 
/// Uses <see cref="CultureInfo.InvariantCulture"/> to ensure consistent
/// output across different environments and locales.
/// </remarks>
internal class TimestampToken : ITokenRendererStrategy
{
    /// <summary>
    /// Date format used for rendering timestamps.
    /// </summary>
    private readonly string _dateFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimestampToken"/> class.
    /// </summary>
    /// <param name="dateFormat">
    /// Date format string (default: <c>yyyy-MM-dd HH:mm:ss.fff</c>).
    /// </param>
    public TimestampToken(string dateFormat = "yyyy-MM-dd HH:mm:ss.fff")
    {
        _dateFormat = dateFormat;
    }

    /// <inheritdoc/>
    public string Token => "%TIMESTAMP";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Format timestamp using invariant culture for consistency
        return message.Timestamp.ToString(_dateFormat, CultureInfo.InvariantCulture);
    }
}