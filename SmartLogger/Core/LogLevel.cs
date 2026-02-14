namespace SmartLogger.Core;

/// <summary>
/// Defines the severity levels for log messages.
/// </summary>
public enum LogLevel
{
    /// <summary> 
    /// Detailed information for debugging and development. 
    /// </summary>
    DEBUG = 1,

    /// <summary> 
    /// General operational messages about application progress. 
    /// </summary>
    INFO = 2,

    /// <summary> 
    /// Non-critical issues that do not interrupt the flow. 
    /// </summary>
    WARNING = 3,

    /// <summary> 
    /// Significant issues that failed a specific operation. 
    /// </summary>
    ERROR = 4,

    /// <summary> 
    /// Critical failures requiring immediate attention. 
    /// </summary>
    FATAL = 5

}

/// <summary>
/// Extension methods for LogLevel to provide Java-like comparison logic.
/// </summary>
public static class LogLevelExtensions
{
    /// <summary>
    /// Checks if this level is greater than or equal to the other level.
    /// </summary>
    /// <param name="current">The level being checked.</param>
    /// <param name="other">The level to compare against.</param>
    /// <returns>True if the current priority is >= the other priority.</returns>
    public static bool IsGreaterOrEqual(this LogLevel current, LogLevel other)
    {
        return (int)current >= (int)other;
    }
}