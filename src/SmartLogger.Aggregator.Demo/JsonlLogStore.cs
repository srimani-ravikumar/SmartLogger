namespace SmartLogger.Aggregator.Demo;

/// <summary>
/// Dummy, file-backed log store used to simulate a real log-aggregator's persistence layer.
/// Not intended for production use - see SmartLogger.Appenders.Aggregation.HttpLogAggregatorSink
/// for the real client-side integration.
/// </summary>
internal sealed class JsonlLogStore
{
    private const string FileName = "dummy-log-aggregtor-db.jsonl";
    private readonly string _filePath;
    private readonly object _lock = new();

    public JsonlLogStore(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, FileName);
    }

    /// <summary>Appends a single JSON line (the raw request body) to the store.</summary>
    public int Append(string jsonLine)
    {
        var line = jsonLine.Replace("\r", string.Empty).Replace("\n", string.Empty);

        lock (_lock)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }

        return 1;
    }

    /// <summary>Reads the last N lines from the store, oldest first.</summary>
    public IReadOnlyList<string> ReadRecent(int take)
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                return Array.Empty<string>();
            }

            var lines = File.ReadAllLines(_filePath);
            return lines.Length <= take ? lines : lines[^take..];
        }
    }
}
