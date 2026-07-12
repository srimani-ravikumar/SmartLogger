namespace SmartLogger.Appenders.FileNaming;


/// <summary>
/// Defines the contract for generating log file names.
/// </summary>
/// <remarks>
/// A naming strategy is responsible only for constructing file names.
/// It does not determine where files are stored or when files are rolled.
/// </remarks>
public interface IFileNamingStrategy
{
    /// <summary>
    /// Creates the active log file name.
    /// </summary>
    /// <returns>The active log file name.</returns>
    string CreateActiveFileName();

    /// <summary>
    /// Creates a rolled log file name.
    /// </summary>
    /// <param name="index">
    /// Optional rolling index used when multiple files
    /// are generated within the same rolling window.
    /// </param>
    /// <returns>The rolled log file name.</returns>
    string CreateRolledFileName(int index = 0);
}
