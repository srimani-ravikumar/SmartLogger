using SmartLogger.Core;
using System;

namespace SmartLogger.Appenders.FileRolling;

internal static class RollingStrategyFactory
{
    public static IRollingStrategy Create(
        FileConfiguration configuration)
    {
        return configuration.Rolling.Strategy switch
        {
            RollingStrategyType.Daily =>
                new DailyRollingStrategy(),

            RollingStrategyType.Size =>
                new SizeRollingStrategy(configuration),

            _ => throw new NotSupportedException(
                $"Unsupported rolling strategy '{configuration.Rolling.Strategy}'.")
        };
    }
}
