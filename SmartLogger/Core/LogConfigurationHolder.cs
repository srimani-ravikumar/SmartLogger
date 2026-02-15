using System.Collections.Generic;

namespace SmartLogger.Core
{
    public class LogConfigurationHolder
    {
        // Smart loggers default level will always be INFO
        public LogLevel RootLogLevel { get; set; } = LogLevel.INFO;

        public Dictionary<string, LogLevel> LoggerOverrides { get; set; } = new();

        public List<AppenderConfiguration> Appenders { get; set; } = new();

    }

    public class AppenderConfiguration
    {
        public LogOutputDestination Destination { get; set; }

        public LogLevel Threshold { get; set; } = LogLevel.DEBUG;

        public Dictionary<string, string> Settings { get; set; } = []; // filePath, maxSizeMB, etc.
    }

    public enum LogOutputDestination
    {
        Unknown = 0,
        Console = 1,
        FileSystem = 2,
        DatabaseSystem = 3
    }

}