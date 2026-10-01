using System;
using System.Net.Http;
using System.Net.Http.Json;
using SmartLogger.Core;

namespace SmartLogger.Appenders.Aggregation;

/// <summary>
/// Default HTTP-based implementation of ILogAggregatorSink.
///
/// This implementation pushes structured log events to an HTTP
/// log aggregation endpoint.
///
/// Example:
///
///     POST https://{your-log-aggregator}/api/logs
///
/// The implementation intentionally knows nothing about
/// SmartLogger's appender pipeline. It only knows how to deliver
/// a LogMessage
/// </summary>
public sealed class HttpLogAggregatorSink : ILogAggregatorSink
{
    /// <summary>
    /// HTTP client used to communicate with the aggregation service.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// HTTP endpoint that accepts log events.
    /// </summary>
    private readonly Uri _endpoint;

    /// <summary>
    /// Creates an HTTP log aggregation sink.
    /// </summary>
    /// <param name="httpClient">
    /// HttpClient used to communicate with the aggregation service.
    /// </param>
    /// <param name="endpoint">
    /// HTTP endpoint that accepts log events.
    /// </param>
    public HttpLogAggregatorSink(HttpClient httpClient, Uri endpoint)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    /// <summary>
    /// Pushes a log entry to the configured aggregation service.
    /// </summary>
    public void Send(LogMessage entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Push the structured log entry to the aggregation service.
        //
        // The aggregator is expected to expose an endpoint such as:
        //
        // POST /api/logs
        //
        // with a JSON body representing LogMessage.
        var response = _httpClient
            .PostAsJsonAsync(_endpoint, entry)
            .GetAwaiter()
            .GetResult();

        // Convert unsuccessful HTTP responses into exceptions.
        //
        // The logging framework can then decide how to handle
        // sink failures.
        response.EnsureSuccessStatusCode();
    }
}