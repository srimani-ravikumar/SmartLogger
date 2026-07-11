using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartLogger.Appenders;

/// <summary>
/// Asynchronous wrapper for <see cref="ILogAppender"/>.
/// </summary>
/// <remarks>
/// Decouples log production from log writing by introducing a background worker
/// that processes log messages from a queue.
/// 
/// Benefits:
/// <list type="bullet">
/// <item><description>Reduces latency in caller threads</description></item>
/// <item><description>Prevents blocking on I/O-heavy appenders</description></item>
/// <item><description>Smoothens burst traffic via buffering</description></item>
/// </list>
/// 
/// Threading Model:
/// - Producers enqueue messages under a lock
/// - A single background thread consumes and forwards them
/// </remarks>
internal sealed class AsyncAppenderWrapper : ILogAppender
{
    /// <summary>
    /// Maximum number of messages allowed in the queue before applying backpressure.
    /// TODO: This needs to configurable item as part of async mode
    /// </summary>
    private const int MaxQueueSize = 10_000;

    /// <summary>
    /// The underlying appender that performs actual log writes.
    /// </summary>
    internal ILogAppender InnerAppender { get; }

    /// <summary>
    /// In-memory queue buffering log messages before processing.
    /// </summary>
    private readonly Queue<LogMessage> _queue = new();

    /// <summary>
    /// Synchronization primitive for coordinating producers and the worker thread.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Background worker thread responsible for processing queued messages.
    /// </summary>
    private readonly Thread _worker;

    /// <summary>
    /// Indicates whether the worker thread should continue processing.
    /// </summary>
    private bool _running = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncAppenderWrapper"/> class.
    /// </summary>
    /// <param name="innerAppender">The appender to wrap with asynchronous behavior.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerAppender"/> is null.</exception>
    public AsyncAppenderWrapper(ILogAppender innerAppender)
    {
        InnerAppender = innerAppender ?? throw new ArgumentNullException(nameof(innerAppender));

        // Dedicated background worker for consuming the queue
        _worker = new Thread(ProcessQueue)
        {
            IsBackground = true,
            Name = "SmartLogger.AsyncAppenderWorker"
        };

        _worker.Start();
    }

    /// <inheritdoc/>
    public void Append(LogMessage message)
    {
        // Ignore null messages to avoid unnecessary queue operations
        if (message is null) return;

        lock (_lock)
        {
            // Apply simple backpressure: drop oldest message when queue is full
            // TODO: Rather dropping old messages. Drop the lower priority messages
            if (_queue.Count >= MaxQueueSize)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(message);

            // Signal the worker thread that new data is available
            Monitor.Pulse(_lock);
        }
    }

    /// <inheritdoc/>
    public void SetLogLevel(LogLevel level) => InnerAppender.SetLogLevel(level);

    /// <inheritdoc/>
    public LogLevel GetLogLevel(LogLevel level) => InnerAppender.GetLogLevel(level);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel level) => InnerAppender.IsEnabled(level);

    /// <inheritdoc/>
    public void SetFormatter(ILogOutputFormatterStrategy formatter) => InnerAppender.SetFormatter(formatter);

    /// <inheritdoc/>
    public ILogOutputFormatterStrategy GetFormatter() => InnerAppender.GetFormatter();

    /// <summary>
    /// Stops the background worker and ensures all queued messages are processed.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            _running = false;

            // Wake up worker if waiting
            Monitor.PulseAll(_lock);
        }

        // Wait for worker to finish processing remaining messages
        _worker.Join();
    }

    /// <summary>
    /// Continuously processes queued log messages on a background thread.
    /// </summary>
    /// <remarks>
    /// Waits efficiently when the queue is empty and resumes when signaled.
    /// Ensures messages are processed in FIFO order.
    /// </remarks>
    private void ProcessQueue()
    {
        while (true)
        {
            LogMessage message;

            lock (_lock)
            {
                // Wait until there is work OR shutdown is requested
                while (_queue.Count == 0 && _running)
                {
                    Monitor.Wait(_lock);
                }

                // Exit condition: no more work and shutdown requested
                if (_queue.Count == 0 && !_running)
                {
                    return;
                }

                message = _queue.Dequeue();
            }

            try
            {
                // Delegate actual write to the underlying appender
                InnerAppender.Append(message);
            }
            catch
            {
                // Never allow logging failures to crash the worker thread
                // (optional: plug in fallback logging here)
            }
        }
    }
}