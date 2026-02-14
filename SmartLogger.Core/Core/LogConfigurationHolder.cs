namespace SmartLogger.Core
{
    public class LogConfigurationHolder
    {
        public LogLevel RootLogLevel { get; set; } = LogLevel.INFO;

        public Dictionary<string, LogLevel> LoggerOverrides { get; set; } = new();

        public List<AppenderConfiguration> Appenders { get; set; } = new();

    }

    public class AppenderConfiguration
    {
        public string Type { get; set; } = string.Empty;   // "Console", "File"
        public LogLevel Threshold { get; set; } = LogLevel.DEBUG;

        public Dictionary<string, string> Settings { get; set; } = new(); // filePath, maxSizeMB, etc.
    }

    [Flags]
    public enum LogOutputDestination
    {
        Console = 1,
        FileSystem = 2,
        DatabaseSystem = 3
    }
}