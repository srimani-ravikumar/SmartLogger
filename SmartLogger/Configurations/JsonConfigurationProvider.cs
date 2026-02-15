using SmartLogger.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLogger.Configurations;

public class JsonConfigurationProvider : ILogConfigurationProvider
{
    private readonly string _filePath;
    private FileSystemWatcher? _watcher;
    private readonly object _reloadLock = new();

    public JsonConfigurationProvider(string filePath)
    {
        if(string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath)); 
        }

        if (Path.IsPathRooted(filePath))
            _filePath = filePath;
        else
            _filePath = Path.Combine(AppContext.BaseDirectory, filePath);

    }

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

    public void EnableAutoReload()
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
