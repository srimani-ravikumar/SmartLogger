using SmartLogger.Core;
using SmartLogger.Appenders.Aggregation;
using System;

namespace SmartLogger.Appenders;

internal class LogAggregatorAppender : ILogAppender
{
    /// <summary>
    /// Log aggregator appender that sends log messages to a specified log aggregator sink.
    /// </summary>
    private readonly ILogAggregatorSink _sink;

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
    private ILogOutputFormatterStrategy _formatter;

    /// <summary>
    /// Creates a new instance of <see cref="LogAggregatorAppender"/> with the specified sink.
    /// </summary>
    /// <param name="sink">The log aggregator sink to which log messages will be sent.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public LogAggregatorAppender(ILogAggregatorSink sink)
    {
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    }

    /// <inheritdoc/>
    public void Append(LogMessage message)
    {
        // Send the log message to the configured sink.
        _sink.Send(message);
    }

    /// <inheritdoc/>
    public ILogOutputFormatterStrategy GetFormatter() => _formatter;

    /// <inheritdoc/>
    public LogLevel GetLogLevel(LogLevel logLevel) => _logLevel;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

    /// <inheritdoc/>
    public void SetFormatter(ILogOutputFormatterStrategy formatter) => _formatter = formatter;

    /// <inheritdoc/>
    public void SetLogLevel(LogLevel logLevel) => _logLevel = logLevel;
}