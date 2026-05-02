using SmartLogger.Core;

namespace SmartLogger.Formatters;

/// <summary>
/// Plain text implementation of <see cref="ILogOutputFormatterStrategy"/>.
/// </summary>
/// <remarks>
/// Delegates rendering to the configured <see cref="ILogLayoutStrategy"/>.
/// 
/// Pipeline:
/// <list type="bullet">
/// <item><description>Layout → resolves tokens and builds structured string</description></item>
/// <item><description>Formatter → returns final plain text output</description></item>
/// </list>
/// 
/// This formatter is primarily intended for human-readable logs.
/// </remarks>
internal class PlainTextFormatter : ILogOutputFormatterStrategy
{
    /// <summary>
    /// Layout responsible for structuring the log message.
    /// </summary>
    private readonly ILogLayoutStrategy _layout;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextFormatter"/> class.
    /// </summary>
    /// <param name="layout">Layout strategy used to render the log message.</param>
    public PlainTextFormatter(ILogLayoutStrategy layout)
    {
        _layout = layout;
    }

    /// <inheritdoc />
    public string Format(LogMessage message) => _layout.Render(message);
}