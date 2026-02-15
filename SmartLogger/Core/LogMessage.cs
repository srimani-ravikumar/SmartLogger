using System;

namespace SmartLogger.Core;

/// <summary>
/// Represents a standardized log entry containing context and metadata.
/// </summary>
public class LogMessage
{
    /// <summary>
    /// Gets the UTC timestamp when the log message was created.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the severity level of the log.
    /// </summary>
    public LogLevel LogLevel { get; init; }

    /// <summary>
    /// Gets the actual log message content.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the component or class generating the log.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets the managed thread ID that generated the log.
    /// </summary>
    public int ThreadId { get; init; }

    /// <summary>
    /// Gets the unique ID used to trace requests across services, if applicable.
    /// </summary>
    public string CorrelationId { get; init; }

    private LogMessage(Builder builder)
    {
        Timestamp = builder.InternalTimestamp;
        LogLevel = builder.InternalLogLevel ?? LogLevel.INFO;
        Message = builder.InternalMessage;
        Source = builder.InternalSource;
        ThreadId = builder.InternalThreadId;
        CorrelationId = builder.InternalCorrelationId;
    }

    /// <summary>
    /// Provides a fluent interface for constructing <see cref="LogMessage"/> instances.
    /// </summary>
    public class Builder
    {
        internal DateTime InternalTimestamp { get; private set; } = DateTime.UtcNow;
        internal LogLevel? InternalLogLevel { get; private set; }
        internal string InternalMessage { get; private set; } = string.Empty;
        internal string InternalSource { get; private set; } = "System";
        internal int InternalThreadId { get; private set; } = Environment.CurrentManagedThreadId;
        internal string InternalCorrelationId { get; private set; }

        /// <summary>
        /// Sets the log level.
        /// </summary>
        public Builder WithLevel(LogLevel logLevel)
        {
            InternalLogLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Sets the message content.
        /// </summary>
        public Builder WithMessage(string message)
        {
            InternalMessage = message;
            return this;
        }

        /// <summary>
        /// Sets the source name (e.g., "DatabaseService").
        /// </summary>
        public Builder FromSource(string source)
        {
            InternalSource = source;
            return this;
        }

        /// <summary>
        /// Sets the Correlation ID for distributed tracing.
        /// </summary>
        public Builder WithCorrelationId(string correlationId)
        {
            InternalCorrelationId = correlationId;
            return this;
        }

        /// <summary>
        /// Validates and builds the final <see cref="LogMessage"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if Level or Message is missing.</exception>
        public LogMessage Build()
        {
            if (InternalLogLevel is null) throw new InvalidOperationException("LogLevel is required.");

            if (string.IsNullOrWhiteSpace(InternalMessage)) throw new InvalidOperationException("Message is required.");

            return new LogMessage(this);
        }
    }
}