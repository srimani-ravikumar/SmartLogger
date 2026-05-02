using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Formatters.LogLayouts;

/// <summary>
/// Pattern-based implementation of <see cref="ILogLayoutStrategy"/>.
/// </summary>
/// <remarks>
/// Uses a token-replacement approach to render log messages.
/// Tokens in the pattern (e.g., <c>%TIMESTAMP</c>, <c>%LEVEL</c>) are replaced
/// with values resolved from the provided <see cref="LogMessage"/>.
/// 
/// Example pattern:
/// <code>
/// "[%TIMESTAMP] [%LEVEL] %SOURCE - %MESSAGE"
/// </code>
/// 
/// Design:
/// <list type="bullet">
/// <item><description>Simple string replacement-based rendering</description></item>
/// <item><description>Token resolution delegated to <see cref="ITokenRendererStrategy"/></description></item>
/// <item><description>Extensible via <see cref="TokenRegistry"/></description></item>
/// </list>
/// </remarks>
internal class PatternLayout : ILogLayoutStrategy
{
    /// <summary>
    /// Raw pattern string containing tokens.
    /// </summary>
    private readonly string _pattern;

    /// <summary>
    /// Registry containing token renderers.
    /// </summary>
    private readonly TokenRegistry _tokenRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PatternLayout"/> class.
    /// </summary>
    /// <param name="pattern">Pattern string containing tokens.</param>
    /// <param name="tokenRegistry">Registry used to resolve tokens.</param>
    public PatternLayout(string pattern, TokenRegistry tokenRegistry)
    {
        _pattern = pattern;
        _tokenRegistry = tokenRegistry;
    }

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        // Fallback: return raw message if pattern is not defined
        if (string.IsNullOrWhiteSpace(_pattern))
            return message.Message;

        string result = _pattern;

        // Replace each token with its rendered value
        foreach (var token in _tokenRegistry.Tokens.Values)
        {
            // Note: Replace performs global substitution for each token
            result = result.Replace(token.Token, token.Render(message));
        }

        return result;
    }
}