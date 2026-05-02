using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters.Tokens;

/// <summary>
/// Renders the thread identifier associated with a <see cref="LogMessage"/>.
/// </summary>
/// <remarks>
/// Useful for diagnosing concurrency issues and understanding execution flow
/// across multiple threads.
/// </remarks>
internal class ThreadToken : ITokenRendererStrategy
{
    /// <inheritdoc/>
    public string Token => "%THREAD";

    /// <inheritdoc/>
    public string Render(LogMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), $"{nameof(message)} cannot be null.");

        // Convert thread ID to string for rendering
        return message.ThreadId.ToString();
    }
}