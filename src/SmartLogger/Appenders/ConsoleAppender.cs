using SmartLogger.Core;
using SmartLogger.Formatters;
using System;
using System.Threading;

namespace SmartLogger.Appenders;

/// <summary>
/// Appends log messages to the standard console output or error stream.
/// </summary>
/// <remarks>
/// Thread-safe implementation that minimizes contention by:
/// <list type="bullet">
/// <item><description>Performing formatting outside the critical section</description></item>
/// <item><description>Locking only during the actual I/O write</description></item>
/// </list>
/// 
/// Routing Behavior:
/// <list type="bullet">
/// <item><description><see cref="LogLevel.ERROR"/> and <see cref="LogLevel.FATAL"/> → <see cref="Console.Error"/></description></item>
/// <item><description>All other levels → <see cref="Console.Out"/></description></item>
/// </list>
/// </remarks>
internal sealed class ConsoleAppender : ILogAppender
{
    /// <summary>
    /// Minimum log level required for a message to be written.
    /// </summary>
    /// <remarks>
    /// Updated via <see cref="SetLogLevel"/>. Read frequently in hot path.
    /// </remarks>
    private LogLevel _logLevel;

    /// <summary>
    /// Formatter used to convert <see cref="LogMessage"/> into string output.
    /// </summary>
    /// <remarks>
    /// Reference is swapped atomically; formatting is done outside locks for performance.
    /// </remarks>
    private ILogOutputFormatterStrategy _formatter;

    /// <summary>
    /// Synchronization object to prevent interleaved console writes.
    /// </summary>
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a default console appender with standard configuration.
    /// </summary>
    internal ConsoleAppender()
    {
        _logLevel = LogLevel.INFO;

        // Default formatter aligned with console output expectations
        _formatter = FormatterFactory.Create(new AppenderConfiguration()
        {
            Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.Console
            },
            Formatter = new FormatterConfiguration
            {
                OutputFormat = LogOutputFormat.PlainText,
                LayoutType = LogMessageLayoutType.Simple
            }
        });
    }

    /// <summary>
    /// Initializes a console appender with explicit log level and formatter.
    /// </summary>
    /// <param name="logLevel">Minimum log level threshold.</param>
    /// <param name="formatter">Formatter used for message rendering.</param>
    internal ConsoleAppender(LogLevel logLevel, ILogOutputFormatterStrategy formatter)
    {
        _logLevel = logLevel;
        _formatter = formatter;
    }

    /// <inheritdoc />
    public void Append(LogMessage logMessage)
    {
        // Fast exit: null or below threshold
        if (logMessage is null || !IsEnabled(logMessage.LogLevel))
        {
            return;
        }

        // Capture formatter reference to avoid race conditions during formatting
        var currentFormatter = _formatter;

        // Perform formatting outside lock (CPU-bound work)
        var formattedMessage = currentFormatter.Format(logMessage);

        // Route output based on severity
        var output = logMessage.LogLevel is LogLevel.ERROR or LogLevel.FATAL
            ? Console.Error
            : Console.Out;

        // Lock only for the actual I/O operation (prevents interleaved writes)
        lock (_lockObject)
        {
            output.WriteLine(formattedMessage);
        }
    }

    /// <inheritdoc />
    public void SetLogLevel(LogLevel level) => _logLevel = level;

    /// <inheritdoc />
    public LogLevel GetLogLevel(LogLevel logLevel) => _logLevel;

    /// <inheritdoc />
    /// <remarks>
    /// Returns <c>true</c> if the provided level meets or exceeds the configured threshold.
    /// </remarks>
    public bool IsEnabled(LogLevel logLevel) => logLevel.IsGreaterOrEqual(_logLevel);

    /// <inheritdoc />
    public void SetFormatter(ILogOutputFormatterStrategy formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    /// <inheritdoc />
    public ILogOutputFormatterStrategy GetFormatter() => _formatter;
}