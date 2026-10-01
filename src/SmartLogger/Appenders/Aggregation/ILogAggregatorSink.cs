using SmartLogger.Core;

namespace SmartLogger.Appenders.Aggregation;

/// <summary>
/// Defines a contract for log aggregator sinks that can receive log messages.
/// </summary>
public interface ILogAggregatorSink
{
    /// <summary>
    /// Sends a log message to the aggregator sink.
    /// </summary>
    /// <param name="message">The log message to send.</param>
    void Send(LogMessage message);
}