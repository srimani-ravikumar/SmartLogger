using SmartLogger.Core;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Formatters.Tokens;

internal class TokenRegistry
{
    private readonly Dictionary<string, ITokenRendererStrategy> _tokens;

    public TokenRegistry(IEnumerable<ITokenRendererStrategy> tokens)
    {
        _tokens = tokens.ToDictionary(
            keySelector: tokenStrategy => tokenStrategy.Token,
            elementSelector: tokenStrategy => tokenStrategy);
    }

    public IReadOnlyDictionary<string, ITokenRendererStrategy> Tokens => _tokens;
}