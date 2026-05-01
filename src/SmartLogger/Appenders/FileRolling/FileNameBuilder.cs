using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

internal sealed class FileNameBuilder
{
    private readonly FileConfiguration _config;

    public FileNameBuilder(FileConfiguration config)
    {
        _config = config;
    }

    public string Build(int index = 0)
    {
        var parts = new List<string>();

        // Base name
        parts.Add(Path.GetFileName(_config.BasePath));

        // Date
        if (_config.Naming.IncludeDate)
        {
            var date = DateTime.Now.ToString(_config.Naming.DateFormat);
            parts.Add(date);
        }

        // Index
        if (_config.Naming.IncludeIndex && index > 0)
        {
            parts.Add(index.ToString());
        }

        var fileName = string.Join(_config.Naming.Separator, parts);

        var fullPath = Path.Combine(
            Path.GetDirectoryName(_config.BasePath)!,
            $"{fileName}.{_config.Extension}"
        );

        return fullPath;
    }
}