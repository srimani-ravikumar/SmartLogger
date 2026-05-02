using SmartLogger.Appenders.FileRolling;
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
    /// Responsible for constructing file names for active logs.
    /// </summary>
    private readonly FileNameBuilder _fileNameBuilder;

    /// <summary>
    /// Current active log file path used for writing.
    /// </summary>
    private string _activeFilePath;

    /// <summary>
    /// File-related configuration (base path, naming, extension).
    /// </summary>
    private readonly FileConfiguration _fileConfig;

    /// <summary>
    /// Minimum log level required for a message to be written.
    /// </summary>
    private LogLevel _logLevel;

    /// <summary>
    /// Formatter used to convert <see cref="LogMessage"/> into string output.
    /// </summary>
    private ILogOutputFormatterStrategy _formatter;

    /// <summary>
    /// Optional rolling strategy controlling file rotation.
    /// </summary>
    private readonly IRollingStrategy? _rollingStrategy;

    /// <summary>
    /// Synchronization primitive to ensure thread-safe file operations.
    /// </summary>
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAppender"/> class.
    /// </summary>
    /// <param name="fileConfig">File configuration for naming and paths.</param>
    /// <param name="logLevel">Minimum log level threshold.</param>
    /// <param name="formatter">Formatter for message rendering.</param>
    /// <param name="rollingStrategy">Optional rolling strategy.</param>
    /// <exception cref="ArgumentNullException">Thrown when required arguments are null.</exception>
    internal FileAppender(
        FileConfiguration fileConfig,
        LogLevel logLevel,
        ILogOutputFormatterStrategy formatter,
        IRollingStrategy? rollingStrategy)
    {
        _fileConfig = fileConfig ?? throw new ArgumentNullException(nameof(fileConfig));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

        _logLevel = logLevel;
        _rollingStrategy = rollingStrategy;

        _fileNameBuilder = new FileNameBuilder(_fileConfig);

        // Resolve initial file path and ensure directory exists
        _activeFilePath = ResolvePath(_fileNameBuilder.Build());
        EnsureDirectoryExists();
    }

    /// <inheritdoc/>
    public void Append(LogMessage message)
    {
        // Fast exit for null or below-threshold messages
        if (message is null || !IsEnabled(message.LogLevel))
            return;

        // Capture formatter reference to avoid race conditions
        var currentFormatter = _formatter;

        // Perform formatting outside lock (CPU-bound)
        var formattedMessage = currentFormatter.Format(message);

        // Lock only for file system operations
        lock (_lockObject)
        {
            // Evaluate rolling before writing
            if (_rollingStrategy != null && _rollingStrategy.ShouldRoll(_activeFilePath))
            {
                var newFile = _rollingStrategy.GetNextFilePath(_activeFilePath);

                // Rename current file to rolled file
                if (File.Exists(_activeFilePath))
                {
                    File.Move(_activeFilePath, newFile);
                }

                // Notify strategy about roll event
                _rollingStrategy.OnRoll(newFile);

                // Create a fresh active file path
                _activeFilePath = ResolvePath(_fileNameBuilder.Build());
            }

            // Append log entry (shared read/write allows tailing tools)
            using var stream = new FileStream(
                _activeFilePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite);

            using var writer = new StreamWriter(stream);
            writer.WriteLine(formattedMessage);
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

    /// <summary>
    /// Ensures the directory for the active log file exists.
    /// </summary>
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_activeFilePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Resolves a relative path to an absolute path using the application base directory.
    /// </summary>
    /// <param name="path">The input file path.</param>
    /// <returns>An absolute file path.</returns>
    private static string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);
    }
}