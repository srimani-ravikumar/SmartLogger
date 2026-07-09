using System.Collections.Generic;

namespace SmartLogger.Core;

/// <summary>
/// Represents the root logging configuration.
/// This is the main entry point for configuring logging behavior.
/// </summary>
/// <remarks>
/// Default behavior:
/// - RootLogLevel = INFO
/// - A console appender is automatically enabled if no appenders are configured.
/// </remarks>
public sealed class LogConfigurationHolder
{
    /// <summary>
    /// Gets or sets the default log level.
    /// This level is applied when no specific override is found.
    /// </summary>
    /// <value>Default: INFO</value>
    public LogLevel RootLogLevel { get; set; } = LogLevel.INFO;

    /// <summary>
    /// Gets or sets logger-specific log level overrides.
    /// </summary>
    /// <remarks>
    /// Allows individual loggers to override the root log level.
    ///
    /// Example:
    /// - MyApp.Services → Debug
    /// - MyApp.Controllers → Warning
    /// </remarks>
    public List<LoggerOverrideConfiguration> LoggerOverrides { get; set; } = new();

    /// <summary>
    /// Gets or sets all configured appenders.
    /// Each appender defines where logs go and how they are formatted.
    /// </summary>
    public List<AppenderConfiguration> Appenders { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether a default console appender should be enabled.
    /// </summary>
    /// <remarks>
    /// If no appenders are configured, the system automatically logs to console.
    /// </remarks>
    public bool EnableDefaultConsoleAppender => Appenders == null || Appenders.Count == 0;

    /// <summary>
    /// Enables asynchronous log processing.
    /// </summary>
    /// <value>Default: false</value>
    /// <remarks>
    /// When enabled, logs are processed in background threads to improve performance.
    /// </remarks>
    public bool EnableAsyncLoggingProcess { get; set; } = false;
}


/// <summary>
/// Represents a logger-specific log level override.
/// </summary>
/// <remarks>
/// Allows an individual logger to override the root log level.
/// </remarks>
public sealed class LoggerOverrideConfiguration
{
    /// <summary>
    /// Gets or sets the logger name.
    /// </summary>
    /// <remarks>
    /// Typically the fully qualified namespace or class name.
    /// </remarks>
    public string LoggerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum log level for the logger.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.INFO;
}

/// <summary>
/// Represents a single log appender.
/// An appender defines where logs are written, how they are formatted, and optional filtering.
/// </summary>
/// <remarks>
/// Example:
/// - Console appender with simple format
/// - File appender with JSON format
/// </remarks>
public sealed class AppenderConfiguration
{
    /// <summary>
    /// Gets or sets destination-related configuration.
    /// </summary>
    public DestinationConfiguration Destination { get; set; } = new();

    /// <summary>
    /// Gets or sets formatting configuration.
    /// </summary>
    public FormatterConfiguration Formatter { get; set; } = new();

    /// <summary>
    /// Gets or sets filtering configuration (future extension).
    /// </summary>
    /// <remarks>
    /// If not set, all logs are allowed.
    /// </remarks>
    public object? Filter { get; set; } // TODO: Strong type later

    /// <summary>
    /// Optional override for minimum log level for this appender.
    /// </summary>
    /// <remarks>
    /// If not specified, RootLogLevel will be used.
    /// </remarks>
    public LogLevel? AppenderLogLevel { get; set; }
}

/// <summary>
/// Defines where log messages are written.
/// </summary>
/// <remarks>
/// Default destination is Console.
/// </remarks>
public sealed class DestinationConfiguration
{
    /// <summary>
    /// Gets or sets the output destination.
    /// </summary>
    /// <value>Default: Console</value>
    public LogOutputDestination Type { get; set; } = LogOutputDestination.Console;

    /// <summary>
    /// File-specific configuration.
    /// Required only when Type = FileSystem.
    /// </summary>
    public FileConfiguration? File { get; set; }

    /// <summary>
    /// Database-specific configuration (future extension).
    /// </summary>
    public object? Database { get; set; } // TODO: Strong type later
}

/// <summary>
/// Defines how log messages are formatted before being written.
/// </summary>
/// <remarks>
/// Default is plain text with a simple layout.
/// </remarks>
public sealed class FormatterConfiguration
{
    /// <summary>
    /// Gets or sets output format.
    /// </summary>
    /// <value>Default: PlainText</value>
    public LogOutputFormat OutputFormat { get; set; } = LogOutputFormat.PlainText;

    /// <summary>
    /// Gets or sets layout type.
    /// </summary>
    /// <value>Default: Simple</value>
    public LogMessageLayoutType LayoutType { get; set; } = LogMessageLayoutType.Simple;

    /// <summary>
    /// Custom pattern used when Layout = Custom.
    /// </summary>
    /// <remarks>
    /// Example: "%date [%level] %message"
    /// </remarks>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Defines fields to include when using JSON format.
    /// </summary>
    /// <remarks>
    /// Default fields provide a balanced observability view.
    /// </remarks>
    public List<string> IncludedJsonFields { get; set; } =
        new() { "timestamp", "level", "thread", "correlation", "source", "message" };

    /// <summary>
    /// Gets or sets custom JSON field name mappings.
    /// </summary>
    /// <remarks>
    /// Allows default JSON field names to be renamed.
    ///
    /// Example:
    /// timestamp → ts
    /// correlation → cid
    /// </remarks>
    public List<JsonFieldMappingConfiguration> JsonFieldMappings { get; set; } = new();
}

/// <summary>
/// Represents a JSON field name mapping.
/// </summary>
/// <remarks>
/// Allows a default JSON field name to be mapped to a custom name.
/// </remarks>
public sealed class JsonFieldMappingConfiguration
{
    /// <summary>
    /// Gets or sets the original JSON field name.
    /// </summary>
    public string SourceField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom JSON field name.
    /// </summary>
    public string TargetField { get; set; } = string.Empty;
}

/// <summary>
/// Represents configuration for file-based logging.
/// </summary>
/// <remarks>
/// Encapsulates all settings required for:
/// <list type="bullet">
/// <item><description>File path and extension</description></item>
/// <item><description>File naming strategy</description></item>
/// <item><description>Rolling policy (size/time-based rotation)</description></item>
/// </list>
/// </remarks>
public sealed class FileConfiguration
{
    /// <summary>
    /// Base file path without extension or suffix.
    /// </summary>
    /// <remarks>
    /// Example: <c>logs/app</c> → final file may become <c>logs/app-2026-05-02.log</c>
    /// </remarks>
    public string BasePath { get; set; } = "logs/app";

    /// <summary>
    /// File extension (without leading dot).
    /// </summary>
    /// <remarks>
    /// Example: <c>log</c>, <c>txt</c>, <c>json</c>
    /// </remarks>
    public string Extension { get; set; } = "log";

    /// <summary>
    /// Controls how file names are constructed.
    /// </summary>
    public FileNamingConfiguration Naming { get; set; } = new();

    /// <summary>
    /// Defines rolling policy for file rotation.
    /// </summary>
    public RollingPolicyConfiguration RollingPolicy { get; set; } = new();
}

/// <summary>
/// Defines how log file names are constructed.
/// </summary>
/// <remarks>
/// Combines base name with optional components such as date and index.
/// </remarks>
public sealed class FileNamingConfiguration
{
    /// <summary>
    /// Indicates whether the current date should be included in the file name.
    /// </summary>
    public bool IncludeDate { get; set; } = true;

    /// <summary>
    /// Date format used when <see cref="IncludeDate"/> is enabled.
    /// </summary>
    /// <remarks>
    /// Uses standard .NET date format strings (e.g., <c>yyyy-MM-dd</c>).
    /// </remarks>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Indicates whether a rolling index should be included.
    /// </summary>
    /// <remarks>
    /// Used to differentiate multiple files within the same time window.
    /// </remarks>
    public bool IncludeIndex { get; set; } = true;

    /// <summary>
    /// Separator used between file name components.
    /// </summary>
    /// <remarks>
    /// Example: <c>-</c>, <c>_</c>
    /// </remarks>
    public string Separator { get; set; } = "-";
}


/// <summary>
/// Defines rules for log file rotation.
/// </summary>
/// <remarks>
/// Supports multiple rolling strategies:
/// <list type="bullet">
/// <item><description>Size-based rolling</description></item>
/// <item><description>Time-based rolling</description></item>
/// </list>
/// </remarks>
public sealed class RollingPolicyConfiguration
{
    /// <summary>
    /// Type of rolling strategy to apply.
    /// </summary>
    public RollingType RollingType { get; set; } = RollingType.None;

    /// <summary>
    /// Maximum file size in megabytes before triggering a roll.
    /// </summary>
    /// <remarks>
    /// Applicable only for size-based rolling.
    /// </remarks>
    public long MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Time interval used for rolling.
    /// </summary>
    /// <remarks>
    /// Applicable only for time-based rolling.
    /// </remarks>
    public RollingInterval Interval { get; set; } = RollingInterval.None;

    /// <summary>
    /// Maximum number of rolled files to retain.
    /// </summary>
    /// <remarks>
    /// Older files may be deleted when this limit is exceeded.
    /// </remarks>
    public int MaxRetainedFiles { get; set; } = 7;

    /// <summary>
    /// Date format used for time-based rolling.
    /// </summary>
    /// <remarks>
    /// Should align with <see cref="FileNamingConfiguration.DateFormat"/> when both are used.
    /// </remarks>
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

/// <summary>
/// Represents the strategy used for log file rolling.
/// </summary>
/// <remarks>
/// Determines how and when log files are rotated.
/// </remarks>
public enum RollingType
{
    /// <summary>
    /// No rolling is applied. Logs continue writing to a single file.
    /// </summary>
    None,

    /// <summary>
    /// Rolling occurs when the file size exceeds a configured threshold.
    /// </summary>
    Size,

    /// <summary>
    /// Rolling occurs based on time intervals (e.g., hourly, daily).
    /// </summary>
    Time,

    /// <summary>
    /// Rolling occurs based on a combination of size and time conditions.
    /// </summary>
    /// <remarks>
    /// Not yet implemented.
    /// </remarks>
    Hybrid
}

/// <summary>
/// Represents the time interval used for time-based rolling.
/// </summary>
public enum RollingInterval
{
    /// <summary>
    /// No time-based rolling is applied.
    /// </summary>
    None,

    /// <summary>
    /// Roll logs every hour.
    /// </summary>
    Hour,

    /// <summary>
    /// Roll logs every day.
    /// </summary>
    Day,

    /// <summary>
    /// Roll logs every month.
    /// </summary>
    Month
}