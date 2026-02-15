using SmartLogger.Core;
using System;

namespace SmartLogger.Formatters
{
    internal class SimpleFormatter : ILogFormatter
    {
        public string Format(LogMessage message)
        {
            throw new NotImplementedException();
        }

        public string GetPattern()
        {
            throw new NotImplementedException();
        }

        public void SetDateFormat(string dateFormat)
        {
            throw new NotImplementedException();
        }

        public void SetPattern(string pattern)
        {
            throw new NotImplementedException();
        }
    }
}
