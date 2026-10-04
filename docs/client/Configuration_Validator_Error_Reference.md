# Configuration Validator - Error Reference Card

Quick reference of common validation errors and how to fix them.

## Format

Each error follows this consistent format:

```
[ERROR ISSUE STATEMENT]

REASON:
[Why this is a problem for the system]

FIX:
[Concrete, actionable steps to resolve]

LEARN MORE:
[Reference to configuration-guide.md or configuration-reference.md]
```

---

## Common Validation Errors

### 1. Duplicate Appender Destinations

**Error Message**:
```
Duplicate appender destinations detected: Console, FileSystem (at appender indices: 0, 1).

REASON:
SmartLogger allows only one appender per output destination to maintain a clear log routing strategy.

FIX:
Merge duplicate destination appenders into a single appender with appropriate formatting and filtering.

LEARN MORE:
See configuration-reference.md: 'Destination' section for destination types or configuration-guide.md: 'Console + File (Common Combo)'.
```

**JSON Config Issue**:
```json
{
  "appenders": [
    { "destination": { "type": "Console" }, ... },
    { "destination": { "type": "Console" }, ... }  ❌ Duplicate!
  ]
}
```

**Fix**: Keep only one Console appender.

---

### 2. Unknown Destination Type

**Error Message**:
```
Appender[0]: Destination type is not configured.

REASON:
Every appender must specify a valid output destination (Console, FileSystem, or LogAggregator).

FIX:
Set the 'Destination.Type' property to a valid destination type.

LEARN MORE:
See configuration-reference.md: 'Destination' section or configuration-guide.md for examples.
```

**JSON Config Issue**:
```json
{
  "appenders": [
    {
      "destination": { "type": null }  ❌ Missing type!
    }
  ]
}
```

**Fix**: 
```json
{
  "destination": { "type": "Console" }
}
```

---

### 3. FileSystem Appender Missing File Configuration

**Error Message**:
```
Appender[0]: FileSystem destination requires 'File' configuration.

REASON:
When the destination is FileSystem, file-specific settings must be provided.

FIX:
Add a 'Destination.File' object with directory, fileName, and extension.

LEARN MORE:
See configuration-guide.md: 'Production - File Logging, JSON, Rolling' for a complete example.
```

**JSON Config Issue**:
```json
{
  "destination": {
    "type": "FileSystem",
    "file": null  ❌ Missing!
  }
}
```

**Fix**:
```json
{
  "destination": {
    "type": "FileSystem",
    "file": {
      "directory": "Logs",
      "fileName": "Application",
      "extension": "log"
    }
  }
}
```

---

### 4. Empty File Directory

**Error Message**:
```
Appender[0]: File directory is empty.

REASON:
The file logging system needs a valid directory path to create and store log files.

FIX:
Set 'File.Directory' to a valid path (e.g., "Logs" or "C:\Logs").

LEARN MORE:
See configuration-reference.md: 'File' section for property details.
```

**JSON Config Issue**:
```json
{
  "file": {
    "directory": "",  ❌ Empty!
    "fileName": "Application",
    "extension": "log"
  }
}
```

**Fix**:
```json
{
  "file": {
    "directory": "Logs",
    "fileName": "Application",
    "extension": "log"
  }
}
```

---

### 5. Empty File Extension

**Error Message**:
```
Appender[0]: File extension is empty.

REASON:
A file extension is required to produce valid log files.

FIX:
Set 'File.Extension' to a valid extension (e.g., "log", "txt", "json").

LEARN MORE:
See configuration-reference.md: 'File' section.
```

**JSON Config Issue**:
```json
{
  "extension": ""  ❌ Missing!
}
```

**Fix**:
```json
{
  "extension": "log"
}
```

---

### 6. Date Naming Without Date Format

**Error Message**:
```
Appender[0]: Date-based naming strategy requires a 'DateFormat'.

REASON:
When using Date or Timestamp naming strategies, a date format string is required.

FIX:
Set 'File.Naming.DateFormat' to a valid .NET date format string (e.g., "yyyy-MM-dd").

LEARN MORE:
See configuration-reference.md: 'File Naming' section.
```

**JSON Config Issue**:
```json
{
  "naming": {
    "strategy": "Date",
    "dateFormat": ""  ❌ Empty!
  }
}
```

**Fix**:
```json
{
  "naming": {
    "strategy": "Date",
    "dateFormat": "yyyy-MM-dd"
  }
}
```

---

### 7. Size-Based Rolling Without Max Size

**Error Message**:
```
Appender[0]: Size-based rolling requires 'MaxFileSizeMB' > 0.

REASON:
When using Size-based rolling, a positive maximum file size must be specified.

FIX:
Set 'File.Rolling.MaxFileSizeMB' to a value > 0 (e.g., 10, 50, 100).

LEARN MORE:
See configuration-reference.md: 'Rolling' section.
```

**JSON Config Issue**:
```json
{
  "rolling": {
    "strategy": "Size",
    "maxFileSizeMB": 0  ❌ Must be > 0!
  }
}
```

**Fix**:
```json
{
  "rolling": {
    "strategy": "Size",
    "maxFileSizeMB": 50
  }
}
```

---

### 8. Archive Directory Empty When Enabled

**Error Message**:
```
Appender[0]: Archive directory is empty.

REASON:
When archival is enabled, a directory path must be provided to store archived log files.

FIX:
Set 'File.Archive.Directory' to a valid path (e.g., "Archive", "Logs/Archive").

LEARN MORE:
See configuration-reference.md: 'Archive' section.
```

**JSON Config Issue**:
```json
{
  "archive": {
    "enabled": true,
    "directory": ""  ❌ Empty!
  }
}
```

**Fix**:
```json
{
  "archive": {
    "enabled": true,
    "directory": "Archive"
  }
}
```

---

### 9. Retention Days Zero or Negative

**Error Message**:
```
Appender[0]: Retention days must be > 0.

REASON:
Retention period must be a positive value to maintain log file cleanup schedules.

FIX:
Set 'File.Retention.RetentionDays' to a positive integer (e.g., 30, 90, 365).

LEARN MORE:
See configuration-reference.md: 'Retention' section.
```

**JSON Config Issue**:
```json
{
  "retention": {
    "retentionDays": 0  ❌ Must be > 0!
  }
}
```

**Fix**:
```json
{
  "retention": {
    "retentionDays": 30
  }
}
```

---

### 10. LogAggregator Non-JSON Format

**Error Message**:
```
Appender[1]: LogAggregator requires JSON output format.

REASON:
Log aggregators consume structured JSON payloads. Other formats cannot be parsed.

FIX:
Set 'Formatter.OutputFormat' to 'Json'.

LEARN MORE:
See configuration-reference.md: 'Log Aggregator' section.
```

**JSON Config Issue**:
```json
{
  "destination": {
    "type": "LogAggregator",
    "logAggregator": { ... }
  },
  "formatter": {
    "outputFormat": "PlainText"  ❌ Must be Json!
  }
}
```

**Fix**:
```json
{
  "formatter": {
    "outputFormat": "Json"
  }
}
```

---

### 11. LogAggregator Default Sink Without Endpoint

**Error Message**:
```
Appender[1]: LogAggregator using default sink requires 'Endpoint'.

REASON:
The default HTTP aggregator sink needs a valid endpoint URL to send logs to.

FIX:
Set 'LogAggregator.Endpoint' to a valid HTTP(S) URL (e.g., "http://localhost:8080/logs").

LEARN MORE:
See configuration-reference.md: 'Log Aggregator' section.
```

**JSON Config Issue**:
```json
{
  "logAggregator": {
    "useDefault": true,
    "endpoint": null  ❌ Missing!
  }
}
```

**Fix**:
```json
{
  "logAggregator": {
    "useDefault": true,
    "endpoint": "http://localhost:8080/logs"
  }
}
```

---

### 12. LogAggregator Custom Sink Without Key or Type

**Error Message**:
```
Appender[1]: Custom LogAggregator sink requires 'SinkKey' or 'SinkTypeName'.

REASON:
When UseDefault is false, you must provide either a registered sink key or a type name.

FIX:
Set either 'LogAggregator.SinkKey' (for registered sinks) or 'LogAggregator.SinkTypeName' (for reflection-based instantiation).

LEARN MORE:
See configuration-reference.md: 'Log Aggregator' section for custom sink details.
```

**JSON Config Issue**:
```json
{
  "logAggregator": {
    "useDefault": false,
    "sinkKey": null,
    "sinkTypeName": null  ❌ Must provide one!
  }
}
```

**Fix (Option 1 - Registered Sink)**:
```json
{
  "logAggregator": {
    "useDefault": false,
    "sinkKey": "MyCustomSink"
  }
}
```

**Fix (Option 2 - Type Name)**:
```json
{
  "logAggregator": {
    "useDefault": false,
    "sinkTypeName": "MyApp.CustomSink, MyApp"
  }
}
```

---

### 13. Custom Layout Without Pattern

**Error Message**:
```
Appender[0]: Custom layout requires a non-empty 'Pattern'.

REASON:
When using the Custom layout type, a valid pattern string must be provided.

FIX:
Set the 'Formatter.Pattern' property to a valid pattern (e.g., "[%LEVEL] %MESSAGE").

LEARN MORE:
See configuration-reference.md: 'Formatter' section for available tokens.
```

**JSON Config Issue**:
```json
{
  "formatter": {
    "layoutType": "Custom",
    "pattern": ""  ❌ Missing!
  }
}
```

**Fix**:
```json
{
  "formatter": {
    "layoutType": "Custom",
    "pattern": "[%DATE] [%LEVEL] %MESSAGE"
  }
}
```

---

### 14. JSON Format Without Included Fields

**Error Message**:
```
Appender[0]: JSON output format requires at least one included field.

REASON:
JSON formatted logs need at least one field to produce valid JSON output.

FIX:
Add default fields or specify custom fields in 'Formatter.IncludedJsonFields'.

LEARN MORE:
See configuration-reference.md: 'Formatter' section for available field names.
```

**JSON Config Issue**:
```json
{
  "formatter": {
    "outputFormat": "Json",
    "includedJsonFields": []  ❌ Empty!
  }
}
```

**Fix**:
```json
{
  "formatter": {
    "outputFormat": "Json",
    "includedJsonFields": ["timestamp", "level", "message"]
  }
}
```

---

### 15. Empty Logger Override Name

**Error Message**:
```
LoggerOverride[0]: Logger name is empty.

REASON:
Each logger override must specify a logger name (typically the fully qualified class or namespace name).

FIX:
Set 'LoggerName' to the name of the logger to override (e.g., "MyApp.Services.PaymentService").

LEARN MORE:
See configuration-reference.md: 'Logger Override' section.
```

**JSON Config Issue**:
```json
{
  "loggerOverrides": [
    {
      "loggerName": "",  ❌ Empty!
      "logLevel": "DEBUG"
    }
  ]
}
```

**Fix**:
```json
{
  "loggerOverrides": [
    {
      "loggerName": "MyApp.Services",
      "logLevel": "DEBUG"
    }
  ]
}
```

---

## Quick Tips

✅ **Always provide**:
- `Destination.Type` for every appender
- `File` configuration when using FileSystem destination
- `LogAggregator` configuration when using LogAggregator destination
- `DateFormat` for date/timestamp naming strategies
- At least one field in `IncludedJsonFields` for JSON output

✅ **FileSystem appenders must have**:
- Non-empty `Directory`
- Non-empty `FileName`
- Non-empty `Extension`
- Valid `Rolling` configuration
- `Retention.RetentionDays` > 0 (if retention enabled)

✅ **LogAggregator appenders must**:
- Use `Json` output format
- Provide endpoint (if using default sink)
- Provide SinkKey or SinkTypeName (if using custom sink)

✅ **Custom layout requires**:
- Non-empty `Pattern` string when `LayoutType = Custom`

---

**For complete property documentation**: See [configuration-reference.md](configuration-reference.md)  
**For working examples**: See [configuration-guide.md](configuration-guide.md)  
**For validator implementation details**: See [Configuration_Validator_Guide.md](Configuration_Validator_Guide.md)
