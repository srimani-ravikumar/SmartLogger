using SmartLogger.Core;
using System;
using System.Linq;

namespace SmartLogger.Configurations;

/// <summary>
/// Validates SmartLogger configuration for correctness and consistency.
///
/// All configuration providers should invoke this validator before
/// returning the loaded configuration.
/// </summary>
internal static class ConfigurationValidator
{
    /// <summary>
    /// Validates the supplied logging configuration.
    /// </summary>
    /// <param name="configuration">
    /// Configuration to validate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when configuration is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration contains invalid settings.
    /// </exception>
    internal static void Validate(LogConfigurationHolder configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ValidateDuplicateDestinations(configuration);

        foreach (var appender in configuration.Appenders)
        {
            ValidateDestination(appender);

            ValidateFileConfiguration(appender);

            ValidateCustomLayout(appender);
        }
    }

    /// <summary>
    /// Ensures that each output destination is configured only once.
    /// </summary>
    private static void ValidateDuplicateDestinations(
        LogConfigurationHolder configuration)
    {
        var duplicateDestinations = configuration.Appenders
            .GroupBy(appender => appender.Destination.Type)
            .Where(group =>
                group.Key != LogOutputDestination.Unknown &&
                group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (!duplicateDestinations.Any())
            return;

        string destinations = string.Join(", ", duplicateDestinations);

        throw new InvalidOperationException(
            $"Duplicate appender destinations detected: {destinations}.\n\n" +

            "Reason:\n" +
            "SmartLogger allows only one appender for each output destination.\n\n" +

            "Suggested fixes:\n" +
            "- Merge both configurations into a single appender.\n" +
            "- Or configure different destinations (e.g., Console + FileSystem).");
    }

    /// <summary>
    /// Validates the configured output destination.
    /// </summary>
    private static void ValidateDestination(
        AppenderConfiguration appender)
    {
        if (appender.Destination.Type != LogOutputDestination.Unknown)
            return;

        throw new InvalidOperationException(
            "Appender destination is missing.\n\n" +

            "Reason:\n" +
            "Every appender must define where log messages should be written.\n\n" +

            "Suggested fix:\n" +
            "Configure a valid output destination such as Console or FileSystem.\n\n" +

            "Example:\n" +
            "\"Destination\":\n" +
            "{\n" +
            "  \"Type\": \"Console\"\n" +
            "}");
    }

    /// <summary>
    /// Validates file-specific configuration.
    /// </summary>
    private static void ValidateFileConfiguration(
        AppenderConfiguration appender)
    {
        if (appender.Destination.Type != LogOutputDestination.FileSystem)
            return;

        if (!string.IsNullOrWhiteSpace(appender.Destination.File?.BasePath))
            return;

        throw new InvalidOperationException(
            "FileSystem appender requires a valid 'BasePath'.\n\n" +

            "Reason:\n" +
            "The framework cannot create or write log files without a target file path.\n\n" +

            "Suggested fix:\n" +
            "Specify a valid BasePath for the FileSystem appender.\n\n" +

            "Example:\n" +
            "\"File\":\n" +
            "{\n" +
            "  \"BasePath\": \"logs/app\",\n" +
            "  \"Extension\": \"log\"\n" +
            "}");
    }

    /// <summary>
    /// Validates custom formatter configuration.
    /// </summary>
    private static void ValidateCustomLayout(
        AppenderConfiguration appender)
    {
        if (appender.Formatter.LayoutType != LogMessageLayoutType.Custom)
            return;

        if (!string.IsNullOrWhiteSpace(appender.Formatter.Pattern))
            return;

        throw new InvalidOperationException(
            "Custom layout requires a non-empty 'Pattern'.\n\n" +

            "Reason:\n" +
            "The formatter is configured to use the Custom layout, but no pattern was provided.\n\n" +

            "Suggested fix:\n" +
            "Specify a valid pattern or change the layout type to Simple or Detailed.\n\n" +

            "Example:\n" +
            "\"Pattern\": \"[%LEVEL] %MESSAGE\"");
    }
}