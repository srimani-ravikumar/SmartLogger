# SmartLogger.Aggregator.Demo

Dev-only dummy log aggregator API. It simulates a real log-aggregator endpoint so
`HttpLogAggregatorSink` can be exercised end-to-end during local development and testing.

> This is **not** production code. The real aggregator integration logic lives client-side
> in `SmartLogger.Appenders.Aggregation.HttpLogAggregatorSink`.

## Storage

All ingested log events are appended as JSON Lines to `dummy-log-aggregtor-db.jsonl`
in the project's content root (one JSON object per line).

## Endpoints

| Method | Route       | Description                                   |
|--------|-------------|------------------------------------------------|
| POST   | `/api/logs` | Accepts a `LogMessage` JSON body, appends it.  |
| GET    | `/api/logs?take=100` | Returns the most recent ingested entries. |
| GET    | `/health`   | Liveness check.                                |

## Running

```powershell
./start-aggregator.ps1
```

The script checks if a process is already listening on port `5290` (default) and
skips starting a duplicate instance if so.

Or run directly:

```powershell
dotnet run --project SmartLogger.Aggregator.Demo.csproj
```
