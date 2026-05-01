using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

internal class TimeRollingStrategy : IRollingStrategy
{
    private readonly RollingInterval _interval;
    private readonly FileNameBuilder _builder;
    private DateTime _currentWindow;

    public TimeRollingStrategy(FileConfiguration fileConfig)
    {
        _interval = fileConfig.RollingPolicy.Interval;
        _builder = new FileNameBuilder(fileConfig);
        _currentWindow = GetCurrentWindow();
    }

    public bool ShouldRoll(string filePath)
    {
        var now = GetCurrentWindow();

        if (now > _currentWindow)
        {
            _currentWindow = now;
            return true;
        }

        return false;
    }

    public string GetNextFilePath(string basePath)
    {
        return _builder.Build();
    }

    public void OnRoll(string currentFilePath) { }

    private DateTime GetCurrentWindow()
    {
        var now = DateTime.Now;

        return _interval switch
        {
            RollingInterval.Hour => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0),
            RollingInterval.Day => new DateTime(now.Year, now.Month, now.Day),
            RollingInterval.Month => new DateTime(now.Year, now.Month, 1),
            _ => now
        };
    }
}