# SmartLogger v1

# File Logging Architecture Design Decisions

## Document Information

| Project | Version | Date | Author | Status | Description |
|---------|---------|------------|---------|-------------|-------------|
| SmartLogger | 1.0.0 | 2026-07-12 | Srimani | Final | Defines the architecture, design decisions, responsibilities, and lifecycle of the SmartLogger file logging subsystem, including file naming, rolling, archival, compression, and retention strategies. |

# Design Philosophy

> **Keep the default experience simple while making the architecture open for extension.**

SmartLogger is designed to solve **95% of real-world logging requirements** with minimal configuration while providing well-defined extension points for future enhancements without requiring modifications to the existing framework.

The framework follows **Convention over Configuration**, **Single Responsibility Principle**, **Open/Closed Principle**, and **Fail Fast** design principles.

# Guiding Principles

## 1. Simplicity First

The default configuration should be sufficient for most applications.

A client should not understand the internal architecture in order to configure logging.

## 2. Predictable Behavior

The framework should always behave consistently.

Given the same configuration:

- The same file names are generated.
- The same rolling behavior occurs.
- The same archive structure is maintained.

No timers.

No hidden background workers.

No unexpected background activities.

## 3. Separation of Responsibilities

Every component has exactly one responsibility.

```
FileNamingStrategy
        ↓
Determines WHAT the log file should be called.

--------------------------------

FileRollingStrategy
        ↓
Determines WHEN the active log file should roll.

--------------------------------

FileLifecycleManager
        ↓
Owns the complete lifecycle of the active log file.

--------------------------------

CompressionHelper
        ↓
Compresses archived log files.

--------------------------------

RetentionHelper
        ↓
Deletes expired archived log files.

--------------------------------

FileAppender
        ↓
Formats log messages and delegates file operations to FileLifecycleManager.
```

No component should know another component's internal implementation.

# Default Behavior

If the client only specifies

```json
{
    "Directory": "Logs"
}
```

SmartLogger automatically uses

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

This keeps configuration simple while following sensible defaults.

# File Naming Strategy

## Responsibility

Determines **what the log file should be called**.

It **never**

- decides when to roll
- checks file existence
- resolves file name collisions
- performs archive operations

Example

```
Active

Application.log
```

Rolled

```
Application_20260712.log

Application_20260712_1.log

Application_20260712_2.log
```

File name collision detection is handled by **FileLifecycleManager**, not by the naming strategy.

# File Rolling Strategy

## Responsibility

Determines whether the current active log file should continue to be used.

Returns

```
true

or

false
```

It never

- generates file names
- moves files
- archives logs
- compresses logs

Supported strategies

```
Daily

Size
```

Default

```
Daily
```

# Hybrid Rolling Approach

SmartLogger follows a **Lazy Rolling** approach.

Rolling is evaluated only when a new log entry arrives.

No timers.

No scheduler.

No background worker.

A single synchronization lock protects the complete **roll-and-write** operation.

```
Acquire Lock

↓

Ensure Active File

↓

Should Roll ?

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

↓

Release Lock
```

Benefits

- Thread-safe
- Ordered writes
- Lightweight implementation
- Predictable behavior

# File Lifecycle Manager

## Responsibility

The **FileLifecycleManager** owns the complete lifecycle of the active log file.

Responsibilities include

- Creating the active log file
- Evaluating rolling conditions
- Archiving rolled log files
- Compressing archived logs
- Cleaning up expired archives
- Writing log messages to the active file

The FileAppender delegates all file-related responsibilities to this component.

# Archive Strategy

When a rolling event occurs

```
Logs

Application.log
```

becomes

```
Logs

Application.log

Archive

Application_20260712.log
```

The active directory always contains only the current log file.

# Compression Strategy

Compression occurs immediately after archival.

```
Application_20260712.log

↓

Application_20260712.zip
```

The original log file is deleted after successful compression.

Benefits

- Reduced disk usage
- Cleaner archive directory
- Faster backups

# Retention Strategy

Retention cleanup executes immediately after a successful rolling operation.

No scheduler.

No timers.

Lifecycle

```
Archive

↓

Compress

↓

Delete archives older than 30 days
```

Only archived ZIP files participate in retention cleanup.

# Configuration Philosophy

Configuration should express **behavior**, not implementation.

Example

```json
{
    "RollingStrategy": "Daily",
    "NamingStrategy": "Date",
    "RetentionDays": 30
}
```

Clients configure **what they want**, never **how SmartLogger implements it**.

# Public API Philosophy

Built-in behavior is configured using enums.

```
RollingStrategyType

FileNamingStrategyType
```

Advanced consumers can extend the framework by implementing

```
IFileNamingStrategy

IRollingStrategy
```

without modifying SmartLogger itself.

# Extension Points

Future enhancements may include

```
WeeklyRollingStrategy

HourlyRollingStrategy

MonthlyRollingStrategy

TimestampNamingStrategy

MachineNameNamingStrategy

ProcessIdNamingStrategy
```

The existing framework remains unchanged.

# Failure Philosophy (Fail Fast)

Invalid configurations or custom implementations should fail immediately.

Examples

❌ Invalid file name

❌ Invalid archive path

❌ Invalid extension

❌ Invalid rolling configuration

❌ Invalid custom strategy implementation

Failing early prevents silent log corruption and simplifies troubleshooting.

# Ownership Matrix

| Component | Responsibility |
|------------|----------------|
| **FileAppender** | Validates log level, formats messages, delegates file operations |
| **FileLifecycleManager** | Owns the complete lifecycle of the active log file |
| **FileNamingStrategy** | Generates deterministic log file names |
| **FileRollingStrategy** | Determines when the active file should roll |
| **CompressionHelper** | Compresses archived log files |
| **RetentionHelper** | Removes expired archived log files |

# Complete Lifecycle

```
Log()

↓

Validate Log Level

↓

Format Message

↓

FileLifecycleManager.Write()

↓

Acquire Lock

↓

Ensure Active File

↓

Should Roll ?

↓

No -----------------------------┐
↓                               │
Write Message                   │
↓                               │
Release Lock                    │
                                │
Yes                             │
↓                               │
Archive Active File             │
↓                               │
Compress Archive                │
↓                               │
Cleanup Expired Archives        │
↓                               │
Create Fresh Active File        │
↓                               │
Write Message                   │
↓                               │
Release Lock
```

# Key Architectural Decisions

| Decision | Reason |
|-----------|--------|
| Lazy Rolling | Eliminates timers and background workers |
| Single Synchronization Lock | Ensures thread-safe ordered writes |
| Convention over Configuration | Keeps client configuration simple |
| FileLifecycleManager | Centralizes the entire file lifecycle |
| Strategy Pattern | Supports extensibility without modifying existing code |
| Helper Classes | Avoids premature abstraction while keeping responsibilities isolated |
| Fail Fast | Detects configuration issues as early as possible |

# Design Summary

Overall, this design follows solid engineering principles.

- **Single Responsibility Principle (SRP)** – Every component has one clear responsibility.
- **Open/Closed Principle (OCP)** – New naming and rolling strategies can be introduced without modifying existing implementations.
- **Convention over Configuration** – Sensible defaults minimize client configuration.
- **Fail Fast** – Invalid configurations are detected immediately.
- **Keep It Simple** – No schedulers, timers, or unnecessary background services.
- **Extensibility** – The architecture can evolve without requiring major redesigns.

<p align="center">
<strong>© 2026 Srimani. All rights reserved.</strong>
</p>