using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Appenders.FileSystem;

internal sealed class FileLifecycleManager
{
    private readonly FileConfiguration _configuration;
    private readonly IRollingStrategy? _rollingStrategy;
    private readonly IFileNamingStrategy _namingStrategy;

    private readonly object _syncRoot = new();

    private readonly string _activeDirectory;
    private readonly string _archiveDirectory;

    private string _activeFilePath;

    internal FileLifecycleManager(
        FileConfiguration configuration,
        IRollingStrategy? rollingStrategy,
        IFileNamingStrategy namingStrategy)
    {
        _configuration = configuration
            ?? throw new ArgumentNullException(nameof(configuration));

        _rollingStrategy = rollingStrategy;

        _namingStrategy = namingStrategy
            ?? throw new ArgumentNullException(nameof(namingStrategy));

        _activeDirectory = configuration.Directory;
        _archiveDirectory = configuration.Archive.Directory;

        EnsureDirectoriesExist();

        CreateFreshActiveFile();
    }

    /// <summary>
    /// Writes a formatted log message to the active log file.
    /// </summary>
    public void Write(string formattedMessage)
    {
        if (string.IsNullOrWhiteSpace(formattedMessage))
            return;

        lock (_syncRoot)
        {
            EnsureActiveFile();

            WriteToActiveFile(formattedMessage);
        }
    }

    /// <summary>
    /// Ensures an active log file is available for writing.
    /// </summary>
    private void EnsureActiveFile()
    {
        if (!File.Exists(_activeFilePath))
        {
            CreateFreshActiveFile();
            return;
        }

        if (_rollingStrategy == null)
            return;

        if (!_rollingStrategy.ShouldRoll(_activeFilePath))
            return;

        ArchiveActiveFile();

        CleanupArchives();

        CreateFreshActiveFile();
    }

    /// <summary>
    /// Creates a fresh active log file.
    /// </summary>
    private void CreateFreshActiveFile()
    {
        _activeFilePath = CreateActiveFilePath();

        if (!File.Exists(_activeFilePath))
        {
            using var file = File.Create(_activeFilePath);
        }
    }

    /// <summary>
    /// Moves the current active log file into the archive location.
    /// </summary>
    private void ArchiveActiveFile()
    {
        if (!File.Exists(_activeFilePath))
            return;

        var archivePath = CreateUniqueArchiveFilePath();

        File.Move(
            _activeFilePath,
            archivePath);

        CompressArchive(archivePath);
    }

    /// <summary>
    /// Writes the formatted message to the active file.
    /// </summary>
    private void WriteToActiveFile(string formattedMessage)
    {
        File.AppendAllText(
            _activeFilePath,
            formattedMessage + Environment.NewLine);
    }

    /// <summary>
    /// Creates the active log file path.
    /// </summary>
    private string CreateActiveFilePath()
    {
        return Path.Combine(
            _activeDirectory,
            _namingStrategy.CreateActiveFileName());
    }

    /// <summary>
    /// Creates a unique archive file path.
    /// </summary>
    private string CreateUniqueArchiveFilePath()
    {
        var index = 0;

        string archivePath;

        do
        {
            archivePath = Path.Combine(
                _archiveDirectory,
                _namingStrategy.CreateRolledFileName(index));

            index++;

        } while (File.Exists(archivePath));

        return archivePath;
    }

    /// <summary>
    /// Compresses an archived log file.
    /// </summary>
    private void CompressArchive(string archiveFilePath)
    {
        if (!_configuration.Archive.Enabled)
            return;

        if (!_configuration.Archive.Compress)
            return;

        CompressionHelper.Compress(archiveFilePath);
    }

    /// <summary>
    /// Removes expired archived log files.
    /// </summary>
    private void CleanupArchives()
    {
        RetentionHelper.Cleanup(_configuration);
    }

    /// <summary>
    /// Ensures all required directories exist.
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_activeDirectory);

        if (_configuration.Archive.Enabled)
        {
            Directory.CreateDirectory(_archiveDirectory);
        }
    }
}