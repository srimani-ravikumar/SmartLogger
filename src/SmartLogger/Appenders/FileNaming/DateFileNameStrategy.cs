using SmartLogger.Core;
using System;

namespace SmartLogger.Appenders.FileNaming;

/// <summary>
/// Generates date-based log file names.
/// </summary>
internal sealed class DateFileNamingStrategy : IFileNamingStrategy
{
    private readonly FileConfiguration _configuration;

    public DateFileNamingStrategy(FileConfiguration configuration)
    {
        _configuration = configuration
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string CreateActiveFileName()
    {
        return $"{_configuration.FileName}.{_configuration.Extension}";
    }

    /// <inheritdoc/>
    public string CreateRolledFileName(int index = 0)
    {
        var date = DateTime.Now.ToString(_configuration.Naming.DateFormat);

        var fileName = index > 0
            ? $"{_configuration.FileName}_{date}_{index}"
            : $"{_configuration.FileName}_{date}";

        return $"{fileName}.{_configuration.Extension}";
    }
}