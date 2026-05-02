# Sample Configurations

## 1. Minimal Setup (Default Behavior)

```json
{
  "rootLogLevel": "INFO",
  "appenders": []
}
```

### What this does

* Uses default console appender
* Uses simple layout
* Plain text output
* Synchronous logging

---

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

---

## 4. Custom Pattern

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
        "pattern": "[%LEVEL] %MESSAGE (%THREAD)"
      }
    }
  ]
}
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
          "basePath": "logs/app",
          "extension": "log"
        }
      },
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Detailed"
      },
      "appenderLogLevel": "INFO"
    }
  ]
}
```

### What this does

* Writes logs to `logs/app.log`
* Uses default naming (date + index)
* Uses detailed layout

---

## 6. File Naming Customization

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/payment",
          "extension": "log",
          "naming": {
            "includeDate": true,
            "dateFormat": "yyyy-MM-dd",
            "includeIndex": true,
            "separator": "_"
          }
        }
      }
    }
  ]
}
```

### Output Example

```
logs/payment_2026-01-01_1.log
logs/payment_2026-01-01_2.log
```

---

## 7. Static File (No Date / Index)

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/static",
          "extension": "log",
          "naming": {
            "includeDate": false,
            "includeIndex": false
          }
        }
      }
    }
  ]
}
```

### Output

```
logs/static.log
```

---

# Rolling File Logging

## 8. Size-Based Rolling

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/app"
        }
      },
      "formatter": {
        "outputFormat": "PlainText"
      },
      "file": {
        "rollingPolicy": {
          "rollingType": "Size",
          "maxFileSizeMB": 10,
          "maxRetainedFiles": 5
        }
      }
    }
  ]
}
```

### What this does

* Rolls file when size exceeds 10 MB
* Keeps last 5 files
* Prevents disk overflow

---

## 9. Time-Based Rolling (Daily)

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/app",
          "rollingPolicy": {
            "rollingType": "Time",
            "interval": "Day"
          }
        }
      }
    }
  ]
}
```

### What this does

* Creates a new file every day
* Segments logs by time window

---

## 10. Hybrid Rolling (Coming Soon)

```json
{
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/app",
          "rollingPolicy": {
            "rollingType": "Hybrid",
            "interval": "Day",
            "maxFileSizeMB": 50,
            "maxRetainedFiles": 7
          }
        }
      }
    }
  ]
}
```

---

# JSON Logging

## 11. Default JSON Output

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

---

## 12. JSON with Selected Fields

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "Json",
        "jsonFields": ["timestamp", "level", "message"]
      }
    }
  ]
}
```

---

## 13. JSON with Custom Field Names

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "Json",
        "jsonFields": ["timestamp", "level", "message"],
        "jsonFieldMapping": {
          "timestamp": "@timestamp",
          "level": "severity",
          "message": "msg"
        }
      }
    }
  ]
}
```

---

# Filtering

## 14. Filter by Log Level

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "filter": {
        "minLevel": "INFO",
        "maxLevel": "ERROR"
      }
    }
  ]
}
```

---

## 15. Keyword Filtering

```json
{
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "filter": {
        "includeKeywords": ["Payment"],
        "excludeKeywords": ["HealthCheck"]
      }
    }
  ]
}
```

---

# Multi-Appender Setup

## 16. Console + File (Production)

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "formatter": {
        "outputFormat": "PlainText"
      }
    },
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/app",
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

---

# Logger Overrides

## 17. Per-Component Control

```json
{
  "rootLogLevel": "INFO",
  "loggerOverrides": {
    "SmartLogger.PaymentService": "DEBUG",
    "SmartLogger.Database": "ERROR"
  }
}
```
# Async Logging (Performance Optimization)

## 18. Enable Async Logging

```json
{
  "rootLogLevel": "INFO",
  "enableAsyncLoggingProcess": true,
  "appenders": [
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "basePath": "logs/app",
          "extension": "jsonl"
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

* Moves logging I/O to a background worker thread
* Reduces latency on the main application thread
* Uses an internal queue to buffer log events

---

### When to use

* High-throughput systems (APIs, batch jobs, streaming)
* File or remote logging (I/O heavy operations)
* Production workloads

---

### When NOT to use

* Debugging critical issues (you want immediate visibility)
* Systems where **strict log ordering / durability** is required

---

### Design Note

> SmartLogger is **synchronous by default** for correctness.
> Async mode is an **opt-in optimization**, not the default behavior.

---

# Supported Tokens (PlainText Layout)

```
%TIMESTAMP
%LEVEL
%MESSAGE
%SOURCE
%THREAD
%CORRELATION
```

---

# JSON Configuration Notes

* `jsonFields` → controls which fields are included
* `jsonFieldMapping` → renames fields
* Default fields provide balanced observability

---

# Destination Notes

* `type` → Console | FileSystem | DatabaseSystem
* `file` → required only for FileSystem

---

# File Configuration Notes

* `basePath` → logical file identity
* `extension` → output type (log, json, txt)
* `naming` → controls file naming behavior

---

# Rolling Policy Notes

* `rollingType` → Size | Time | Hybrid
* `maxFileSizeMB` → size-based rotation
* `interval` → Hour | Day | Month
* `maxRetainedFiles` → disk safety

---

# Filter Notes

* `minLevel` / `maxLevel` → level-based filtering
* `includeKeywords` → allow only matching logs
* `excludeKeywords` → drop unwanted logs

---

# Thanks for reading upto the end. Below is the pro tip for you! 

* Use **PlainText + Detailed layout** during development
* Use **JSON output** for observability platforms (ELK, Splunk)
* Use **filters** to reduce noise without code changes
* Combine **file naming + rolling** for production hygiene
* Enable **async logging** for high-throughput systems

---

# If I want to sum up smart logger in one line... it would be defined as follows :) 

> SmartLogger is designed to give you **control without complexity**
> Start simple → scale to production → no redesign needed

---