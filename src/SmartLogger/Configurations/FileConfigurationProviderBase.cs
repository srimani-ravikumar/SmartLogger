using SmartLogger.Core;
using System;
using System.IO;
using System.Threading;

namespace SmartLogger.Configurations;

/// <summary>
/// Base implementation for all file-based configuration providers.
/// </summary>
/// <remarks>
/// Handles:
/// <list type="bullet">
/// <item><description>File path resolution (relative → absolute)</description></item>
/// <item><description>Automatic configuration reload via <see cref="FileSystemWatcher"/></description></item>
/// <item><description>File change detection and reload synchronization</description></item>
/// <item><description>Configuration validation via <see cref="ConfigurationValidator"/></description></item>
/// </list> 
/// Derived classes are only responsible for deserializing
/// their respective configuration format.
/// </remarks>
public abstract class FileConfigurationProviderBase : ILogConfigurationProvider
{
    /// <summary>
    /// Absolute path to the configuration file.
    /// </summary>
    protected readonly string FilePath;

    /// <summary>
    /// File watcher used for automatic configuration reload.
    /// </summary>
    private FileSystemWatcher? _watcher;

    /// <summary>
    /// Prevents concurrent reload attempts.
    /// </summary>
    private readonly object _reloadLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileConfigurationProviderBase"/> class.
    /// </summary>
    /// <param name="filePath">
    /// Relative or absolute configuration file path.
    /// </param>
    /// <param name="enableAutoReload">
    /// Enables automatic reload when the configuration file changes.
    /// </param>
    /// <exception cref="ArgumentNullException"/>
    protected FileConfigurationProviderBase(string filePath, bool enableAutoReload)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Invalid configuration file path.");

        FilePath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(AppContext.BaseDirectory, filePath);

        if (enableAutoReload)
            EnableAutoReload();
    }

    /// <inheritdoc/>
    public LogConfigurationHolder Load()
    {
        if (!File.Exists(FilePath))
        {
            throw new FileNotFoundException($"SmartLogger configuration file not found: {FilePath}");
        }

        var configuration = Deserialize(FilePath);

        if (configuration is null)
        {
            throw new InvalidOperationException("Failed to load SmartLogger configuration.");
        }

        ConfigurationValidator.Validate(configuration);

        return configuration;
    }

    /// <summary>
    /// Deserializes the configuration file into <see cref="LogConfigurationHolder"/>.
    /// </summary>
    protected abstract LogConfigurationHolder Deserialize(string filePath);

    /// <summary>
    /// Enables automatic configuration reload.
    /// </summary>
    private void EnableAutoReload()
    {
        string directory = Path.GetDirectoryName(FilePath)!;
        string fileName = Path.GetFileName(FilePath);

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter =
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.CreationTime
        };

        _watcher.Changed += OnConfigurationFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Handles configuration file changes.
    /// </summary>
    private void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (_reloadLock)
        {
            try
            {
                // FileSystemWatcher may raise multiple events
                // while the file is being written.
                Thread.Sleep(100);

                LoggerManager.ReloadConfiguration(this);
            }
            catch
            {
                // Intentionally ignored.
                // Existing configuration remains active.
            }
        }
    }
}