using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Appenders.FileSystem;
using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Appenders;

/// <summary>
/// Appends log messages to a file in append mode.
/// </summary>
/// <remarks>
/// Thread-safe file appender that:
/// <list type="bullet">
/// <item><description>Formats messages outside the critical section</description></item>
/// <item><description>Locks only during file I/O operations</description></item>
/// <item><description>Supports optional rolling via <see cref="IRollingStrategy"/></description></item>
/// </list>
/// 
/// Rolling Behavior:
/// <list type="bullet">
/// <item><description>If a rolling strategy is configured, rotation is evaluated before each write</description></item>
/// <item><description>Active file may be renamed and replaced transparently</description></item>
/// </list>
/// </remarks>
internal sealed class FileAppender : ILogAppender
{
    /// <summary>
    /// Minimum log level required for a message to be written.
    /// </summary>
    private LogLevel _logLevel;

    /// <summary>
    /// Formatter used to convert <see cref="LogMessage"/> into string output.
    /// </summary>
    private ILogOutputFormatterStrategy _formatter;

    private readonly FileLifecycleManager _fileLifecycleManager;

    internal FileAppender(
    FileConfiguration configuration,
    LogLevel logLevel,
    ILogOutputFormatterStrategy formatter,
    IRollingStrategy? rollingStrategy,
    IFileNamingStrategy namingStrategy)
    {
        _formatter = formatter;
        _logLevel = logLevel;

        _fileLifecycleManager =
            new FileLifecycleManager(
                configuration,
                rollingStrategy,
                namingStrategy);
    }


    /// <inheritdoc/>
    public void Append(LogMessage message)
    {
        if (message is null) return;

        if (!IsEnabled(message.LogLevel)) return;

        var formattedMessage = _formatter.Format(message);

        _fileLifecycleManager.Write(formattedMessage);
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

    internal void UpdateConfiguration(LogLevel logLevel, ILogOutputFormatterStrategy formatter)
    {
        _logLevel = logLevel;
        _formatter = formatter;
    }
}