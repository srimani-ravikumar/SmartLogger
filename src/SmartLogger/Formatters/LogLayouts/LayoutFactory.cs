using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;
using System;

namespace SmartLogger.Formatters.LogLayouts;

/// <summary>
/// Factory responsible for creating <see cref="ILogLayoutStrategy"/> instances
/// based on the configured layout type.
/// </summary>
/// <remarks>
/// Composes:
/// <list type="bullet">
/// <item><description>Token renderers (via <see cref="TokenRegistry"/>)</description></item>
/// <item><description>Pattern-based layout definitions</description></item>
/// </list>
/// 
/// This design enables flexible layout customization without modifying formatter logic.
/// </remarks>
internal static class LayoutFactory
{
    /// <summary>
    /// Creates a layout strategy based on the provided appender configuration.
    /// </summary>
    /// <param name="appenderConfig">Appender configuration containing layout settings.</param>
    /// <returns>An initialized <see cref="ILogLayoutStrategy"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the layout type is not supported.
    /// </exception>
    public static ILogLayoutStrategy Create(AppenderConfiguration appenderConfig)
    {
        // Register all supported tokens (extensible point)
        var tokens = new ITokenRendererStrategy[]
        {
            new TimestampToken(),
            new LevelToken(),
            new MessageToken(),
            new SourceToken(),
            new ThreadToken(),
            new CorrelationToken()
        };

        var registry = new TokenRegistry(tokens);

        // Select layout strategy based on configuration
        return appenderConfig.Formatter.LayoutType switch
        {
            // Minimal layout for readability
            LogMessageLayoutType.Simple => new PatternLayout(
                "[%TIMESTAMP] [%LEVEL] %SOURCE - %MESSAGE",
                registry),

            // Extended layout with thread and correlation context
            LogMessageLayoutType.Detailed => new PatternLayout(
                "[%TIMESTAMP] [%LEVEL] [T#%THREAD] [%CORRELATION] %SOURCE - %MESSAGE",
                registry),

            // Fully user-defined pattern
            LogMessageLayoutType.Custom => new PatternLayout(
                appenderConfig.Formatter.Pattern,
                registry),

            _ => throw new NotSupportedException(
                $"Unsupported layout type: {appenderConfig.Formatter.LayoutType}")
        };
    }
}