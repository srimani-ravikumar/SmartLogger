using SmartLogger.Core;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

internal sealed class SizeRollingStrategy : IRollingStrategy
{
    private readonly long _maxBytes;

    public SizeRollingStrategy(FileConfiguration configuration)
    {
        _maxBytes =
            configuration.Rolling.MaxFileSizeMB * 1024 * 1024;
    }

    public bool ShouldRoll(string activeFilePath)
    {
        if (!File.Exists(activeFilePath))
            return false;

        return new FileInfo(activeFilePath).Length >= _maxBytes;
    }
}