using SmartLogger.Core;
using System;

namespace SmartLogger.Appenders.FileNaming;

/// <summary>
/// Generates timestamp-based log file names.
/// </summary>
internal sealed class TimestampFileNamingStrategy : IFileNamingStrategy
{
    public TimestampFileNamingStrategy(FileConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public string CreateActiveFileName()
    {
        throw new NotImplementedException();
    }

    public string CreateRolledFileName(int index = 0)
    {
        throw new NotImplementedException();
    }
}