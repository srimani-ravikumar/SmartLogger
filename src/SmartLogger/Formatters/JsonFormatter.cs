using SmartLogger.Core;
using System.Collections.Generic;
using System.Text.Json;

internal class JsonFormatter : ILogOutputFormatterStrategy
{
    private readonly HashSet<string>? _fields;
    private readonly Dictionary<string, string>? _mapping;
    private readonly JsonSerializerOptions _options;

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

    public string Format(LogMessage message)
    {
        var dict = new Dictionary<string, object?>();

        void Add(string key, object? value)
        {
            if (_fields != null && !_fields.Contains(key))
                return;

            var finalKey = _mapping != null && _mapping.TryGetValue(key, out var mapped)
                ? mapped
                : key;

            dict[finalKey] = value;
        }

        Add("timestamp", message.Timestamp);
        Add("level", message.LogLevel.ToString());
        Add("message", message.Message);
        Add("source", message.Source);
        Add("threadId", message.ThreadId);
        Add("correlationId", message.CorrelationId);

        return JsonSerializer.Serialize(dict, _options);
    }
}