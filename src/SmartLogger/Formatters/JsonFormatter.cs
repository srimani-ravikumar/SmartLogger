using SmartLogger.Core;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// JSON-based implementation of <see cref="ILogOutputFormatterStrategy"/>.
/// </summary>
/// <remarks>
/// Serializes <see cref="LogMessage"/> into a JSON object with optional:
/// <list type="bullet">
/// <item><description>Field filtering (via <c>fields</c>)</description></item>
/// <item><description>Field name mapping (via <c>mapping</c>)</description></item>
/// </list>
/// 
/// This formatter bypasses layout/token rendering and directly serializes structured data.
/// </remarks>
internal class JsonFormatter : ILogOutputFormatterStrategy
{
    /// <summary>
    /// Optional set of fields to include in the output.
    /// </summary>
    /// <remarks>
    /// If null, all fields are included.
    /// </remarks>
    private readonly HashSet<string>? _fields;

    /// <summary>
    /// Optional mapping of field names (e.g., "timestamp" → "ts").
    /// </summary>
    private readonly Dictionary<string, string>? _mapping;

    /// <summary>
    /// JSON serialization options.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormatter"/> class.
    /// </summary>
    /// <param name="fields">Optional list of fields to include.</param>
    /// <param name="mapping">Optional field name mapping.</param>
    /// <param name="prettyPrint">Whether JSON output should be indented.</param>
    public JsonFormatter(
        List<string>? fields,
        Dictionary<string, string>? mapping,
        bool prettyPrint = false)
    {
        _fields = fields != null && fields.Count > 0
            ? new HashSet<string>(fields)
            : null;

        _mapping = mapping;

        _options = new JsonSerializerOptions
        {
            WriteIndented = prettyPrint
        };
    }

    /// <inheritdoc/>
    public string Format(LogMessage message)
    {
        var dict = new Dictionary<string, object?>();

        // Local helper to apply filtering + mapping logic
        void Add(string key, object? value)
        {
            // Skip if field filtering is enabled and key not included
            if (_fields != null && !_fields.Contains(key))
                return;

            // Apply optional field name mapping
            var finalKey = _mapping != null && _mapping.TryGetValue(key, out var mapped)
                ? mapped
                : key;

            dict[finalKey] = value;
        }

        // Populate structured fields
        Add("timestamp", message.Timestamp);
        Add("level", message.LogLevel.ToString());
        Add("message", message.Message);
        Add("source", message.Source);
        Add("thread", message.ThreadId);
        Add("correlation", message.CorrelationId);

        return JsonSerializer.Serialize(dict, _options);
    }
}