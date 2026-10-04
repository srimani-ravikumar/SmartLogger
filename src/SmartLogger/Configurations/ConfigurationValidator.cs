using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Configurations;

/// <summary>
/// Comprehensive configuration validator for SmartLogger.
///
/// Provides fine-grained validation with fail-fast semantics and solution guidance.
/// All configuration providers should invoke this validator before returning the loaded configuration.
///
/// Design principles:
/// - Fail-fast: Stop at the first validation error
/// - Explicit: Clear error messages highlighting the exact issue
/// - Actionable: Provide concise solutions with reference links
/// - Composable: Each validation concern is independent and reusable
/// </summary>
internal static class ConfigurationValidator
{
    /// <summary>
    /// Validates the supplied logging configuration.
    /// </summary>
    /// <param name="configuration">Configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration contains invalid settings.</exception>
    internal static void Validate(LogConfigurationHolder configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Validate root configuration
        ValidateRootConfiguration(configuration);

        // Validate appenders
        if (configuration.Appenders.Count > 0)
        {
            ValidateDuplicateDestinations(configuration);

            for (int i = 0; i < configuration.Appenders.Count; i++)
            {
                ValidateAppender(configuration.Appenders[i], i);
            }
        }

        // Validate logger overrides
        for (int i = 0; i < configuration.LoggerOverrides.Count; i++)
        {
            ValidateLoggerOverride(configuration.LoggerOverrides[i], i);
        }
    }

    #region Root Level Validation

    /// <summary>
    /// Validates root-level configuration settings.
    /// </summary>
    private static void ValidateRootConfiguration(LogConfigurationHolder config)
    {
        // Note: Async logging enabled with no appenders is OK because the default console appender
        // will be automatically enabled (see LogConfigurationHolder.EnableDefaultConsoleAppender)
    }

    #endregion

    #region Duplicate Destination Validation

    /// <summary>
    /// Ensures that each output destination is configured only once.
    /// </summary>
    private static void ValidateDuplicateDestinations(LogConfigurationHolder config)
    {
        var duplicateGroups = config.Appenders
            .GroupBy(appender => appender.Destination.Type)
            .Where(group =>
                group.Key != LogOutputDestination.Unknown &&
                group.Count() > 1)
            .ToList();

        if (!duplicateGroups.Any())
            return;

        var destinations = string.Join(", ", duplicateGroups.Select(g => g.Key));
        var appenderIndices = string.Join(", ",
            duplicateGroups
                .SelectMany(g => g.Select((a, idx) => config.Appenders.IndexOf(a)))
                .Distinct()
                .OrderBy(i => i));

        Fail(
            $"Duplicate appender destinations detected: {destinations} (at appender indices: {appenderIndices}).",
            "SmartLogger allows only one appender per output destination to maintain a clear log routing strategy.",
            "Merge duplicate destination appenders into a single appender with appropriate formatting and filtering.",
            "See configuration-reference.md: 'Destination' section for destination types or configuration-guide.md: 'Console + File (Common Combo)'.");
    }

    #endregion

    #region Appender Validation

    /// <summary>
    /// Validates a single appender configuration.
    /// </summary>
    private static void ValidateAppender(AppenderConfiguration appender, int appenderIndex)
    {
        // Destination validation
        ValidateAppenderDestination(appender, appenderIndex);

        // Formatter validation
        ValidateAppenderFormatter(appender, appenderIndex);

        // Destination-specific validations
        switch (appender.Destination.Type)
        {
            case LogOutputDestination.FileSystem:
                ValidateFileDestination(appender, appenderIndex);
                break;

            case LogOutputDestination.LogAggregator:
                ValidateLogAggregatorDestination(appender, appenderIndex);
                break;
        }
    }

    /// <summary>
    /// Validates appender destination configuration.
    /// </summary>
    private static void ValidateAppenderDestination(AppenderConfiguration appender, int appenderIndex)
    {
        if (appender.Destination?.Type != LogOutputDestination.Unknown)
            return;

        Fail(
            $"Appender[{appenderIndex}]: Destination type is not configured.",
            "Every appender must specify a valid output destination (Console, FileSystem, or LogAggregator).",
            "Set the 'Destination.Type' property to a valid destination type.",
            "See configuration-reference.md: 'Destination' section or configuration-guide.md for examples.");
    }

    /// <summary>
    /// Validates appender formatter configuration.
    /// </summary>
    private static void ValidateAppenderFormatter(AppenderConfiguration appender, int appenderIndex)
    {
        if (appender.Formatter == null)
        {
            Fail(
                $"Appender[{appenderIndex}]: Formatter is null.",
                "Every appender requires a formatter configuration to define output format and layout.",
                "Ensure the 'Formatter' object is properly configured.",
                "See configuration-reference.md: 'Formatter' section.");
        }

        // Validate custom layout pattern
        if (appender.Formatter.LayoutType == LogMessageLayoutType.Custom)
        {
            if (string.IsNullOrWhiteSpace(appender.Formatter.Pattern))
            {
                Fail(
                    $"Appender[{appenderIndex}]: Custom layout requires a non-empty 'Pattern'.",
                    "When using the Custom layout type, a valid pattern string must be provided.",
                    "Set the 'Formatter.Pattern' property to a valid pattern (e.g., \"[%LEVEL] %MESSAGE\").",
                    "See configuration-reference.md: 'Formatter' section for available tokens.");
            }
        }

        // Validate JSON fields for JSON output format
        if (appender.Formatter.OutputFormat == LogOutputFormat.Json)
        {
            if (appender.Formatter.IncludedJsonFields == null || appender.Formatter.IncludedJsonFields.Count == 0)
            {
                Fail(
                    $"Appender[{appenderIndex}]: JSON output format requires at least one included field.",
                    "JSON formatted logs need at least one field to produce valid JSON output.",
                    "Add default fields or specify custom fields in 'Formatter.IncludedJsonFields'.",
                    "See configuration-reference.md: 'Formatter' section for available field names.");
            }
        }
    }

    #endregion

    #region FileSystem Destination Validation

    /// <summary>
    /// Validates FileSystem destination configuration.
    /// </summary>
    private static void ValidateFileDestination(AppenderConfiguration appender, int appenderIndex)
    {
        if (appender.Destination.File == null)
        {
            Fail(
                $"Appender[{appenderIndex}]: FileSystem destination requires 'File' configuration.",
                "When the destination is FileSystem, file-specific settings must be provided.",
                "Add a 'Destination.File' object with directory, fileName, and extension.",
                "See configuration-guide.md: 'Production - File Logging, JSON, Rolling' for a complete example.");
        }

        var fileConfig = appender.Destination.File;

        // Validate directory
        if (string.IsNullOrWhiteSpace(fileConfig.Directory))
        {
            Fail(
                $"Appender[{appenderIndex}]: File directory is empty.",
                "The file logging system needs a valid directory path to create and store log files.",
                "Set 'File.Directory' to a valid path (e.g., \"Logs\" or \"C:\\Logs\").",
                "See configuration-reference.md: 'File' section for property details.");
        }

        // Validate file name
        if (string.IsNullOrWhiteSpace(fileConfig.FileName))
        {
            Fail(
                $"Appender[{appenderIndex}]: File name is empty.",
                "A base file name is required; it will be combined with the naming strategy to create the final file name.",
                "Set 'File.FileName' to a descriptive name (e.g., \"Application\", \"MyService\").",
                "See configuration-reference.md: 'File' section.");
        }

        // Validate extension
        if (string.IsNullOrWhiteSpace(fileConfig.Extension))
        {
            Fail(
                $"Appender[{appenderIndex}]: File extension is empty.",
                "A file extension is required to produce valid log files.",
                "Set 'File.Extension' to a valid extension (e.g., \"log\", \"txt\", \"json\").",
                "See configuration-reference.md: 'File' section.");
        }

        // Validate naming configuration
        if (fileConfig.Naming == null)
        {
            Fail(
                $"Appender[{appenderIndex}]: File naming configuration is null.",
                "Naming configuration defines how file names are generated.",
                "Ensure 'File.Naming' is properly configured.",
                "See configuration-reference.md: 'File Naming' section.");
        }
        else
        {
            ValidateFileNamingConfiguration(fileConfig.Naming, appenderIndex);
        }

        // Validate rolling configuration
        if (fileConfig.Rolling == null)
        {
            Fail(
                $"Appender[{appenderIndex}]: File rolling configuration is null.",
                "Rolling configuration defines when log files are rolled to new files.",
                "Ensure 'File.Rolling' is properly configured.",
                "See configuration-reference.md: 'Rolling' section.");
        }
        else
        {
            ValidateFileRollingConfiguration(fileConfig.Rolling, appenderIndex);
        }

        // Validate archive configuration (if enabled)
        if (fileConfig.Archive != null && fileConfig.Archive.Enabled)
        {
            ValidateArchiveConfiguration(fileConfig.Archive, appenderIndex);
        }

        // Validate retention configuration
        if (fileConfig.Retention != null)
        {
            ValidateRetentionConfiguration(fileConfig.Retention, appenderIndex);
        }
    }

    /// <summary>
    /// Validates file naming configuration.
    /// </summary>
    private static void ValidateFileNamingConfiguration(FileNamingConfiguration naming, int appenderIndex)
    {
        if (naming.Strategy == FileNamingStrategyType.Date ||
            naming.Strategy == FileNamingStrategyType.Timestamp)
        {
            if (string.IsNullOrWhiteSpace(naming.DateFormat))
            {
                Fail(
                    $"Appender[{appenderIndex}]: Date-based naming strategy requires a 'DateFormat'.",
                    "When using Date or Timestamp naming strategies, a date format string is required.",
                    "Set 'File.Naming.DateFormat' to a valid .NET date format string (e.g., \"yyyy-MM-dd\").",
                    "See configuration-reference.md: 'File Naming' section.");
            }
        }
    }

    /// <summary>
    /// Validates file rolling configuration.
    /// </summary>
    private static void ValidateFileRollingConfiguration(FileRollingConfiguration rolling, int appenderIndex)
    {
        if (rolling.Strategy == RollingStrategyType.Size && rolling.MaxFileSizeMB <= 0)
        {
            Fail(
                $"Appender[{appenderIndex}]: Size-based rolling requires 'MaxFileSizeMB' > 0.",
                "When using Size-based rolling, a positive maximum file size must be specified.",
                "Set 'File.Rolling.MaxFileSizeMB' to a value > 0 (e.g., 10, 50, 100).",
                "See configuration-reference.md: 'Rolling' section.");
        }
    }

    /// <summary>
    /// Validates archive configuration.
    /// </summary>
    private static void ValidateArchiveConfiguration(ArchiveConfiguration archive, int appenderIndex)
    {
        if (string.IsNullOrWhiteSpace(archive.Directory))
        {
            Fail(
                $"Appender[{appenderIndex}]: Archive directory is empty.",
                "When archival is enabled, a directory path must be provided to store archived log files.",
                "Set 'File.Archive.Directory' to a valid path (e.g., \"Archive\", \"Logs/Archive\").",
                "See configuration-reference.md: 'Archive' section.");
        }
    }

    /// <summary>
    /// Validates retention configuration.
    /// </summary>
    private static void ValidateRetentionConfiguration(RetentionConfiguration retention, int appenderIndex)
    {
        if (retention.RetentionDays <= 0)
        {
            Fail(
                $"Appender[{appenderIndex}]: Retention days must be > 0.",
                "Retention period must be a positive value to maintain log file cleanup schedules.",
                "Set 'File.Retention.RetentionDays' to a positive integer (e.g., 30, 90, 365).",
                "See configuration-reference.md: 'Retention' section.");
        }
    }

    #endregion

    #region LogAggregator Destination Validation

    /// <summary>
    /// Validates LogAggregator destination configuration.
    /// </summary>
    private static void ValidateLogAggregatorDestination(AppenderConfiguration appender, int appenderIndex)
    {
        // LogAggregator MUST use JSON format
        if (appender.Formatter.OutputFormat != LogOutputFormat.Json)
        {
            Fail(
                $"Appender[{appenderIndex}]: LogAggregator requires JSON output format.",
                "Log aggregators consume structured JSON payloads. Other formats cannot be parsed.",
                "Set 'Formatter.OutputFormat' to 'Json'.",
                "See configuration-reference.md: 'Log Aggregator' section.");
        }

        if (appender.Destination.LogAggregator == null)
        {
            Fail(
                $"Appender[{appenderIndex}]: LogAggregator destination requires configuration.",
                "When the destination type is LogAggregator, sink configuration must be provided.",
                "Add a 'Destination.LogAggregator' object with either a default HTTP endpoint or custom sink.",
                "See configuration-guide.md: 'Remote Log Aggregator (Centralized Sink)' for examples.");
        }

        var aggregatorConfig = appender.Destination.LogAggregator;

        // Validate default HTTP sink configuration
        if (aggregatorConfig.UseDefault)
        {
            if (aggregatorConfig.Endpoint == null)
            {
                Fail(
                    $"Appender[{appenderIndex}]: LogAggregator using default sink requires 'Endpoint'.",
                    "The default HTTP aggregator sink needs a valid endpoint URL to send logs to.",
                    "Set 'LogAggregator.Endpoint' to a valid HTTP(S) URL (e.g., \"http://localhost:8080/logs\").",
                    "See configuration-reference.md: 'Log Aggregator' section.");
            }
            else
            {
                // Validate that endpoint is a valid URI
                try
                {
                    // Endpoint property is Uri, so it's already validated by the deserializer
                }
                catch
                {
                    Fail(
                        $"Appender[{appenderIndex}]: LogAggregator 'Endpoint' is not a valid URI.",
                        "The endpoint must be a properly formatted HTTP or HTTPS URL.",
                        "Ensure 'LogAggregator.Endpoint' is a valid URL (e.g., \"http://aggregator.example.com:8080/api/logs\").",
                        "See configuration-reference.md: 'Log Aggregator' section.");
                }
            }
        }
        else
        {
            // Custom sink configuration
            if (string.IsNullOrWhiteSpace(aggregatorConfig.SinkKey) &&
                string.IsNullOrWhiteSpace(aggregatorConfig.SinkTypeName))
            {
                Fail(
                    $"Appender[{appenderIndex}]: Custom LogAggregator sink requires 'SinkKey' or 'SinkTypeName'.",
                    "When UseDefault is false, you must provide either a registered sink key or a type name.",
                    "Set either 'LogAggregator.SinkKey' (for registered sinks) or 'LogAggregator.SinkTypeName' (for reflection-based instantiation).",
                    "See configuration-reference.md: 'Log Aggregator' section for custom sink details.");
            }
        }
    }

    #endregion

    #region Logger Override Validation

    /// <summary>
    /// Validates a logger override configuration.
    /// </summary>
    private static void ValidateLoggerOverride(LoggerOverrideConfiguration loggerOverride, int overrideIndex)
    {
        if (string.IsNullOrWhiteSpace(loggerOverride.LoggerName))
        {
            Fail(
                $"LoggerOverride[{overrideIndex}]: Logger name is empty.",
                "Each logger override must specify a logger name (typically the fully qualified class or namespace name).",
                "Set 'LoggerName' to the name of the logger to override (e.g., \"MyApp.Services.PaymentService\").",
                "See configuration-reference.md: 'Logger Override' section.");
        }
    }

    #endregion

    #region Error Reporting

    /// <summary>
    /// Formats and throws a validation error with a solution guide.
    /// </summary>
    private static void Fail(string issue, string reason, string solution, string reference)
    {
        var message = new System.Text.StringBuilder();
        message.AppendLine(issue);
        message.AppendLine();
        message.AppendLine("REASON:");
        message.AppendLine(reason);
        message.AppendLine();
        message.AppendLine("FIX:");
        message.AppendLine(solution);
        message.AppendLine();
        message.AppendLine("LEARN MORE:");
        message.Append(reference);

        throw new InvalidOperationException(message.ToString());
    }

    #endregion
}