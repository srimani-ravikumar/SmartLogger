using SmartLogger.Core;
using SmartLogger.Formatters;
using System;
using System.IO;

namespace SmartLogger.Appenders;

/// <summary>
/// Appends log messages to a file in append mode.
/// </summary>
public class FileAppender : ILogAppender
{
    private readonly string _filePath;
    private LogLevel _logLevel;
    private ILogFormatter _formatter;
    private readonly object _lockObject = new();

    public FileAppender(string path, LogLevel logLevel)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("File path cannot be null or empty.", nameof(path));

        _filePath = ResolvePath(path);
        _logLevel = logLevel;
        _formatter = new DetailedFormatter();

        EnsureDirectoryExists();
    }

    public void Append(LogMessage message)
    {
        if (message is null || !IsEnabled(message.LogLevel))
            return;

        // Format outside lock
        var currentFormatter = _formatter;
        var formattedMessage = currentFormatter.Format(message);

        // Lock only during physical I/O
        lock (_lockObject)
        {
            File.AppendAllText(_filePath, formattedMessage + Environment.NewLine);
        }
    }

    public void SetLogLevel(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    public LogLevel GetLogLevel(LogLevel logLevel)
    {
        return _logLevel;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel.IsGreaterOrEqual(_logLevel);
    }

    public void SetFormatter(ILogFormatter formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public ILogFormatter GetFormatter()
    {
        return _formatter;
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);
    }
}
