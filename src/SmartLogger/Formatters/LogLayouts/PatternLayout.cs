using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Formatters.LogLayouts;

internal class PatternLayout : ILogLayoutStrategy
{
    private readonly string _pattern;
    private readonly TokenRegistry _tokenRegistry;

    public PatternLayout(string pattern, TokenRegistry tokenRegistry)
    {
        _pattern = pattern;
        _tokenRegistry = tokenRegistry;
    }
    
    public string Render(LogMessage message)
    {

        if (string.IsNullOrWhiteSpace(_pattern))
            return message.Message;

        string result = _pattern;

        foreach (var token in _tokenRegistry.Tokens.Values)
        {
            // Replaces all occurrences of the token in the pattern with the rendered value from the message.
            result = result.Replace(token.Token, token.Render(message));
        }

        return result;
    }
}
