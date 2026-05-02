using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Rolling strategy based on time intervals.
/// </summary>
/// <remarks>
/// Triggers a roll when the current time crosses into a new configured time window
/// (e.g., hourly, daily, monthly).
/// 
/// The strategy tracks the active time window and compares it against the current time
/// to determine when rotation should occur.
/// </remarks>
internal class TimeRollingStrategy : IRollingStrategy
{
    /// <summary>
    /// Configured rolling interval (e.g., Hour, Day, Month).
    /// </summary>
    private readonly RollingInterval _interval;

    /// <summary>
    /// Responsible for constructing rolled file names.
    /// </summary>
    private readonly FileNameBuilder _builder;

    /// <summary>
    /// Represents the currently active time window.
    /// </summary>
    private DateTime _currentWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeRollingStrategy"/> class.
    /// </summary>
    /// <param name="fileConfig">Configuration containing rolling policy and naming rules.</param>
    public TimeRollingStrategy(FileConfiguration fileConfig)
    {
        _interval = fileConfig.RollingPolicy.Interval;
        _builder = new FileNameBuilder(fileConfig);

        // Initialize the baseline window at startup
        _currentWindow = GetCurrentWindow();
    }

    /// <summary>
    /// Determines whether the log file should be rolled based on time progression.
    /// </summary>
    /// <param name="filePath">The current log file path (not used for time-based decisions).</param>
    /// <returns>
    /// <c>true</c> if the current time has moved into a new time window;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// When a new time window is detected, the internal state is updated
    /// to reflect the new window.
    /// </remarks>
    public bool ShouldRoll(string filePath)
    {
        var now = GetCurrentWindow();

        // Detect transition into a new time window
        if (now > _currentWindow)
        {
            _currentWindow = now;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Generates the next file path for the new time window.
    /// </summary>
    /// <param name="basePath">The base file path defined in configuration (not directly used).</param>
    /// <returns>The file path for the new log file.</returns>
    /// <remarks>
    /// Relies on <see cref="FileNameBuilder"/> to incorporate time-based naming.
    /// No index handling is required for pure time-based rolling.
    /// </remarks>
    public string GetNextFilePath(string basePath)
    {
        return _builder.Build();
    }

    /// <summary>
    /// Executes post-roll actions.
    /// </summary>
    /// <param name="currentFilePath">The file path that was just rolled.</param>
    /// <remarks>
    /// No-op for time-based rolling.
    /// Hook provided for extensibility (e.g., archival, compression).
    /// </remarks>
    public void OnRoll(string currentFilePath) { }

    /// <summary>
    /// Computes the normalized start of the current time window based on the configured interval.
    /// </summary>
    /// <returns>A <see cref="DateTime"/> representing the current window boundary.</returns>
    /// <remarks>
    /// Examples:
    /// <list type="bullet">
    /// <item><description>Hour → 2026-05-02 14:00:00</description></item>
    /// <item><description>Day → 2026-05-02 00:00:00</description></item>
    /// <item><description>Month → 2026-05-01 00:00:00</description></item>
    /// </list>
    /// 
    /// Normalization ensures consistent comparison across time checks.
    /// </remarks>
    private DateTime GetCurrentWindow()
    {
        var now = DateTime.Now;

        return _interval switch
        {
            // Normalize to start of the hour
            RollingInterval.Hour => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0),

            // Normalize to start of the day
            RollingInterval.Day => new DateTime(now.Year, now.Month, now.Day),

            // Normalize to start of the month
            RollingInterval.Month => new DateTime(now.Year, now.Month, 1),

            // Fallback: no normalization
            _ => now
        };
    }
}