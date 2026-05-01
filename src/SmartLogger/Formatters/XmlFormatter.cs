using SmartLogger.Core;

namespace SmartLogger.Formatters;

internal class XmlFormatter : ILogOutputFormatterStrategy
{
    /// <inheritdoc />
    public string Format(LogMessage message)
    {
        return message.Message;
    }
}
