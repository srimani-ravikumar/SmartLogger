Here’s your **updated `config-guide.md`**, aligned with your **new strongly-typed FileConfiguration + Naming + Rolling design**.
Clean, production-grade, no legacy leakage.

---

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

---

## 2. Console Logging (Simple Layout)

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": "Console",
      "threshold": "DEBUG",
      "outputFormat": "PlainText",
      "layoutType": "Simple"
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
      "destination": "Console",
      "threshold": "DEBUG",
      "outputFormat": "PlainText",
      "layoutType": "Detailed"
    }
  ]
}
```

---

## 4. Custom Pattern

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": "Console",
      "outputFormat": "PlainText",
      "layoutType": "Custom",
      "pattern": "[%LEVEL] %MESSAGE (%THREAD)"
    }
  ]
}
```

---

# File Logging (New Design)

## 5. Basic File Logging

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    {
      "destination": "FileSystem",
      "threshold": "INFO",
      "outputFormat": "PlainText",
      "layoutType": "Detailed",
      "file": {
        "basePath": "logs/app",
        "extension": "log"
      }
    }
  ]
}
```

### What this does

* Writes logs to `logs/app.log`
* Uses default naming (date + index enabled)
* Uses detailed layout

---

## 6. File Naming Customization

```json
{
  "appenders": [
    {
      "destination": "FileSystem",
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
  ]
}
```

### Output Example

```
logs/payment_2026-01-01_1.log
logs/payment_2026-01-01_2.log
```

---

## 7. Disable Date / Index (Static File)

```json
{
  "appenders": [
    {
      "destination": "FileSystem",
      "file": {
        "basePath": "logs/static",
        "extension": "log",
        "naming": {
          "includeDate": false,
          "includeIndex": false
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
      "destination": "FileSystem",
      "file": {
        "basePath": "logs/app",
        "extension": "log"
      },
      "rollingPolicy": {
        "rollingType": "Size",
        "maxFileSizeMB": 10,
        "maxRetainedFiles": 5
      }
    }
  ]
}
```

### What this does

* Rolls when file exceeds 10 MB
* Keeps last 5 files
* Prevents disk overflow

---

## 9. Time-Based Rolling (Daily)

```json
{
  "appenders": [
    {
      "destination": "FileSystem",
      "file": {
        "basePath": "logs/app",
        "extension": "log"
      },
      "rollingPolicy": {
        "rollingType": "Time",
        "interval": "Day"
      }
    }
  ]
}
```

### What this does

* Creates a new file every day
* Automatically segments logs by time window

---

## 10. Hybrid Rolling (Time + Size)

```json
{
  "appenders": [
    {
      "destination": "FileSystem",
      "file": {
        "basePath": "logs/app",
        "extension": "log"
      },
      "rollingPolicy": {
        "rollingType": "Hybrid",
        "interval": "Day",
        "maxFileSizeMB": 50,
        "maxRetainedFiles": 7
      }
    }
  ]
}
```

### What this does

* Rolls daily OR on size breach
* Best suited for production workloads

---

# JSON Logging

## 11. Default JSON Output

```json
{
  "appenders": [
    {
      "destination": "Console",
      "outputFormat": "Json"
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
      "destination": "Console",
      "outputFormat": "Json",
      "jsonFields": ["timestamp", "level", "message"]
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
      "destination": "Console",
      "outputFormat": "Json",
      "jsonFields": ["timestamp", "level", "message"],
      "jsonFieldMapping": {
        "timestamp": "@timestamp",
        "level": "severity",
        "message": "msg"
      }
    }
  ]
}
```

---

# Multi-Appender Setup

## 14. Console + File (Production)

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": "Console",
      "outputFormat": "PlainText",
      "layoutType": "Simple"
    },
    {
      "destination": "FileSystem",
      "outputFormat": "Json",
      "file": {
        "basePath": "logs/app",
        "extension": "json"
      }
    }
  ]
}
```

---

# Logger Overrides

## 15. Per-Component Logging Control

```json
{
  "rootLogLevel": "INFO",
  "loggerOverrides": {
    "SmartLogger.PaymentService": "DEBUG",
    "SmartLogger.Database": "ERROR"
  }
}
```

---

# Supported Tokens (PlainText Layouts)

```text
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
* `jsonFieldMapping` → renames output fields
* If not provided → all fields included

---

# File Configuration Notes

* `basePath` → logical file identity (without extension)
* `extension` → output format (log, json, txt)
* `naming.includeDate` → enables time-based file grouping
* `naming.includeIndex` → enables rolling index suffix
* `separator` → controls readability of file names

---

# Rolling Policy Notes

* `rollingType` → Size | Time | Hybrid
* `maxFileSizeMB` → triggers size rotation
* `interval` → Hour | Day | Month
* `maxRetainedFiles` → prevents disk exhaustion

---

# Thanks for reading upto the end. Below is the pro tip for you! 

* Use **PlainText + Detailed layout** during development
* Use **JSON output** for observability systems (ELK, Splunk, etc.)
* Use **File naming + rolling together** for clean log lifecycle
* Always configure **retention** in production environments

---