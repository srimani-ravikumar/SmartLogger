# SmartLogger Configuration Design at a Glance

```mermaid
classDiagram

class LogConfigurationHolder {
    +LogLevel RootLogLevel
    +bool EnableDefaultConsoleAppender
    +List~AppenderConfiguration~ Appenders
    +bool EnableAsyncLoggingProcess
    +List~LoggerOverrideConfiguration~ LoggerOverrides
}

class LoggerOverrideConfiguration {
    +string LoggerName
    +LogLevel LogLevel
}

class AppenderConfiguration {
    +DestinationConfiguration Destination
    +FormatterConfiguration Formatter
    +object Filter
    +LogLevel? AppenderLogLevel
}

class DestinationConfiguration {
    +LogOutputDestination Type
    +FileConfiguration File
}

class FileConfiguration {
    +string Directory
    +string FileName
    +string Extension
    +FileNamingConfiguration Naming
    +FileRollingConfiguration Rolling
    +ArchiveConfiguration Archive
    +RetentionConfiguration Retention
}

class FileNamingConfiguration {
    +FileNamingStrategyType Strategy
    +string DateFormat
}

class FileRollingConfiguration {
    +RollingStrategyType Strategy
    +long MaxFileSizeMB
}

class ArchiveConfiguration {
    +bool Enabled
    +string Directory
    +bool Compress
}

class RetentionConfiguration {
    +int RetentionDays
}

class FormatterConfiguration {
    +LogOutputFormat OutputFormat
    +LogMessageLayoutType LayoutType
    +string Pattern
    +List~string~ IncludedJsonFields
    +List~JsonFieldMappingConfiguration~ JsonFieldMappings
}

class JsonFieldMappingConfiguration {
    +string SourceField
    +string TargetField
}

class LogOutputDestination {
    <<enumeration>>
    Unknown
    Console
    FileSystem
    DatabaseSystem
}

class LogLevel {
    <<enumeration>>
    NONE
    DEBUG
    INFO
    WARNING
    ERROR
    FATAL
}

class LogOutputFormat {
    <<enumeration>>
    PlainText
    Json
    Xml
}

class LogMessageLayoutType {
    <<enumeration>>
    Simple
    Detailed
    Custom
}

class FileNamingStrategyType {
    <<enumeration>>
    Date
    Timestamp
    Custom
}

class RollingStrategyType {
    <<enumeration>>
    Daily
    Size
}

LogConfigurationHolder "1" *-- "0..*" LoggerOverrideConfiguration : overrides
LogConfigurationHolder "1" *-- "0..*" AppenderConfiguration : appenders

AppenderConfiguration "1" *-- "1" DestinationConfiguration : destination
AppenderConfiguration "1" *-- "1" FormatterConfiguration : formatter

DestinationConfiguration "1" *-- "0..1" FileConfiguration : file

FileConfiguration "1" *-- "1" FileNamingConfiguration : naming
FileConfiguration "1" *-- "1" FileRollingConfiguration : rolling
FileConfiguration "1" *-- "1" ArchiveConfiguration : archive
FileConfiguration "1" *-- "1" RetentionConfiguration : retention

FormatterConfiguration "1" *-- "0..*" JsonFieldMappingConfiguration : mappings

LogConfigurationHolder --> LogLevel : root level
LoggerOverrideConfiguration --> LogLevel : override level

AppenderConfiguration --> LogLevel : appender level

DestinationConfiguration --> LogOutputDestination : destination type

FormatterConfiguration --> LogOutputFormat : output format
FormatterConfiguration --> LogMessageLayoutType : layout type

FileNamingConfiguration --> FileNamingStrategyType : naming strategy
FileRollingConfiguration --> RollingStrategyType : rolling strategy
```

### One-line descriptions

| Entity                          | Description                   |
| ------------------------------- | ----------------------------- |
| **LogConfigurationHolder**      | **Root configuration**        |
| `LoggerOverrideConfiguration`   | **Logger-level override**     |
| `AppenderConfiguration`         | **Output pipeline config**    |
| `DestinationConfiguration`      | **Where logs go**             |
| `FileConfiguration`             | **File logging config**       |
| `FileNamingConfiguration`       | **File naming strategy**      |
| `FileRollingConfiguration`      | **File rotation policy**      |
| `ArchiveConfiguration`          | **Rolled-file archival**      |
| `RetentionConfiguration`        | **Archive cleanup policy**    |
| `FormatterConfiguration`        | **Log formatting rules**      |
| `JsonFieldMappingConfiguration` | **JSON field mapping**        |
| `LogLevel`                      | **Logging severity**          |
| `LogOutputDestination`          | **Log destination type**      |
| `LogOutputFormat`               | **Log serialization format**  |
| `LogMessageLayoutType`          | **Message layout style**      |
| `FileNamingStrategyType`        | **File naming strategy type** |
| `RollingStrategyType`           | **File rotation strategy**    |

### Design intent

The hierarchy can be remembered as:

**`LogConfigurationHolder` => What + Where + How**

* **What**: `RootLogLevel`, `LoggerOverrides`

* **Where**: `Appender → Destination → File`

* **How**: `Formatter`

* **File lifecycle**: `Naming → Rolling → Archive → Retention`

This is written in a way to **show responsibilities and composition, not XML-style property documentation.**

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
