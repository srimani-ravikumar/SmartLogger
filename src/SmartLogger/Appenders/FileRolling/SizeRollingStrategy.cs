using SmartLogger.Core;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

internal class SizeRollingStrategy : IRollingStrategy
{
    private readonly long _maxBytes;
    private readonly FileNameBuilder _builder;

    public SizeRollingStrategy(FileConfiguration fileConfig)
    {
        _maxBytes = fileConfig.RollingPolicy.MaxFileSizeMB * 1024 * 1024;
        _builder = new FileNameBuilder(fileConfig);
    }

    public bool ShouldRoll(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        var info = new FileInfo(filePath);
        return info.Length >= _maxBytes;
    }

    public string GetNextFilePath(string basePath)
    {
        int index = 1;

        string newPath;
        do
        {
            newPath = _builder.Build(index);
            index++;
        }
        while (File.Exists(newPath));

        return Path.GetFullPath(newPath);
    }

    public void OnRoll(string currentFilePath) { }
}