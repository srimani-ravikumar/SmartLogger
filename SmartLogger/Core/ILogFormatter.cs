namespace SmartLogger.Core
{
    public interface ILogFormatter
    {
        string Format(LogMessage message);

        void SetPattern(string pattern);

        string GetPattern();

        void SetDateFormat(string dateFormat);
    }
}
