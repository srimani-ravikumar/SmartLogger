namespace SmartLogger.Core;

/// <summary>
/// Defines the contract for rendering individual tokens within a log layout.
/// </summary>
/// <remarks>
/// A token renderer is responsible for resolving a specific placeholder
/// (e.g., timestamp, level, message) into its string representation.
/// 
/// Typically used in pattern-based layouts where tokens like
/// <c>%timestamp</c>, <c>%level</c>, <c>%message</c> are dynamically replaced.
/// </remarks>
internal interface ITokenRendererStrategy
{
    /// <summary>
    /// Gets the token identifier handled by this renderer.
    /// </summary>
    /// <remarks>
    /// Example: <c>%timestamp</c>, <c>%level</c>, <c>%message</c>.
    /// </remarks>
    string Token { get; }

    /// <summary>
    /// Renders the token value using the provided <see cref="LogMessage"/>.
    /// </summary>
    /// <param name="message">The log message containing data for rendering.</param>
    /// <returns>The resolved string value for the token.</returns>
    string Render(LogMessage message);
}