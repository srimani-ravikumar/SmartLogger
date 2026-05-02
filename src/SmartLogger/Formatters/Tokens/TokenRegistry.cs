using SmartLogger.Core;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Registry for managing and resolving token renderers used in pattern layouts.
/// </summary>
/// <remarks>
/// Maps token identifiers (e.g., <c>%TIMESTAMP</c>, <c>%LEVEL</c>) 
/// to their corresponding <see cref="ITokenRendererStrategy"/> implementations.
/// 
/// This enables:
/// <list type="bullet">
/// <item><description>Fast lookup during pattern rendering</description></item>
/// <item><description>Centralized token management</description></item>
/// <item><description>Extensibility via new token strategies</description></item>
/// </list>
/// </remarks>
internal class TokenRegistry
{
    /// <summary>
    /// Internal mapping of token string to renderer strategy.
    /// </summary>
    private readonly Dictionary<string, ITokenRendererStrategy> _tokens;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRegistry"/> class.
    /// </summary>
    /// <param name="tokens">Collection of token renderer strategies.</param>
    /// <remarks>
    /// Assumes that each token string is unique.
    /// Duplicate tokens will result in an exception during dictionary construction.
    /// </remarks>
    public TokenRegistry(IEnumerable<ITokenRendererStrategy> tokens)
    {
        _tokens = tokens.ToDictionary(
            tokenStrategy => tokenStrategy.Token,
            tokenStrategy => tokenStrategy);
    }

    /// <summary>
    /// Gets the registered token renderers.
    /// </summary>
    /// <value>
    /// A read-only dictionary mapping token strings to renderer strategies.
    /// </value>
    public IReadOnlyDictionary<string, ITokenRendererStrategy> Tokens => _tokens;
}