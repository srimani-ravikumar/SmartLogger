using System;
using System.Threading;

namespace SmartLogger.Core;

/// <summary>
/// Provides a lightweight, request-scoped context for logging.
/// </summary>
/// <remarks>
/// <para>
/// Stores contextual data (e.g., correlation ID) that flows with the current
/// execution context across async/await boundaries.
/// </para>
/// 
/// <para>
/// Typical usage:
/// <code>
/// using (LogContext.BeginCorrelationScope("req-123"))
/// {
///     logger.Info("Processing request");
/// }
/// </code>
/// </para>
/// 
/// <para>
/// Design Goals:
/// <list type="bullet">
/// <item><description>Avoid passing correlation data explicitly through method calls</description></item>
/// <item><description>Ensure isolation between concurrent execution flows</description></item>
/// <item><description>Keep logging concerns decoupled from business logic</description></item>
/// </list>
/// </para>
/// </remarks>
public static class LogContext
{
    /// <summary>
    /// Holds correlation ID per async execution flow.
    /// </summary>
    /// <remarks>
    /// <see cref="AsyncLocal{T}"/> ensures values flow across async calls
    /// while remaining isolated between parallel requests.
    /// </remarks>
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Gets the correlation ID associated with the current execution flow.
    /// </summary>
    /// <value>
    /// The current correlation ID, or <c>null</c> if no scope is active.
    /// </value>
    public static string? CorrelationId => _correlationId.Value;

    /// <summary>
    /// Begins a new correlation scope for the current execution flow.
    /// </summary>
    /// <param name="correlationId">
    /// Identifier used to group and trace related log entries.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> that restores the previous correlation ID when disposed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Supports nested scopes. Each call preserves the previous value and restores it on dispose.
    /// </para>
    /// 
    /// <para>
    /// This method is safe for concurrent usage due to <see cref="AsyncLocal{T}"/>.
    /// </para>
    /// </remarks>
    public static IDisposable BeginCorrelationScope(string correlationId)
    {
        var previous = _correlationId.Value;

        // Set new correlation context for current execution flow
        _correlationId.Value = correlationId;

        // Return scope that restores previous value on dispose
        return new Scope(() => _correlationId.Value = previous);
    }

    /// <summary>
    /// Represents a disposable scope that restores the previous correlation ID.
    /// </summary>
    private sealed class Scope : IDisposable
    {
        private readonly Action _onDispose;

        /// <summary>
        /// Initializes a new scope with the specified cleanup action.
        /// </summary>
        /// <param name="onDispose">Action to execute when the scope ends.</param>
        public Scope(Action onDispose) => _onDispose = onDispose;

        /// <summary>
        /// Ends the current scope and restores the previous correlation ID.
        /// </summary>
        public void Dispose()
        {
            _onDispose();
        }
    }
}