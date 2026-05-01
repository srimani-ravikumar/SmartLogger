using SmartLogger.Core;

namespace SmartLogger.Formatters;

internal class PlainTextFormatter : ILogOutputFormatterStrategy
{
    private readonly ILogLayoutStrategy _layout;

    public PlainTextFormatter(ILogLayoutStrategy layout)
    {
        _layout = layout;
    }

    /// <inheritdoc />
    public string Format(LogMessage message) => _layout.Render(message);
}