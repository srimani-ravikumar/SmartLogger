using System.Text.Json;
using SmartLogger.Aggregator.Demo;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<JsonlLogStore>();

var app = builder.Build();

// Simulated real log-aggregator behaviour: accept one or many log events, append to a JSONL file.
app.MapPost("/api/logs", async (HttpRequest request, JsonlLogStore store) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(body))
    {
        return Results.BadRequest(new { error = "Request body cannot be empty." });
    }

    var written = store.Append(body);
    return Results.Accepted(value: new { stored = written });
});

// Fetch recently ingested log events (most recent last), useful for manual verification during dev testing.
app.MapGet("/api/logs", (int? take, JsonlLogStore store) =>
{
    var entries = store.ReadRecent(take ?? 100);
    return Results.Ok(entries);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
