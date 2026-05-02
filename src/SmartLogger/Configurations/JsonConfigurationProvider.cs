using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace SmartLogger.Configurations;

/// <summary>
/// Loads SmartLogger configuration from a JSON file and optionally supports
/// automatic reloading when the file changes.
/// </summary>
/// <remarks>
/// Features:
/// <list type="bullet">
/// <item><description>Flexible JSON parsing (case-insensitive, comments allowed)</description></item>
/// <item><description>Strong validation for configuration correctness</description></item>
/// <item><description>Optional file-watcher based hot reload</description></item>
/// </list>
/// 
/// Auto Reload Behavior:
/// <list type="bullet">
/// <item><description>Triggered on file changes detected by <see cref="FileSystemWatcher"/></description></item>
/// <item><description>Reload is synchronized to prevent concurrent updates</description></item>
/// <item><description>Reload failures do not corrupt existing configuration</description></item>
/// </list>
/// </remarks>
public sealed class JsonConfigurationProvider : ILogConfigurationProvider
{
    /// <summary>
    /// Absolute path to the configuration file.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// File watcher used for detecting configuration changes.
    /// </summary>
    private FileSystemWatcher _watcher;

    /// <summary>
    /// Synchronization primitive to prevent concurrent reload attempts.
    /// </summary>
    private readonly object _reloadLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigurationProvider"/> class.
    /// </summary>
    /// <param name="filePath">Relative or absolute path to the JSON configuration file.</param>
    /// <param name="enableAutoReload">Indicates whether automatic reload on file change is enabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is invalid.</exception>
    public JsonConfigurationProvider(string filePath, bool enableAutoReload)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Invalid file path provided!");

        // Normalize path (relative → absolute)
        _filePath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(AppContext.BaseDirectory, filePath);

        if (enableAutoReload)
            EnableAutoReload();
    }

    // Private to enforce explicit file path usage
    private JsonConfigurationProvider() { }

    /// <inheritdoc/>
    public LogConfigurationHolder Load()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"SmartLogger configuration file not found at path: {_filePath}");
        }

        var json = File.ReadAllText(_filePath);

        // Flexible JSON parsing options
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
            }
        };

        var configuration = JsonSerializer.Deserialize<LogConfigurationHolder>(json, options);

        if (configuration is null)
        {
            throw new InvalidOperationException(
                "Failed to deserialize SmartLogger configuration.");
        }

        // Validate structural correctness
        Validate(configuration);

        return configuration;
    }

    /// <summary>
    /// Validates the configuration for correctness and consistency.
    /// </summary>
    /// <param name="config">Configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private static void Validate(LogConfigurationHolder config)
    {
        // Detect duplicate destinations (except Unknown)
        var duplicateDestinations = config.Appenders
            .GroupBy(a => a.Destination.Type)
            .Where(g => g.Count() > 1 && g.Key != LogOutputDestination.Unknown)
            .Select(g => g.Key)
            .ToList();

        if (duplicateDestinations.Any())
        {
            string destinations = string.Join(", ", duplicateDestinations);

            throw new InvalidOperationException(
                $"Duplicate appender destinations detected: {destinations}. " +
                "Each destination should be configured only once.\n\n" +
                "Suggested fixes:\n" +
                "- Merge configurations into a single appender\n" +
                "- Or use different destinations (e.g., Console + FileSystem)"
            );
        }

        foreach (var appenderConfig in config.Appenders)
        {
            // Validate destination type
            if (appenderConfig.Destination.Type == LogOutputDestination.Unknown)
            {
                throw new InvalidOperationException(
                    "Appender destination must be specified.\n" +
                    "Tip: Use 'Console' or 'FileSystem' to get started."
                );
            }

            // Validate file configuration for file-based appenders
            if (appenderConfig.Destination.Type == LogOutputDestination.FileSystem)
            {
                if (string.IsNullOrWhiteSpace(appenderConfig.Destination.File.BasePath))
                {
                    throw new InvalidOperationException(
                        "FileSystem appender requires a valid 'filePath' in settings.\n" +
                        "Example:\n" +
                        "\"settings\": { \"filePath\": \"logs/app.log\" }"
                    );
                }
            }

            // Validate custom layout requirements
            if (appenderConfig.Formatter.LayoutType == LogMessageLayoutType.Custom &&
                string.IsNullOrWhiteSpace(appenderConfig.Formatter.Pattern))
            {
                throw new InvalidOperationException(
                    "Custom layout requires a non-empty 'pattern'.\n" +
                    "Example:\n" +
                    "\"pattern\": \"[%LEVEL] %MESSAGE\""
                );
            }
        }
    }

    /// <summary>
    /// Enables automatic reloading when the configuration file changes.
    /// </summary>
    private void EnableAutoReload()
    {
        string directory = Path.GetDirectoryName(_filePath)!;
        string fileName = Path.GetFileName(_filePath);

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite |
                           NotifyFilters.Size |
                           NotifyFilters.CreationTime
        };

        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Handles file change events and triggers configuration reload.
    /// </summary>
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // FileSystemWatcher can fire multiple times for a single change
        lock (_reloadLock)
        {
            try
            {
                // Small delay to avoid partial file reads / file locks
                // TODO: Wait until the queue empties in real world scenerio
                Thread.Sleep(100);

                var newConfig = Load();

                // Apply updated configuration
                LoggerManager.ReloadConfiguration(this);
            }
            catch
            {
                // Intentionally suppressed to avoid crashing watcher thread
                // (logging can be added here if needed)
            }
        }
    }
}