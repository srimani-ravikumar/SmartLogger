using SmartLogger.Core;
using System;
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
        if (config.Appenders is null || !config.Appenders.Any())
        {
            throw new InvalidOperationException(
                "At least one appender must be configured.");
        }

        foreach (var appender in config.Appenders)
        {
            if (appender.Destination == LogOutputDestination.Unknown)
            {
                throw new InvalidOperationException(
                    "Appender destination must be specified.");
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

                LoggerManager.Reload(this);

                Console.WriteLine("[SmartLogger] Configuration reloaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartLogger] Failed to reload configuration: {ex.Message}");
            }
        }
    }

}
