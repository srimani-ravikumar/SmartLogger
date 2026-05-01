namespace SmartLogger.Core;

internal interface ITokenRendererStrategy
{
    string Token { get; }
    string Render(LogMessage message);
}
