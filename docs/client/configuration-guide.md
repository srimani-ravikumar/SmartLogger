# SmartLogger v1
# Configuration Guide

---

## Document Information

| Project | Version | Date | Author | Status | Description |
|---------|---------|------------|---------|-------------|-------------|
| SmartLogger | 1.0.0 | 2026-07-12 | Srimani | Final | Demonstrates common SmartLogger configuration scenarios, recommended practices, and production-ready configuration examples. |

---

# Introduction

SmartLogger is designed around **Convention over Configuration**.

The default configuration is sufficient for most applications while still allowing advanced customization through a clean and extensible configuration model.

This guide demonstrates the most common configuration scenarios used in development and production environments.

---

# 1. Minimal Setup (Default Behavior)

```json
{
  "rootLogLevel": "INFO",
  "appenders": []
}
```

## Default Behavior

When no appenders are configured, SmartLogger automatically enables a default Console Appender.

The framework uses:

- Console Appender
- Plain Text Output
- Simple Layout
- Synchronous Logging
- Root Log Level = INFO

No additional configuration is required.

---

# Console Logging

## 2. Console Logging (Simple Layout)

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Simple"
      },
      "appenderLogLevel": "DEBUG"
    }
  ]
}
```

### What this does

- Writes logs to the console.
- Uses the Simple layout.
- Outputs plain text.
- Appender accepts DEBUG and above.

---

## 3. Console Logging (Detailed Layout)

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Detailed"
      }
    }
  ]
}
```

### What this does

Produces additional diagnostic information such as

- Thread Id
- Correlation Id
- Source
- Timestamp

Useful during development and troubleshooting.

---

## 4. Custom Pattern Layout

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Custom",
        "pattern": "[%LEVEL] [%THREAD] [%CORRELATION] >> %MESSAGE"
      }
    }
  ]
}
```

### What this does

Allows complete control over the rendered log output.

Example

```
[INFO] [12] [REQ-1023] >> Payment processed successfully
```

---

# File Logging

## 5. Basic File Logging

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log"
        }
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Detailed"
      }
    }
  ]
}
```

### Default Behavior

Because SmartLogger follows **Convention over Configuration**, the remaining settings are automatically applied.

```
Rolling Strategy
    Daily

Naming Strategy
    Date

Archive
    Enabled

Compression
    Enabled

Retention
    30 Days
```

Generated files

```
Logs

Application.log

Archive

Application_2026-07-12.zip
```

---

## 6. Custom File Naming

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "PaymentService",
          "extension": "log",
          "naming": {
            "strategy": "Date",
            "dateFormat": "yyyy-MM-dd"
          }
        }
      }
    }
  ]
}
```

### Generated Files

```
Logs

PaymentService.log
```

After rolling

```
Archive

PaymentService_2026-07-12.zip
```

---

## 7. Timestamp Naming Strategy

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Orders",
          "extension": "log",
          "naming": {
            "strategy": "Timestamp"
          }
        }
      }
    }
  ]
}
```

### Notes

Timestamp-based naming provides higher uniqueness and is useful for high-frequency rolling scenarios.

---

# File Naming Notes

| Property | Description |
|------------|-------------|
| directory | Directory where the active log file is stored |
| fileName | Logical name of the active log file |
| extension | File extension without '.' |
| naming.strategy | Determines how rolled files are named |
| naming.dateFormat | Date format used by the Date naming strategy |

---

# Design Notes

The File Naming Strategy is responsible only for determining **what a rolled log file should be called**.

It does **not**

- determine when rolling occurs
- check whether files already exist
- archive files
- compress archives

Those responsibilities belong to the **FileLifecycleManager**.

---

# Rolling File Logging

SmartLogger supports two rolling strategies in v1.

- **Daily Rolling** (Default)
- **Size-Based Rolling**

Rolling is evaluated lazily whenever a new log entry arrives.

No timers.

No background services.

No scheduler.

---

# 8. Daily Rolling (Default)

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log",
          "rolling": {
            "strategy": "Daily"
          }
        }
      }
    }
  ]
}
```

### What this does

- Uses the same active log file throughout the day.
- Creates a new archive when the date changes.
- Automatically creates a fresh active log file.

Example

```
Day 1

Logs

Application.log

↓

Roll

↓

Archive

Application_2026-07-12.zip

↓

Logs

Application.log
```

---

# 9. Size-Based Rolling

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log",
          "rolling": {
            "strategy": "Size",
            "maxFileSizeMB": 10
          }
        }
      }
    }
  ]
}
```

### What this does

- Rolls whenever the active log exceeds **10 MB**.
- Automatically archives the previous file.
- Starts writing into a fresh active log file.

Useful for

- High-throughput applications
- APIs
- Long-running Windows Services
- Background Workers

---

# Rolling Strategy Notes

| Property | Description |
|------------|-------------|
| strategy | Rolling strategy (Daily or Size) |
| maxFileSizeMB | Maximum active log file size before rolling (Size strategy only) |

---

# Rolling Design

A rolling strategy has only one responsibility.

```
Should the current active file be rolled?
```

It never

- generates file names
- archives files
- compresses files
- deletes archives

It simply returns

```
true

or

false
```

---

# Archive Configuration

By default, SmartLogger archives every rolled log file.

## 10. Default Archive

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "archive": {
            "enabled": true
          }
        }
      }
    }
  ]
}
```

Default archive directory

```
Logs

Archive
```

Generated structure

```
Logs

Application.log

Archive

Application_2026-07-12.zip
```

---

## 11. Custom Archive Directory

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "archive": {
            "enabled": true,
            "directory": "ArchivedLogs",
            "compress": true
          }
        }
      }
    }
  ]
}
```

Generated structure

```
Logs

Application.log

ArchivedLogs

Application_2026-07-12.zip
```

---

# Archive Configuration Notes

| Property | Description |
|------------|-------------|
| enabled | Enables archive support |
| directory | Archive folder location |
| compress | Compress archived files into ZIP format |

---

# Compression

Compression is enabled by default.

Immediately after a rolling event

```
Application.log

↓

Application_2026-07-12.log

↓

Application_2026-07-12.zip

↓

Delete Application_2026-07-12.log
```

This keeps the archive directory small and efficient.

---

## 12. Disable Compression

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "archive": {
            "enabled": true,
            "compress": false
          }
        }
      }
    }
  ]
}
```

Generated structure

```
Archive

Application_2026-07-12.log
```

instead of

```
Application_2026-07-12.zip
```

---

# Retention Policy

SmartLogger automatically removes expired archived log files.

Retention executes only during rolling.

No timers.

No background cleanup service.

---

## 13. Default Retention

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "retention": {
            "retentionDays": 30
          }
        }
      }
    }
  ]
}
```

### What this does

During every rolling event

```
Roll

↓

Archive

↓

Compress

↓

Delete ZIP files older than 30 days
```

---

## 14. Custom Retention

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "retention": {
            "retentionDays": 90
          }
        }
      }
    }
  ]
}
```

Useful for

- Audit systems
- Banking applications
- Compliance requirements
- Long-term diagnostics

---

# File Lifecycle

Every log write follows the same predictable lifecycle.

```
Write Log

↓

Ensure Active File

↓

Need Roll?

↓

No
    Write Message

↓

Yes

Archive Active File

↓

Compress Archive

↓

Cleanup Old Archives

↓

Create Fresh Active File

↓

Write Message
```

The entire lifecycle is protected by a single synchronization lock, ensuring thread-safe and ordered log writes.

---

# File Lifecycle Design Notes

SmartLogger follows a **Hybrid (Lazy Rolling + Locking)** approach.

Rolling is evaluated only when a new log entry arrives.

Benefits

- No scheduler
- No timer
- No polling
- Minimal synchronization
- Thread-safe rolling
- Ordered log writes

This design keeps the implementation lightweight while remaining predictable under concurrent workloads.

---

# Structured Logging

SmartLogger supports structured logging through JSON output.

JSON logging is recommended for production systems where logs are consumed by tools such as

- ELK Stack
- OpenSearch
- Splunk
- Azure Monitor
- Grafana Loki

---

# 15. Default JSON Logging

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "Json"
      }
    }
  ]
}
```

### Example Output

```json
{
  "timestamp": "2026-07-12T10:15:43.127Z",
  "level": "INFO",
  "thread": 8,
  "correlation": "REQ-1024",
  "source": "OrderService",
  "message": "Order processed successfully."
}
```

---

# 16. JSON with Selected Fields

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "Json",
        "includedJsonFields": [
          "timestamp",
          "level",
          "message"
        ]
      }
    }
  ]
}
```

### Example Output

```json
{
  "timestamp": "2026-07-12T10:15:43.127Z",
  "level": "INFO",
  "message": "Order processed successfully."
}
```

Useful when reducing payload size or integrating with systems that require only essential fields.

---

# 17. JSON with Custom Field Names

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "Json",
        "includedJsonFields": [
          "timestamp",
          "level",
          "message"
        ],
        "jsonFieldMappings": [
          {
            "sourceField": "timestamp",
            "targetField": "@timestamp"
          },
          {
            "sourceField": "level",
            "targetField": "severity"
          },
          {
            "sourceField": "message",
            "targetField": "msg"
          }
        ]
      }
    }
  ]
}
```

### Example Output

```json
{
  "@timestamp": "2026-07-12T10:15:43.127Z",
  "severity": "INFO",
  "msg": "Order processed successfully."
}
```

Useful when integrating with external observability platforms.

---

# JSON Configuration Notes

| Property | Description |
|------------|-------------|
| outputFormat | PlainText, Json or Xml |
| includedJsonFields | Controls which fields appear in the JSON output |
| jsonFieldMappings | Renames JSON property names |
| layoutType | Ignored for JSON output |

---

# Logger Overrides

Logger overrides allow specific loggers to use different log levels without affecting the global root log level.

---

# 18. Logger Overrides

```json
{
  "rootLogLevel": "INFO",

  "loggerOverrides": [
    {
      "loggerName": "SmartLogger.PaymentService",
      "logLevel": "DEBUG"
    },
    {
      "loggerName": "SmartLogger.Database",
      "logLevel": "ERROR"
    }
  ]
}
```

### Effective Log Levels

| Logger | Effective Level |
|---------|-----------------|
| Root | INFO |
| SmartLogger.PaymentService | DEBUG |
| SmartLogger.Database | ERROR |

Logger overrides are evaluated before the Root Log Level.

---

# Multi-Appender Configuration

A single logger may write to multiple destinations simultaneously.

Each appender maintains its own

- Destination
- Formatter
- Log Level

---

# 19. Console + File

```json
{
  "rootLogLevel": "DEBUG",

  "appenders": [

    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Simple"
      }
    },

    {
      "destination": {
        "type": "FileSystem",

        "file": {

          "directory": "Logs",

          "fileName": "Application",

          "extension": "log",

          "rolling": {
            "strategy": "Daily"
          },

          "archive": {
            "enabled": true,
            "compress": true
          }
        }
      },

      "formatter": {
        "outputFormat": "Json"
      }
    }
  ]
}
```

### What this does

Console

- Plain Text
- Simple Layout

File

- JSON Output
- Daily Rolling
- ZIP Compression
- 30 Day Retention

---

# Async Logging

By default SmartLogger performs synchronous logging.

Async logging can be enabled when maximum throughput is required.

---

# 20. Enable Async Logging

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

          "extension": "json"
        }
      },

      "formatter": {
        "outputFormat": "Json"
      }
    }
  ]
}
```

### What this does

- Moves log processing to a background worker.
- Improves application responsiveness.
- Suitable for high-throughput applications.

---

# When to use Async Logging

Recommended for

- ASP.NET Core APIs
- Worker Services
- Windows Services
- Batch Processing
- High-volume Applications

---

# When NOT to use Async Logging

Avoid Async Logging when

- Immediate log durability is required.
- Debugging startup failures.
- Diagnosing application crashes.

---

# Design Note

SmartLogger is intentionally **synchronous by default**.

Async logging is an **opt-in performance optimization**, ensuring correctness remains the default behavior.

---

# Supported Plain Text Tokens

When using the **Custom** layout, SmartLogger supports the following built-in tokens.

| Token | Description |
|---------|-------------|
| `%TIMESTAMP` | Timestamp of the log event |
| `%LEVEL` | Log level (DEBUG, INFO, etc.) |
| `%MESSAGE` | Log message |
| `%SOURCE` | Logger source (class or logger name) |
| `%THREAD` | Managed thread identifier |
| `%CORRELATION` | Current correlation identifier |

---

# Sample Custom Pattern

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Custom",
        "pattern": "[%TIMESTAMP] [%LEVEL] [%THREAD] [%CORRELATION] %MESSAGE"
      }
    }
  ]
}
```

Example Output

```
[2026-07-12 10:35:42.815]
[INFO]
[12]
[REQ-1001]
Payment completed successfully.
```

---

# Root Configuration Reference

| Property | Description | Default |
|------------|-------------|----------|
| rootLogLevel | Default log level | INFO |
| loggerOverrides | Logger-specific log levels | Empty |
| appenders | Configured appenders | Empty (Console added automatically) |
| enableAsyncLoggingProcess | Enables async logging | false |

---

# Appender Configuration Reference

| Property | Description |
|------------|-------------|
| destination | Where logs are written |
| formatter | Controls output formatting |
| filter | Reserved for future versions |
| appenderLogLevel | Overrides RootLogLevel for this appender |

---

# Destination Configuration Reference

| Property | Description |
|------------|-------------|
| type | Console, FileSystem or DatabaseSystem |
| file | File configuration (required for FileSystem) |
| database | Reserved for future versions |

---

# Formatter Configuration Reference

| Property | Description | Default |
|------------|-------------|----------|
| outputFormat | PlainText, Json or Xml | PlainText |
| layoutType | Simple, Detailed or Custom | Simple |
| pattern | Custom layout pattern | Empty |
| includedJsonFields | Fields included in JSON output | Default fields |
| jsonFieldMappings | Renames JSON fields | Empty |

---

# File Configuration Reference

| Property | Description | Default |
|------------|-------------|----------|
| directory | Active log directory | Logs |
| fileName | Active log file name | Application |
| extension | File extension | log |
| naming | File naming configuration | Date Strategy |
| rolling | Rolling configuration | Daily |
| archive | Archive configuration | Enabled |
| retention | Retention configuration | 30 Days |

---

# File Naming Configuration

| Property | Description | Default |
|------------|-------------|----------|
| strategy | File naming strategy | Date |
| dateFormat | Date format used for rolled files | yyyy-MM-dd |

---

# Rolling Configuration

| Property | Description | Default |
|------------|-------------|----------|
| strategy | Daily or Size | Daily |
| maxFileSizeMB | Maximum size before rolling | 10 |

---

# Archive Configuration

| Property | Description | Default |
|------------|-------------|----------|
| enabled | Enables archive support | true |
| directory | Archive directory | Archive |
| compress | Compress rolled logs | true |

---

# Retention Configuration

| Property | Description | Default |
|------------|-------------|----------|
| retentionDays | Number of days to retain archived logs | 30 |

---

# Logger Override Notes

Logger overrides always take precedence over the Root Log Level.

Example

```
Root

INFO

↓

PaymentService

DEBUG

↓

Database

ERROR
```

This allows individual components to emit more or less information without affecting the rest of the application.

---

# Logging Pipeline

Every log request passes through the following pipeline.

```
Application

↓

Logger

↓

Log Level Resolution

↓

Appender Selection

↓

Formatter

↓

FileLifecycleManager

↓

Ensure Active File

↓

Need Roll?

↓

Archive

↓

Compress

↓

Retention Cleanup

↓

Write Log
```

This pipeline remains identical regardless of whether logging is synchronous or asynchronous.

---

# Best Practices

## Development

Recommended

- Console Appender
- Plain Text
- Detailed Layout
- DEBUG Log Level

Example

```json
{
  "rootLogLevel": "DEBUG"
}
```

---

## Production

Recommended

- File Appender
- JSON Output
- Daily Rolling
- ZIP Compression
- 30 Day Retention
- INFO Log Level

This provides an excellent balance between observability and storage efficiency.

---

## High Throughput Applications

Recommended

- Async Logging
- JSON Output
- Size-Based Rolling

Ideal for

- ASP.NET Core APIs
- Worker Services
- Event Processors
- Streaming Applications

---

## Long Running Services

Recommended

- Daily Rolling
- Archive Enabled
- Compression Enabled
- 90 Day Retention (if required)

Suitable for

- Windows Services
- Background Services
- Scheduled Jobs

---

# Production Recommendations

✔ Prefer JSON output for machine-readable logs.

✔ Keep PlainText for local debugging.

✔ Use Daily rolling unless log volume is exceptionally high.

✔ Enable compression for production deployments.

✔ Increase retention only when required by compliance or audit policies.

✔ Prefer logger overrides over globally increasing the Root Log Level.

✔ Use correlation identifiers for distributed request tracing.

---

# Common Mistakes

❌ Using `.log` instead of `log` for the extension.

Correct

```json
"extension": "log"
```

Incorrect

```json
"extension": ".log"
```

---

❌ Disabling archive while expecting historical logs.

If archive is disabled, rolled log files are not preserved.

---

❌ Setting an extremely small `maxFileSizeMB`.

Very small sizes may cause excessive rolling and unnecessary disk activity.

---

❌ Setting the Root Log Level to DEBUG in production.

Use logger overrides instead for components that require detailed diagnostics.

---

# Frequently Asked Questions

## Does SmartLogger use background timers for rolling?

No.

Rolling is evaluated lazily whenever a new log message arrives.

---

## Does SmartLogger create multiple active log files?

No.

There is always a single active log file.

Older files are archived after rolling.

---

## When does retention cleanup execute?

Immediately after a successful rolling operation.

No scheduler or timer is used.

---

## Can I add my own rolling strategy?

Yes.

Implement

```
IRollingStrategy
```

and register it within the framework.

---

## Can I implement my own naming strategy?

Yes.

Implement

```
IFileNamingStrategy
```

to generate custom rolled file names.

---

## Does SmartLogger support multiple appenders?

Yes.

Each appender

- maintains its own formatter
- has its own log level
- operates independently

---

# Configuration Philosophy

SmartLogger follows a few simple principles.

- Convention over Configuration
- Sensible Defaults
- Predictable Behavior
- Fail Fast
- Open for Extension
- Keep It Simple

The framework aims to reduce configuration complexity while remaining flexible enough for production environments.

---

# Final Thoughts

SmartLogger is designed to help developers focus on their applications rather than logging infrastructure.

Whether you're building a small console application or a production-grade distributed service, the same configuration model scales naturally without requiring architectural changes.

Start with the defaults.

Customize only when necessary.

---

# If I had to summarize SmartLogger in one sentence...

> **SmartLogger provides production-ready logging with sensible defaults, clean architecture, and extensibility—without the complexity commonly found in traditional logging frameworks.**

---

<p align="center">
<strong>© 2026 Srimani. All rights reserved.</strong>
</p>