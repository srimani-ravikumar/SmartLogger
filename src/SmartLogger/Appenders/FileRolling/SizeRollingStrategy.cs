using SmartLogger.Core;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Rolling strategy based on file size.
/// </summary>
/// <remarks>
/// Triggers a roll when the current log file exceeds the configured size limit.
/// New files are created with incrementing indices to avoid overwriting existing logs.
/// </remarks>
internal class SizeRollingStrategy : IRollingStrategy
{
    /// <summary>
    /// Maximum allowed file size in bytes before triggering a roll.
    /// </summary>
    private readonly long _maxBytes;

    /// <summary>
    /// Responsible for constructing rolled file names.
    /// </summary>
    private readonly FileNameBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeRollingStrategy"/> class.
    /// </summary>
    /// <param name="fileConfig">Configuration containing rolling policy and naming rules.</param>
    public SizeRollingStrategy(FileConfiguration fileConfig)
    {
        // Convert MB → bytes for precise size comparison
        _maxBytes = fileConfig.RollingPolicy.MaxFileSizeMB * 1024 * 1024;

        _builder = new FileNameBuilder(fileConfig);
    }

    /// <summary>
    /// Determines whether the current file should be rolled based on its size.
    /// </summary>
    /// <param name="filePath">The current log file path.</param>
    /// <returns>
    /// <c>true</c> if the file exists and its size exceeds the configured limit;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool ShouldRoll(string filePath)
    {
        // If file doesn't exist, nothing to roll yet
        if (!File.Exists(filePath)) return false;

        var info = new FileInfo(filePath);

        // Trigger roll when size threshold is reached or exceeded
        return info.Length >= _maxBytes;
    }

    /// <summary>
    /// Generates the next available file path using an incrementing index.
    /// </summary>
    /// <param name="basePath">The base file path defined in configuration (not directly used).</param>
    /// <returns>A unique file path that does not collide with existing files.</returns>
    /// <remarks>
    /// Iteratively increments an index until a non-existing file name is found.
    /// Ensures no existing log files are overwritten.
    /// </remarks>
    public string GetNextFilePath(string basePath)
    {
        int index = 1;
        string newPath;

        // Find the first available file name that does not already exist
        do
        {
            newPath = _builder.Build(index);
            index++;
        }
        while (File.Exists(newPath));

        return Path.GetFullPath(newPath);
    }

    /// <summary>
    /// Executes post-roll actions.
    /// </summary>
    /// <param name="currentFilePath">The file path that was just rolled.</param>
    /// <remarks>
    /// No-op for size-based rolling.
    /// Hook provided for future extensibility (e.g., archiving, compression).
    /// </remarks>
    /// TODO: Expose on roll logic hook to clients but have default logic of moving to subdirectory
    public void OnRoll(string currentFilePath) { }
}