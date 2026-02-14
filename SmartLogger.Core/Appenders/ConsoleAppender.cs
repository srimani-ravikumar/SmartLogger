using SmartLogger.Core;
using SmartLogger.Formatters;

namespace SmartLogger.Appenders;

/// <summary>
/// Appends log messages to the standard console output or error stream.
/// </summary>
internal class ConsoleAppender : ILogAppender
{
    // Threshold and Formatter are volatile or read-only references to ensure 
    // thread-safety when they are updated via Setters.
    private LogLevel _logLevel;
    private ILogFormatter _formatter;
    private readonly object _lockObject = new();

    public ConsoleAppender() : this(LogLevel.DEBUG) { }

    public ConsoleAppender(LogLevel logLevel)
    {
        _logLevel = logLevel;
        _formatter = new DetailedFormatter();
    }

    /// <inheritdoc />
    public void Append(LogMessage logMessage)
    {
        // 1. Thread-safe threshold and null check
        if (logMessage is null || !IsEnabled(logMessage.LogLevel))
        {
            return;
        }

        // 2. Format the message outside the lock to maximize performance
        var currentFormatter = _formatter;
        var formattedMessage = currentFormatter.Format(logMessage);

        // 3. Select stream (Error for Fatal/Error, Out for the rest)
        var output = logMessage.LogLevel is LogLevel.ERROR or LogLevel.FATAL
            ? Console.Error
            : Console.Out;

        // 4. Lock only during the physical I/O operation to prevent interleaved lines
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
    /// Logic: Returns true if the incoming level is higher or equal to our internal threshold.
    /// </remarks>
    public bool IsEnabled(LogLevel logLevel) => logLevel.IsGreaterOrEqual(_logLevel);

    /// <inheritdoc />
    public void SetFormatter(ILogFormatter formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    /// <inheritdoc />
    public ILogFormatter GetFormatter() => _formatter;
}