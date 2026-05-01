using SmartLogger.Core;

namespace SmartLogger.Appenders.FileRolling;

internal static class RollingFactory
{
    public static IRollingStrategy Create(FileConfiguration fileConfig)
    {

        return fileConfig.RollingPolicy.RollingType switch
        {
            RollingType.Size => new SizeRollingStrategy(fileConfig),

            RollingType.Time => new TimeRollingStrategy(fileConfig),

            // TODO
            // RollingType.Hybrid => new HybridRollingStrategy(rollingPolicyConfig),

            _ => null
        };
    }
}