namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Defines the contract for log file rolling strategies.
/// </summary>
/// <remarks>
/// Implementations determine:
/// <list type="bullet">
/// <item>
/// <description>When a log file should be rolled (rotation trigger)</description>
/// </item>
/// <item>
/// <description>How the next file path is generated</description>
/// </item>
/// <item>
/// <description>Any post-roll actions (e.g., cleanup, archiving)</description>
/// </item>
/// </list>
/// 
/// This abstraction allows multiple rolling policies 
/// (e.g., size-based, time-based) without modifying the appender logic.
/// </remarks>
public interface IRollingStrategy
{
    /// <summary>
    /// Determines whether the current log file should be rolled.
    /// </summary>
    /// <param name="filePath">The current log file path.</param>
    /// <returns><c>true</c> if rolling should occur; otherwise, <c>false</c>.</returns>
    bool ShouldRoll(string filePath);

    /// <summary>
    /// Generates the next file path after a roll operation.
    /// </summary>
    /// <param name="basePath">The base file path defined in configuration.</param>
    /// <returns>The file path to be used for the new log file.</returns>
    string GetNextFilePath(string basePath);

    /// <summary>
    /// Executes post-roll actions after a file has been rotated.
    /// </summary>
    /// <param name="currentFilePath">The file path that was just rolled.</param>
    /// <remarks>
    /// Can be used for cleanup, archiving, or triggering external processes.
    /// </remarks>
    void OnRoll(string currentFilePath);
}