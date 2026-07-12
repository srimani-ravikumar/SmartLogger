using System;

namespace SmartLogger.Appenders.FileRolling;

internal sealed class DailyRollingStrategy : IRollingStrategy
{
    private DateTime _currentDay =
        DateTime.Today;

    public bool ShouldRoll(string activeFilePath)
    {
        if (DateTime.Today > _currentDay)
        {
            _currentDay = DateTime.Today;
            return true;
        }

        return false;
    }
}