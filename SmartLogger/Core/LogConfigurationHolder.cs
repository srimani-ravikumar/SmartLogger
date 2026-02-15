using System.Collections.Generic;

namespace SmartLogger.Core
{
    /// <summary>
    /// Holds the complete logging configuration
    /// including root level, logger overrides, and appenders.
    /// </summary>
    public class LogConfigurationHolder
    {
        // Smart loggers default level will always be INFO

        /// <summary>
        /// Gets or sets the default log level applied
        /// when no specific logger override is defined.
        /// </summary>
        public LogLevel RootLogLevel { get; set; } = LogLevel.INFO;

        /// <summary>
        /// Gets or sets logger-specific level overrides.
        /// Key represents logger name (e.g., namespace/class),
        /// value represents the minimum log level.
        /// </summary>
        public Dictionary<string, LogLevel> LoggerOverrides { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of configured appenders
        /// that define where log messages are written.
        /// </summary>
        public List<AppenderConfiguration> Appenders { get; set; } = new();
    }

    /// <summary>
    /// Represents configuration settings for a specific log output destination.
    /// </summary>
    public class AppenderConfiguration
    {
        /// <summary>
        /// Gets or sets the destination type
        /// where log messages will be written.
        /// </summary>
        public LogOutputDestination Destination { get; set; }

        /// <summary>
        /// Gets or sets the minimum log level required
        /// for this appender to process a message.
        /// </summary>
        public LogLevel Threshold { get; set; } = LogLevel.DEBUG;

        /// <summary>
        /// Gets or sets additional key-value configuration settings
        /// specific to the destination (e.g., filePath, maxSizeMB).
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new(); // filePath, maxSizeMB, etc.
    }

    /// <summary>
    /// Defines the supported log output destinations.
    /// </summary>
    public enum LogOutputDestination
    {
        /// <summary>
        /// Undefined or unconfigured destination.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Writes log output to the system console.
        /// </summary>
        Console = 1,

        /// <summary>
        /// Writes log output to the file system.
        /// </summary>
        FileSystem = 2,

        /// <summary>
        /// Writes log output to a database system.
        /// </summary>
        DatabaseSystem = 3
    }
}