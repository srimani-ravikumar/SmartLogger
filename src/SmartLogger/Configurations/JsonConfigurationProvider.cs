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
/// Loads SmartLogger configuration from a JSON file
/// and optionally supports real-time auto-reloading
/// when the file changes.
/// </summary>
public sealed class JsonConfigurationProvider : ILogConfigurationProvider
{
    private readonly string _filePath;
    private FileSystemWatcher _watcher;
    private readonly object _reloadLock = new();

    /// <summary>
    /// Initializes a new instance of <see cref="JsonConfigurationProvider"/>
    /// with the specified configuration file path.
    /// </summary>
    /// <param name="filePath">Relative or absolute path to the JSON configuration file.</param>
    /// <param name="enableAutoReload">Determines the configuration change nature</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the file path is null or empty.
    /// </exception>
    public JsonConfigurationProvider(string filePath, bool enableAutoReload)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException("Invalid file path provided!");


        if (Path.IsPathRooted(filePath))
            _filePath = filePath;
        else
            _filePath = Path.Combine(AppContext.BaseDirectory, filePath);

        if (enableAutoReload)
            EnableAutoReload();

    }

    // Making it private to mandate providing filepath during construction
    private JsonConfigurationProvider()
    {
    }

    /// <inheritdoc/>
    public LogConfigurationHolder Load()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"SmartLogger configuration file not found at path: {_filePath}");
        }

        var json = File.ReadAllText(_filePath);

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


        Validate(configuration);

        return configuration;
    }

    private static void Validate(LogConfigurationHolder config)
    {
        List<LogOutputDestination> duplicateDestinations = config.Appenders.GroupBy(a => a.Destination)
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

        foreach (var appender in config.Appenders)
        {
            if (appender.Destination == LogOutputDestination.Unknown)
            {
                throw new InvalidOperationException(
                    "Appender destination must be specified. \n" +
                    "Tip: Use 'Console' or 'FileSystem' to get started."
                );
            }

            // validate file path for FileSystem
            if (appender.Destination == LogOutputDestination.FileSystem)
            {
                if (string.IsNullOrWhiteSpace(appender.File.BasePath))
                {
                    throw new InvalidOperationException(
                        "FileSystem appender requires a valid 'filePath' in settings.\n" +
                        "Example:\n" +
                        "\"settings\": { \"filePath\": \"logs/app.log\" }"
                    );
                }
            }

            // validate custom layout
            if (appender.LayoutType == LogMessageLayoutType.Custom &&
                string.IsNullOrWhiteSpace(appender.Pattern))
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
    /// Enables automatic reloading of the configuration
    /// when the underlying JSON file changes.
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

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // FileSystemWatcher may fire multiple times
        lock (_reloadLock)
        {
            try
            {
                // Small delay to avoid file lock issues
                Thread.Sleep(100);

                var newConfig = Load();

                LoggerManager.ReloadConfiguration(this);

                // Console.WriteLine("[SmartLogger] Configuration reloaded successfully.");
            }
            catch (Exception ex)
            {
                // TO BE REVISITED
                throw;
                // Console.WriteLine($"[SmartLogger] Failed to reload configuration: {ex.Message}");
            }
        }
    }

}
