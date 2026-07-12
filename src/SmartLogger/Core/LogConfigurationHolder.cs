using System.Collections.Generic;

namespace SmartLogger.Core;

#region Top-level Configuration Holder

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

#endregion

#region Logger Override Configuration


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

#endregion

#region Appender Configuration

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

#endregion

#region Destination Configuration

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
    //public object? Database { get; set; } // TODO: Strong type later
}

#endregion

#region File Configuration

/// <summary>
/// Represents the complete configuration for file-based logging.
/// </summary>
/// <remarks>
/// Encapsulates all settings required for writing logs to the file system,
/// including file naming, rolling policy, archival, and retention.
/// </remarks>
public sealed class FileConfiguration
{
    /// <summary>
    /// Gets or sets the directory where active log files are stored.
    /// </summary>
    /// <remarks>
    /// Default: <c>Logs</c>
    /// </remarks>
    public string Directory { get; set; } = "Logs";

    /// <summary>
    /// Gets or sets the base name of the log file.
    /// </summary>
    /// <remarks>
    /// The configured naming strategy determines how the final file name
    /// is generated from this base name.
    /// </remarks>
    /// <example>
    /// <c>Application</c> → <c>Application-2026-07-12.log</c>
    /// </example>
    public string FileName { get; set; } = "Application";

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    /// <remarks>
    /// Do not include the leading dot.
    /// </remarks>
    /// <example>
    /// <c>log</c>, <c>txt</c>, <c>json</c>
    /// </example>
    public string Extension { get; set; } = "log";

    /// <summary>
    /// Gets or sets the file naming configuration.
    /// </summary>
    public FileNamingConfiguration Naming { get; set; } = new();

    /// <summary>
    /// Gets or sets the file rolling configuration.
    /// </summary>
    public FileRollingConfiguration Rolling { get; set; } = new();

    /// <summary>
    /// Gets or sets archive-related configuration.
    /// </summary>
    public ArchiveConfiguration Archive { get; set; } = new();

    /// <summary>
    /// Gets or sets log retention configuration.
    /// </summary>
    public RetentionConfiguration Retention { get; set; } = new();
}

/// <summary>
/// Defines how log file names are generated.
/// </summary>
/// <remarks>
/// Responsible only for constructing file names.
/// It does not determine when files should roll.
/// </remarks>
public sealed class FileNamingConfiguration
{
    /// <summary>
    /// Gets or sets the file naming strategy.
    /// </summary>
    /// <value>
    /// Default: <see cref="FileNamingStrategyType.Date"/>
    /// </value>
    public FileNamingStrategyType Strategy { get; set; } = FileNamingStrategyType.Date;

    /// <summary>
    /// Gets or sets the date format used by date-based naming strategies.
    /// </summary>
    /// <remarks>
    /// Uses standard .NET date format strings.
    /// </remarks>
    /// <example>
    /// <c>yyyy-MM-dd</c>
    /// </example>
    public string DateFormat { get; set; } = "yyyy-MM-dd";
}

/// <summary>
/// Defines when log files should be rolled.
/// </summary>
/// <remarks>
/// Determines the conditions under which the current log file
/// is replaced with a new one.
/// </remarks>
public sealed class FileRollingConfiguration
{
    /// <summary>
    /// Gets or sets the rolling strategy.
    /// </summary>
    /// <value>
    /// Default: <see cref="RollingStrategyType.Daily"/>
    /// </value>
    public RollingStrategyType Strategy { get; set; } = RollingStrategyType.Daily;

    /// <summary>
    /// Gets or sets the maximum file size in megabytes before rolling occurs.
    /// </summary>
    /// <remarks>
    /// Applicable only when using
    /// <see cref="RollingStrategyType.Size"/>.
    /// </remarks>
    public long MaxFileSizeMB { get; set; } = 10;
}

/// <summary>
/// Defines how rolled log files are archived.
/// </summary>
/// <remarks>
/// Archive processing occurs immediately after a log file is rolled.
/// </remarks>
public sealed class ArchiveConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether archival is enabled.
    /// </summary>
    /// <value>
    /// Default: <c>true</c>
    /// </value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the archive directory.
    /// </summary>
    /// <remarks>
    /// This directory is created automatically if it does not exist.
    /// </remarks>
    /// <value>
    /// Default: <c>Archive</c>
    /// </value>
    public string Directory { get; set; } = "Archive";

    /// <summary>
    /// Gets or sets a value indicating whether archived files
    /// should be compressed.
    /// </summary>
    /// <value>
    /// Default: <c>true</c>
    /// </value>
    public bool Compress { get; set; } = true;
}

/// <summary>
/// Defines how long archived log files are retained.
/// </summary>
/// <remarks>
/// Retention cleanup is evaluated during the rolling process.
/// </remarks>
public sealed class RetentionConfiguration
{
    /// <summary>
    /// Gets or sets the number of days archived log files are retained.
    /// </summary>
    /// <remarks>
    /// Archived files older than the configured number of days
    /// are automatically deleted.
    /// </remarks>
    /// <value>
    /// Default: <c>30</c>
    /// </value>
    public int RetentionDays { get; set; } = 30;
}

/// <summary>
/// Defines the supported strategies used to generate log file names.
/// </summary>
public enum FileNamingStrategyType
{
    /// <summary>
    /// Generates file names using the current date.
    /// </summary>
    Date,

    /// <summary>
    /// Generates file names using the current timestamp.
    /// </summary>
    Timestamp,

    /// <summary>
    /// Uses a custom file naming strategy supplied by the application.
    /// </summary>
    Custom
}

/// <summary>
/// Defines the supported strategies used to determine
/// when log files are rolled.
/// </summary>
public enum RollingStrategyType
{
    /// <summary>
    /// Rolls the log file once per day.
    /// </summary>
    Daily,

    /// <summary>
    /// Rolls the log file when its size exceeds
    /// the configured threshold.
    /// </summary>
    Size
}

#endregion

#region Formatter Configuration


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

#endregion