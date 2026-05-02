using SmartLogger.Core;

namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Factory responsible for creating <see cref="IRollingStrategy"/> instances
/// based on the configured rolling policy.
/// </summary>
/// <remarks>
/// Centralizes strategy selection logic to avoid conditional branching
/// across the codebase and to keep appender implementations clean.
/// 
/// Adding a new rolling strategy requires only extending this factory,
/// preserving the Open/Closed Principle.
/// </remarks>
internal static class RollingFactory
{
    /// <summary>
    /// Creates an appropriate <see cref="IRollingStrategy"/> based on the provided configuration.
    /// </summary>
    /// <param name="fileConfig">The file configuration containing rolling policy details.</param>
    /// <returns>
    /// A concrete <see cref="IRollingStrategy"/> implementation if the rolling type is supported;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Supported strategies:
    /// <list type="bullet">
    /// <item><description><see cref="RollingType.Size"/> → <see cref="SizeRollingStrategy"/></description></item>
    /// <item><description><see cref="RollingType.Time"/> → <see cref="TimeRollingStrategy"/></description></item>
    /// </list>
    /// 
    /// Returning <c>null</c> indicates no rolling strategy is applied.
    /// Callers should handle this case explicitly.
    /// </remarks>
    public static IRollingStrategy Create(FileConfiguration fileConfig)
    {
        return fileConfig.RollingPolicy.RollingType switch
        {
            // Size-based rolling (e.g., rotate when file exceeds configured size)
            RollingType.Size => new SizeRollingStrategy(fileConfig),

            // Time-based rolling (e.g., daily/hourly rotation)
            RollingType.Time => new TimeRollingStrategy(fileConfig),

            // Placeholder for future hybrid strategy (size + time)
            // RollingType.Hybrid => new HybridRollingStrategy(rollingPolicyConfig),

            // No matching strategy → explicitly return null (caller responsibility)
            _ => null
        };
    }
}