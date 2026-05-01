namespace SmartLogger.Core;

internal interface ILogLayoutStrategy
{
    string Render(LogMessage message);
}