using SmartLogger.Core;

namespace SmartLogger.Formatters;

/// <summary>
/// Placeholder implementation of an XML-based <see cref="ILogOutputFormatterStrategy"/>.
/// </summary>
/// <remarks>
/// Currently returns the raw message without applying XML serialization.
/// 
/// Intended future behavior:
/// <list type="bullet">
/// <item><description>Convert <see cref="LogMessage"/> into structured XML</description></item>
/// <item><description>Support configurable fields and element naming</description></item>
/// </list>
/// 
/// This implementation exists to maintain pipeline compatibility
/// until full XML formatting is introduced.
/// </remarks>
internal class XmlFormatter : ILogOutputFormatterStrategy
{
    /// <inheritdoc />
    public string Format(LogMessage message)
    {
        // TODO: Replace with proper XML serialization
        return message.Message;
    }
}