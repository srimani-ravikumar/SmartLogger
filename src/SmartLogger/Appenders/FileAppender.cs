using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;
using SmartLogger.Formatters;
using System;
using System.IO;

namespace SmartLogger.Appenders;

/// <summary>
/// Appends log messages to a file in append mode.
/// </summary>
internal sealed class FileAppender : ILogAppender
{
    private readonly FileNameBuilder _fileNameBuilder;
    private string _activeFilePath;
    private readonly FileConfiguration _fileConfig;
    private LogLevel _logLevel;
    private ILogOutputFormatterStrategy _formatter;
    private readonly IRollingStrategy? _rollingStrategy;
    private readonly object _lockObject = new();

    internal FileAppender(FileConfiguration fileConfig, LogLevel logLevel, ILogOutputFormatterStrategy formatter, IRollingStrategy? rollingStrategy)
    {
        _fileConfig = fileConfig ?? throw new ArgumentNullException(nameof(fileConfig));
        _logLevel = logLevel;
        _formatter = formatter;
        _rollingStrategy = rollingStrategy;
        _fileNameBuilder = new FileNameBuilder(_fileConfig);
        _activeFilePath = ResolvePath(_fileNameBuilder.Build());
        EnsureDirectoryExists();
    }

    /// <inheritdoc/>
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
            if (_rollingStrategy != null && _rollingStrategy.ShouldRoll(_activeFilePath))
            {
                var newFile = _rollingStrategy.GetNextFilePath(_activeFilePath);

                if (File.Exists(_activeFilePath))
                {
                    File.Move(_activeFilePath, newFile);
                }

                _rollingStrategy.OnRoll(newFile);

                _activeFilePath = ResolvePath(_fileNameBuilder.Build());
            }

            File.AppendAllText(_activeFilePath, formattedMessage + Environment.NewLine);
        }
    }

    /// <inheritdoc/>
    public void SetLogLevel(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    /// <inheritdoc/>
    public LogLevel GetLogLevel(LogLevel logLevel)
    {
        return _logLevel;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel.IsGreaterOrEqual(_logLevel);
    }

    /// <inheritdoc/>
    public void SetFormatter(ILogOutputFormatterStrategy formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    /// <inheritdoc/>
    public ILogOutputFormatterStrategy GetFormatter()
    {
        return _formatter;
    }

    /// <inheritdoc/>
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_activeFilePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <inheritdoc/>
    private static string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);
    }
}
