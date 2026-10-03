# SmartLogger

# Configuration Reference

Full property reference for every config object. Looking for a working example instead? See the [Configuration Guide](configuration-guide.md) recipes.

## Contents

- [Root](#root)
- [Logger Override](#logger-override)
- [Appender](#appender)
- [Destination](#destination)
- [Log Aggregator](#log-aggregator)
- [Formatter](#formatter)
- [File](#file)
- [File Naming](#file-naming)
- [Rolling](#rolling)
- [Archive](#archive)
- [Retention](#retention)

---

## Root

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| rootLogLevel | `LogLevel` | Default log level: `NONE`, `DEBUG`, `INFO`, `WARNING`, `ERROR`, `FATAL` | INFO |
| loggerOverrides | `List<LoggerOverrideConfiguration>` | Per-logger log level overrides, win over rootLogLevel - see [Logger Override](#logger-override) | Empty |
| appenders | `List<AppenderConfiguration>` | Configured appenders | Empty (Console added automatically) |
| enableAsyncLoggingProcess | `bool` | Processes logs on a background worker | false |

[↑ Top](#contents)

---

## Logger Override

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| loggerName | `string` | Fully qualified logger name to override (class or namespace) | Empty |
| logLevel | `LogLevel` | Minimum log level for this logger, overrides rootLogLevel | INFO |

```json
{ "loggerName": "SmartLogger.PaymentService", "logLevel": "DEBUG" }
```

Full recipe: [Per-Component Log Levels](configuration-guide.md#per-component-log-levels-overrides).

[↑ Top](#contents)

---

## Appender

| Property | Type | Description |
|------------|--------|-------------|
| destination | `DestinationConfiguration` | Where logs are written |
| formatter | `FormatterConfiguration` | Controls output formatting |
| filter | `object?` | Reserved for future versions |
| appenderLogLevel | `LogLevel?` | Overrides rootLogLevel for this appender only |

[↑ Top](#contents)

---

## Destination

| Property | Type | Description |
|------------|--------|-------------|
| type | `LogOutputDestination` | `Console`, `FileSystem` or `LogAggregator` |
| file | `FileConfiguration?` | Required when type is `FileSystem` |
| logAggregator | `LogAggregatorConfiguration?` | Required when type is `LogAggregator` |

> Gotcha: only one appender per destination type is allowed - SmartLogger rejects duplicates at load time.

[↑ Top](#contents)

---

## Log Aggregator

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| useDefault | `bool` | Use the built-in HTTP sink (`HttpLogAggregatorSink`) | true |
| endpoint | `Uri?` | HTTP endpoint the default sink posts logs to | Required when useDefault is true |
| sinkKey | `string?` | Key of a custom `ILogAggregatorSink` registered via `LoggerManager.Initialize(provider, customSinks)` | - |
| sinkTypeName | `string?` | Assembly-qualified type name of a custom `ILogAggregatorSink`, instantiated via reflection (needs a public parameterless constructor) | - |

Set `useDefault: false` to supply your own sink, resolved via `sinkKey` first, then `sinkTypeName`. Full examples: [Remote Log Aggregator](configuration-guide.md#remote-log-aggregator-centralized-sink).

[↑ Top](#contents)

---

## Formatter

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| outputFormat | `LogOutputFormat` | `PlainText`, `Json` or `Xml` | PlainText |
| layoutType | `LogMessageLayoutType` | `Simple`, `Detailed` or `Custom` | Simple |
| pattern | `string` | Custom layout pattern, required when layoutType is `Custom` | Empty |
| includedJsonFields | `List<string>` | Fields included in JSON output | timestamp, level, thread, correlation, source, message |
| jsonFieldMappings | `List<JsonFieldMappingConfiguration>` (`sourceField`, `targetField`) | Renames JSON fields | Empty |

> Gotcha: `LogAggregator` destinations require `outputFormat: Json`.

[↑ Top](#contents)

---

## File

Required when `destination.type` is `FileSystem`.

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| directory | `string` | Active log directory | Logs |
| fileName | `string` | Active log file name | Application |
| extension | `string` | File extension, no leading dot | log |
| naming | `FileNamingConfiguration` | File naming configuration | Date strategy |
| rolling | `FileRollingConfiguration` | Rolling configuration | Daily |
| archive | `ArchiveConfiguration` | Archive configuration | Enabled |
| retention | `RetentionConfiguration` | Retention configuration | 30 days |

> Gotcha: `"extension": ".log"` is wrong - use `"log"` (no dot).

[↑ Top](#contents)

---

## File Naming

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| strategy | `FileNamingStrategyType` | `Date`, `Timestamp` or `Custom` | Date |
| dateFormat | `string` | .NET date format used for rolled files | yyyy-MM-dd |

Naming only decides *what a rolled file is called* - timing, archiving and compression are handled separately.

[↑ Top](#contents)

---

## Rolling

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| strategy | `RollingStrategyType` | `Daily` or `Size` | Daily |
| maxFileSizeMB | `long` | Max size before rolling, `Size` strategy only | 10 |

A rolling strategy only answers *"should the active file be rolled?"* - never names, archives, or compresses files.

[↑ Top](#contents)

---

## Archive

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| enabled | `bool` | Enables archive support | true |
| directory | `string` | Archive directory, created automatically | Archive |
| compress | `bool` | Compress rolled logs into ZIP | true |

[↑ Top](#contents)

---

## Retention

| Property | Type | Description | Default |
|------------|--------|-------------|----------|
| retentionDays | `int` | Days archived logs are kept before deletion | 30 |

Retention cleanup runs immediately after a successful roll - no scheduler involved.

[↑ Top](#contents)
