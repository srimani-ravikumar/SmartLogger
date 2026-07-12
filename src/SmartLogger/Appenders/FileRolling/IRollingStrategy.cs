namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Defines the contract for determining when a log file should be rolled.
/// </summary>
/// <remarks>
/// A rolling strategy is responsible only for deciding whether
/// the current log file should be rotated.
///
/// It does not perform the roll operation,
/// generate file names,
/// archive files,
/// or compress files.
/// </remarks>
public interface IRollingStrategy
{
    /// <summary>
    /// Determines whether the current log file should be rolled.
    /// </summary>
    /// <param name="activeFilePath">
    /// Full path of the active log file.
    /// </param>
    /// <returns>
    /// <c>true</c> if the file should be rolled;
    /// otherwise <c>false</c>.
    /// </returns>
    bool ShouldRoll(string activeFilePath);
}