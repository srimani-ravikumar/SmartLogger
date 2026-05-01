using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Core;

/// <summary>
/// Holds the complete logging configuration
/// including root level, logger overrides, and appenders.
/// </summary>
public sealed class LogConfigurationHolder
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

    /// <summary>
    /// Set to true to enable a default console appender when no appenders are configured.
    /// </summary>
    public bool EnableDefaultConsoleAppender
    {
        get
        {
            return Appenders is null || Appenders.Count == 0;
        }
    }
}

/// <summary>
/// Represents configuration settings for a specific log output destination.
/// </summary>
public sealed class AppenderConfiguration
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
    public LogLevel Threshold { get; set; } = LogLevel.INFO;

    public FileConfiguration File { get; set; }

    public LogOutputFormat OutputFormat { get; set; } = LogOutputFormat.PlainText;

    public LogMessageLayoutType LayoutType { get; set; } = LogMessageLayoutType.Simple;

    public string Pattern { get; set; } = string.Empty;

    // be default the system should allow all fields
    public List<string> JsonFields { get; set; } = new() { "timestamp", "level", "thread", "correlation", "source", "message" };

    // optional: to configured the json property name
    public Dictionary<string, string> JsonFieldMapping { get; set; } = new();
}

public sealed class FileConfiguration
{
    /// <summary>
    /// Base path without extension or suffix.
    /// Example: logs/app
    /// </summary>
    public string BasePath { get; set; } = "logs/app";

    /// <summary>
    /// File extension (without dot).
    /// Example: log, txt, json
    /// </summary>
    public string Extension { get; set; } = "log";

    /// <summary>
    /// Controls how file names are constructed.
    /// </summary>
    public FileNamingConfiguration Naming { get; set; } = new();

    /// <summary>
    /// Rolling policy for file rotation.
    /// </summary>
    public RollingPolicyConfiguration RollingPolicy { get; set; } = new();
}

public sealed class FileNamingConfiguration
{
    /// <summary>
    /// Whether to include date in file name.
    /// </summary>
    public bool IncludeDate { get; set; } = true;

    /// <summary>
    /// Date format used when IncludeDate is true.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Whether to include rolling index.
    /// </summary>
    public bool IncludeIndex { get; set; } = true;

    /// <summary>
    /// Separator between parts.
    /// Example: "-", "_"
    /// </summary>
    public string Separator { get; set; } = "-";
}

public sealed class RollingPolicyConfiguration
{
    public RollingType RollingType { get; set; } = RollingType.None;

    // Size-based (in MB), defaults to 10 mb
    public long MaxFileSizeMB { get; set; } = 10;

    // Time-based, defaults to None
    public RollingInterval Interval { get; set; } = RollingInterval.None;

    // Max retained files, default to seven
    public int MaxRetainedFiles { get; set; } = 7;

    public string DateFormat { get; set; } = "yyyy-MM-dd";
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

/// <summary>
/// Defines the supported log output formats for log messages.
/// </summary>
public enum LogOutputFormat
{
    /// <summary>
    /// Output log messages in plain text format.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// Output log messages in JSON format. (recommended for modern systems)
    /// </summary>
    Json = 1,

    /// <summary>
    /// Output log messages in XML format. (to support legacy systems)
    /// </summary>
    Xml = 3
}

/// <summary>
/// Defines the supported log message layout types that determine
/// how log messages are formatted and presented.
/// </summary>
public enum LogMessageLayoutType
{
    /// <summary>
    /// Represents a simple layout type with information such as timestamp, log level, and message.
    /// </summary>
    Simple,

    /// <summary>
    /// Represents a detailed layout with additional context such as thread information, correlation context and more.
    /// </summary>
    Detailed,

    /// <summary>
    /// Represents a custom layout defined by the user via a pattern string (e.g., "%date [%level] %message").
    /// </summary>
    Custom
}

public enum RollingType
{
    None,
    Size,
    Time,
    Hybrid // TODO
}

public enum RollingInterval
{
    None,
    Hour,
    Day,
    Month
}