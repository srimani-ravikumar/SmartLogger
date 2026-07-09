using SmartLogger.Core;
using SmartLogger.Formatters.LogLayouts;
using System;

namespace SmartLogger.Formatters;

/// <summary>
/// Factory responsible for creating <see cref="ILogOutputFormatterStrategy"/> instances
/// based on appender configuration.
/// </summary>
/// <remarks>
/// Composes the formatting pipeline:
/// <list type="bullet">
/// <item><description>Layout → Defines structure (tokens + pattern)</description></item>
/// <item><description>Formatter → Defines output representation (PlainText, JSON, XML)</description></item>
/// </list>
/// 
/// This separation enables flexible combinations of layout and output format.
/// </remarks>
internal class FormatterFactory
{
    /// <summary>
    /// Creates a formatter strategy based on the provided configuration.
    /// </summary>
    /// <param name="appenderConfig">Appender configuration containing formatter settings.</param>
    /// <returns>An initialized <see cref="ILogOutputFormatterStrategy"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the configured output format is not supported.
    /// </exception>
    public static ILogOutputFormatterStrategy Create(AppenderConfiguration appenderConfig)
    {
        // Layout defines how the message is structured (tokens + pattern)
        ILogLayoutStrategy layout = LayoutFactory.Create(appenderConfig);

        // Formatter defines how the structured message is emitted
        return appenderConfig.Formatter.OutputFormat switch
        {
            // PlainText → Layout → Tokens → Final string
            LogOutputFormat.PlainText => new PlainTextFormatter(layout),

            // JSON → Select fields → Build object → Serialize
            LogOutputFormat.Json => new JsonFormatter(
                appenderConfig.Formatter.IncludedJsonFields,
                appenderConfig.Formatter.JsonFieldMappings),

            // XML → Structured representation (layout may not be used directly)
            LogOutputFormat.Xml => new XmlFormatter(),

            _ => throw new NotSupportedException(
                $"Unsupported log output format: {appenderConfig.Formatter.OutputFormat}")
        };
    }
}