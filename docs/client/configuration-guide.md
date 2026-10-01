# SmartLogger

# Configuration Guide
# How to use this guide

SmartLogger follows **Convention over Configuration** - you only specify what differs from the sensible defaults.

- Find the use case closest to yours below, copy the JSON, adjust names/paths.
- Need a specific property explained? Jump to [Reference Tables](#reference-tables).
- Need the full token list for custom patterns? See [Custom Pattern Tokens](#custom-pattern-tokens).

# Use Case Recipes

## Local Development - Console, Verbose

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": { "type": "Console" },
      "formatter": { "outputFormat": "PlainText", "layoutType": "Detailed" }
    }
  ]
}
```
Readable console output with thread, correlation and source info - ideal while coding and debugging locally.

## Minimal / Zero Config

```json
{
  "rootLogLevel": "INFO"
}
```
No appenders configured? SmartLogger auto-enables a Console Appender (PlainText, Simple layout, sync). Nothing else to do.

## Production - File Logging, JSON, Rolling

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log",
          "rolling": { "strategy": "Daily" },
          "archive": { "enabled": true, "compress": true },
          "retention": { "retentionDays": 30 }
        }
      },
      "formatter": { "outputFormat": "Json" }
    }
  ]
}
```
Daily rolling -> ZIP archive -> 30-day cleanup, all automatic. Recommended baseline for any production service.

## Console + File (Common Combo)

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": { "type": "Console" },
      "formatter": { "outputFormat": "PlainText", "layoutType": "Simple" }
    },
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log",
          "rolling": { "strategy": "Daily" },
          "archive": { "enabled": true, "compress": true }
        }
      },
      "formatter": { "outputFormat": "Json" }
    }
  ]
}
```
Simple console for live viewing, structured JSON file for machines (ELK/Splunk/Grafana Loki).

## Remote Log Aggregator (Centralized Sink)

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": {
        "type": "LogAggregator",
        "logAggregator": {
          "useDefault": true,
          "endpoint": "http://localhost:5290/api/logs"
        }
      },
      "formatter": { "outputFormat": "Json" }
    }
  ]
}
```
Ships every log event to a centralized HTTP aggregator instead of (or alongside) local files - useful once services are distributed across multiple machines/containers.

* `useDefault: true` uses the built-in `HttpLogAggregatorSink` (just point `endpoint` at your aggregator).
* `useDefault: false` lets you plug in your own `ILogAggregatorSink` implementation.
* Spin up `SmartLogger.Aggregator.Demo` locally to try this recipe end-to-end before wiring a real aggregator.

## High-Throughput APIs / Workers

```json
{
  "rootLogLevel": "INFO",
  "enableAsyncLoggingProcess": true,
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "json",
          "rolling": { "strategy": "Size", "maxFileSizeMB": 10 }
        }
      },
      "formatter": { "outputFormat": "Json" }
    }
  ]
}
```
Background worker writes logs, so request threads aren't blocked. Rolls at 10MB instead of waiting for the day to end.

Note: avoid async logging when you need immediate durability (e.g. debugging a crashing startup).

## Long-Running / Compliance Services

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "archive": { "enabled": true, "compress": true },
          "retention": { "retentionDays": 90 }
        }
      }
    }
  ]
}
```
Same as production baseline, longer retention - useful for audit/banking/compliance needs.

## Per-Component Log Levels (Overrides)

```json
{
  "rootLogLevel": "INFO",
  "loggerOverrides": [
    { "loggerName": "SmartLogger.PaymentService", "logLevel": "DEBUG" },
    { "loggerName": "SmartLogger.Database", "logLevel": "ERROR" }
  ]
}
```
Turn up noise for one component without changing the global level. Overrides always win over `rootLogLevel`.

## Custom Console Pattern

```json
{
  "appenders": [
    {
      "destination": { "type": "Console" },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Custom",
        "pattern": "[%LEVEL] [%THREAD] [%CORRELATION] >> %MESSAGE"
      }
    }
  ]
}
```
Output: `[INFO] [12] [REQ-1023] >> Payment processed successfully`. Token list [here](#custom-pattern-tokens).

## JSON With Trimmed / Renamed Fields

```json
{
  "appenders": [
    {
      "destination": { "type": "Console" },
      "formatter": {
        "outputFormat": "Json",
        "includedJsonFields": ["timestamp", "level", "message"],
        "jsonFieldMappings": [
          { "sourceField": "timestamp", "targetField": "@timestamp" },
          { "sourceField": "level", "targetField": "severity" },
          { "sourceField": "message", "targetField": "msg" }
        ]
      }
    }
  ]
}
```
Output: `{"@timestamp": "...", "severity": "INFO", "msg": "..."}` - handy when integrating with an external observability schema.

# Custom Pattern Tokens

| Token | Description |
|---------|-------------|
| `%TIMESTAMP` | Timestamp of the log event |
| `%LEVEL` | Log level (DEBUG, INFO, etc.) |
| `%MESSAGE` | Log message |
| `%SOURCE` | Logger source (class or logger name) |
| `%THREAD` | Managed thread identifier |
| `%CORRELATION` | Current correlation identifier |

# Best Practices at a Glance

- JSON output for machine-readable production logs, PlainText for local debugging.
- Daily rolling unless volume is exceptionally high - then Size-based.
- Keep compression on; disable only if you need raw `.log` archives.
- Prefer logger overrides over raising the global root level.
- Use correlation IDs for distributed request tracing.

### Common Mistakes

* Not: `"extension": ".log"` -> should be `"extension": "log"` (no dot).
* Not: Disabling `archive` while still expecting historical logs to exist.
* Not: Setting `maxFileSizeMB` too small -> excessive rolling/disk churn.
* Not: Root level `DEBUG` in production -> use `loggerOverrides` instead.

---

# FAQ

* **Does rolling use background timers?** No - evaluated lazily on each log write.
* **Multiple active log files?** No - always one active file; older ones are archived.
* **When does retention cleanup run?** Immediately after a successful roll, no scheduler involved.
* **Can I plug in my own rolling/naming strategy?** Yes - implement `IRollingStrategy` / `IFileNamingStrategy` and register it.

# Reference Tables

## Root Configuration

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| rootLogLevel | `LogLevel` | Default log level | INFO |
| loggerOverrides | `List<LoggerOverrideConfiguration>` | Logger-specific log levels | Empty |
| appenders | `List<AppenderConfiguration>` | Configured appenders | Empty (Console added automatically) |
| enableAsyncLoggingProcess | `bool` | Enables async logging | false |

## Appender

| Property | Type | Description |
|------------|--------|-------------|
| destination | `DestinationConfiguration` | Where logs are written |
| formatter | `FormatterConfiguration` | Controls output formatting |
| filter | `object?` | Reserved for future versions |
| appenderLogLevel | `LogLevel?` | Overrides RootLogLevel for this appender |

## Destination

| Property | Type | Description |
|------------|--------|-------------|
| type | `LogOutputDestination` | Console, FileSystem, LogAggregator or DatabaseSystem |
| file | `FileConfiguration?` | File configuration (required for FileSystem) |
| logAggregator | `LogAggregatorConfiguration?` | Aggregator configuration (required for LogAggregator) |
| database | `object?` | Reserved for future versions |

## Log Aggregator

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| useDefault | `bool` | Use the built-in HTTP sink (`HttpLogAggregatorSink`) | true |
| endpoint | `Uri?` | HTTP endpoint the default sink posts logs to | Required when useDefault is true |

Set `useDefault: false` to supply your own `ILogAggregatorSink` instead of the built-in HTTP sink.

## Formatter

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| outputFormat | `LogOutputFormat` | PlainText, Json or Xml | PlainText |
| layoutType | `LogMessageLayoutType` | Simple, Detailed or Custom | Simple |
| pattern | `string` | Custom layout pattern | Empty |
| includedJsonFields | `List<string>` | Fields included in JSON output | Default fields |
| jsonFieldMappings | `List<JsonFieldMappingConfiguration>` | Renames JSON fields | Empty |

## File

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| directory | `string` | Active log directory | Logs |
| fileName | `string` | Active log file name | Application |
| extension | `string` | File extension (no leading dot) | log |
| naming | `FileNamingConfiguration` | File naming configuration | Date Strategy |
| rolling | `FileRollingConfiguration` | Rolling configuration | Daily |
| archive | `ArchiveConfiguration` | Archive configuration | Enabled |
| retention | `RetentionConfiguration` | Retention configuration | 30 Days |

## File Naming

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| strategy | `FileNamingStrategyType` | Date or Timestamp | Date |
| dateFormat | `string` | Date format used for rolled files | yyyy-MM-dd |

Naming only decides *what a rolled file is called* - rolling timing, archiving and compression are handled by the `FileLifecycleManager`.

## Rolling

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| strategy | `RollingStrategyType` | Daily or Size | Daily |
| maxFileSizeMB | `long` | Max size before rolling (Size strategy only) | 10 |

A rolling strategy only answers *"should the active file be rolled?"* - it never names, archives, or compresses files.

## Archive

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| enabled | `bool` | Enables archive support | true |
| directory | `string` | Archive directory | Archive |
| compress | `bool` | Compress rolled logs into ZIP | true |

## Retention

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| retentionDays | `int` | Days to retain archived logs | 30 |


# Our SmartLogger Pipeline (for the curious)

```
Logger
  │
  ▼
Log Level Resolution → Appender Selection → Formatter
                                              │
                                              ▼
FileLifecycleManager ← Ensure Active File ← Need Roll?
  │
  ▼
Archive → Compress → Retention Cleanup → Write Log
```

Same pipeline for sync and async logging - async just moves the tail end onto a background worker. Single lock guards the lifecycle, so writes stay thread-safe and ordered with no timers/schedulers anywhere.

<p align="center">
<strong>(c) 2026 Srimani. All rights reserved.</strong>
</p>
