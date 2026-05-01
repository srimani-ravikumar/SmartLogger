using System;
using System.Threading;

namespace SmartLogger.Core;

/// <summary>
/// Provides a lightweight, request-scoped context for logging.
/// 
/// <para>
/// The correlation ID stored here represents the current execution flow
/// (e.g., a web request or background job) and is automatically available
/// to all loggers without being passed manually.
/// </para>
/// 
/// <para>
/// Internally uses <see cref="AsyncLocal{T}"/> so the value flows correctly
/// across async/await boundaries while remaining isolated between requests.
/// </para>
/// 
/// <para>
/// This class is intentionally separate from the logger implementation to
/// keep logging concerns and execution context concerns decoupled.
/// </para>
/// </summary>
public static class LogContext
{
    // AsyncLocal ensures the CorrelationId is unique per execution flow (not per thread)
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Gets the correlation ID associated with the current execution flow.
    /// 
    /// <para>
    /// Returns <c>null</c> if no correlation scope has been established.
    /// </para>
    /// </summary>
    public static string? CorrelationId => _correlationId.Value;

    /// <summary>
    /// Begins a new correlation scope for the current execution flow.
    /// 
    /// <para>
    /// All logs created within this scope will automatically include the
    /// provided correlation ID.
    /// </para>
    /// 
    /// <para>
    /// The previous correlation ID (if any) is restored when the returned
    /// <see cref="IDisposable"/> is disposed.
    /// </para>
    /// </summary>
    /// <param name="correlationId">
    /// The identifier used to group and trace related log entries.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> that ends the scope when disposed.
    /// </returns>
    public static IDisposable BeginCorrelationScope(string correlationId)
    {
        var previous = _correlationId.Value;
        _correlationId.Value = correlationId;

        return new Scope(() => _correlationId.Value = previous);
    }

    /// <summary>
    /// Represents a scoped context that restores the previous correlation ID
    /// when disposed.
    /// </summary>
    private sealed class Scope : IDisposable
    {
        private readonly Action _onDispose;

        public Scope(Action onDispose) => _onDispose = onDispose;

        /// <summary>
        /// Ends the current scope and restores the previous correlation ID.
        /// </summary>
        public void Dispose() => _onDispose();
    }
}