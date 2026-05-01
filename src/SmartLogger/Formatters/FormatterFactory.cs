using SmartLogger.Core;
using SmartLogger.Formatters.LogLayouts;
using System;

namespace SmartLogger.Formatters;

internal class FormatterFactory
{
    public static ILogOutputFormatterStrategy Create(AppenderConfiguration config)
    {
        ILogLayoutStrategy layout = LayoutFactory.Create(config);

        return config.OutputFormat switch
        {
            // PlainText → Layout → Tokens → String
            LogOutputFormat.PlainText => new PlainTextFormatter(layout),

            // JSON → Field Selection → Object → Serialize
            LogOutputFormat.Json => new JsonFormatter(config.JsonFields, config.JsonFieldMapping),

            LogOutputFormat.Xml => new XmlFormatter(),

            _ => throw new NotSupportedException($"Unsupported log output format: {config.OutputFormat}")
        };
    }
}