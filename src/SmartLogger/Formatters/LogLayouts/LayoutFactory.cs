using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;
using System;

namespace SmartLogger.Formatters.LogLayouts;

internal static class LayoutFactory
{
    public static ILogLayoutStrategy Create(AppenderConfiguration config)
    {
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

        return config.LayoutType switch
        {
            LogMessageLayoutType.Simple => new PatternLayout(
                "[%TIMESTAMP] [%LEVEL] %SOURCE - %MESSAGE",
                registry),

            LogMessageLayoutType.Detailed => new PatternLayout(
                "[%TIMESTAMP] [%LEVEL] [T#%THREAD] [%CORRELATION] %SOURCE - %MESSAGE",
                registry),

            LogMessageLayoutType.Custom => new PatternLayout(
                config.Pattern,
                registry),

            _ => throw new NotSupportedException()
        };
    }
}
