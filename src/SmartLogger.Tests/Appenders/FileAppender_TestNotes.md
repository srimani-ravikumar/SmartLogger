# FileAppender Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                                    |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `FileAppender` class, validating message filtering, formatting, file delegation, log-level management, formatter management, initialization, configuration updates, and error handling. |

# Objective

Validate that **FileAppender** correctly acts as the file-based implementation of `ILogAppender` by:

* Filtering log messages according to the configured minimum log level.
* Ignoring null log messages.
* Formatting enabled messages before writing.
* Delegating formatted output to the file lifecycle infrastructure.
* Managing its log-level configuration.
* Managing its formatter configuration.
* Supporting runtime configuration updates.
* Correctly initializing its file lifecycle infrastructure.
* Propagating relevant dependency and filesystem failures.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test should use a unique temporary directory because `FileAppender` creates a real `FileLifecycleManager`.
  * Temporary files and directories should be cleaned up after each test.
  * `ILogOutputFormatterStrategy`, `IRollingStrategy`, and `IFileNamingStrategy` should be mocked where their behavior is not the subject of the test.
* Filesystem Dependency:

  * `FileLifecycleManager` is instantiated internally by `FileAppender` and therefore cannot be mocked directly using standard Moq.
  * Tests that verify actual writing should use an isolated temporary filesystem.

# Initialization Tests

## Appender should initialize successfully with a valid configuration

Validated by:

* `Constructor_WithValidConfiguration_ShouldInitializeAppender`

Verifies that a valid configuration, formatter, naming strategy, and optional rolling strategy allow the appender to be constructed successfully.

## Appender should initialize without a rolling strategy

Validated by:

* `Constructor_WithNullRollingStrategy_ShouldInitializeAppender`

Verifies that rolling is optional and a null `IRollingStrategy` does not prevent initialization.

## Appender should reject a null configuration

Validated by:

* `Constructor_WithNullConfiguration_ShouldThrowArgumentNullException`

The exception is propagated from `FileLifecycleManager`.

## Appender should reject a null naming strategy

Validated by:

* `Constructor_WithNullNamingStrategy_ShouldThrowArgumentNullException`

The exception is propagated from `FileLifecycleManager`.

## Appender should accept a valid formatter during initialization

Validated by:

* `Constructor_WithValidFormatter_ShouldSetFormatter`

Verifies that the supplied formatter becomes the active formatter.

> **Note:** The constructor currently does not explicitly validate the formatter. A null formatter may therefore be accepted during construction and fail later when an enabled message is appended.

# Message Append Tests

## Appender should ignore a null log message

Validated by:

* `Append_WithNullMessage_ShouldNotWrite`

Verifies that null messages are safely ignored without formatting or filesystem activity.

## Appender should write an enabled log message

Validated by:

* `Append_WithEnabledMessage_ShouldWriteFormattedMessage`

Verifies the complete path:

```text
LogMessage
    ↓
Log-level check
    ↓
Formatter.Format()
    ↓
FileLifecycleManager.Write()
    ↓
Active log file
```

## Appender should format the message before writing

Validated by:

* `Append_WithEnabledMessage_ShouldFormatMessageBeforeWrite`

Verifies that the formatter receives the original `LogMessage` and its returned string is the content written to the file.

## Appender should append a newline after the formatted message

Validated by:

* `Append_WithEnabledMessage_ShouldWriteFormattedMessageWithNewLine`

The newline is ultimately added by `FileLifecycleManager.WriteToActiveFile()`.

## Appender should append multiple enabled messages

Validated by:

* `Append_WithMultipleEnabledMessages_ShouldAppendAllMessages`

Verifies that successive calls preserve previously written content rather than replacing the active log file.

# Log-Level Filtering Tests

## Appender should enable messages at the configured threshold

Validated by:

* `IsEnabled_WithEqualLogLevel_ShouldReturnTrue`

Verifies inclusive threshold behavior.

## Appender should enable messages above the configured threshold

Validated by:

* `IsEnabled_WithHigherLogLevel_ShouldReturnTrue`

## Appender should disable messages below the configured threshold

Validated by:

* `IsEnabled_WithLowerLogLevel_ShouldReturnFalse`

## Appender should respect NONE as the configured threshold

Validated by:

* `IsEnabled_WithNoneThreshold_ShouldEnableAllLevels`

Since `NONE = 0` and all other levels have a higher numeric priority, the current comparison logic considers all levels enabled when the threshold is `NONE`.

## Appender should treat NONE messages according to the configured threshold

Validated by:

* `IsEnabled_WithNoneMessageLevel_ShouldFollowThreshold`

Verifies the actual enum comparison behavior when a `NONE`-level message is supplied.

## Appender should not format disabled messages

Validated by:

* `Append_WithDisabledMessage_ShouldNotInvokeFormatter`

This is important because filtering occurs before formatting.

## Appender should not write disabled messages

Validated by:

* `Append_WithDisabledMessage_ShouldNotWrite`

Verifies that disabled messages produce no filesystem output.

## Appender should evaluate the message's log level against the current threshold

Validated by:

* `Append_ShouldUseMessageLogLevelForFiltering`

Ensures the `LogMessage.LogLevel` determines whether the message is processed rather than the appender's configured level being used incorrectly.

# Log-Level Management Tests

## Appender should update its log level

Validated by:

* `SetLogLevel_ShouldUpdateCurrentLogLevel`

## Appender should return the updated log level

Validated by:

* `SetLogLevel_ThenGetLogLevel_ShouldReturnUpdatedLevel`

## Appender should apply a changed log level to subsequent messages

Validated by:

* `SetLogLevel_ShouldAffectSubsequentAppendOperations`

Verifies that changing the threshold dynamically changes which messages are accepted.

## Appender should return the current log level regardless of the supplied GetLogLevel argument

Validated by:

* `GetLogLevel_WithDifferentArgument_ShouldReturnConfiguredLogLevel`

> **Note:** `GetLogLevel(LogLevel logLevel)` does not use its parameter. The test should document the current interface behavior rather than implying that the argument has semantic meaning.

# Formatter Management Tests

## Appender should return the formatter supplied during construction

Validated by:

* `GetFormatter_AfterConstruction_ShouldReturnConfiguredFormatter`

## Appender should replace the current formatter

Validated by:

* `SetFormatter_WithValidFormatter_ShouldReplaceCurrentFormatter`

## Appender should use the newly configured formatter

Validated by:

* `SetFormatter_ShouldUseNewFormatterForSubsequentMessages`

Verifies that subsequent messages are formatted using the replacement formatter.

## Appender should reject a null formatter

Validated by:

* `SetFormatter_WithNullFormatter_ShouldThrowArgumentNullException`

The setter explicitly validates the formatter.

## Appender should preserve the existing formatter when SetFormatter fails

Validated by:

* `SetFormatter_WithNullFormatter_ShouldPreserveExistingFormatter`

Verifies that an invalid replacement does not overwrite the existing formatter.

# Configuration Update Tests

## Appender should update log level and formatter together

Validated by:

* `UpdateConfiguration_WithValidValues_ShouldUpdateConfiguration`

Verifies that both internal configuration values are replaced.

## Appender should use updated configuration for subsequent messages

Validated by:

* `UpdateConfiguration_ShouldAffectSubsequentAppendOperations`

Verifies that the new log level controls filtering and the new formatter controls output.

## Configuration update should not recreate the file lifecycle infrastructure

Validated by:

* `UpdateConfiguration_ShouldPreserveExistingFileLifecycle`

The method changes only `_logLevel` and `_formatter`; it does not recreate the underlying `FileLifecycleManager`.

> **Note:** This is an important current implementation characteristic. Changes to file configuration itself are not supported by `UpdateConfiguration()`.

## Configuration update should accept a null formatter according to current implementation

Validated by:

* `UpdateConfiguration_WithNullFormatter_ShouldStoreNullFormatter`

> **Note:** Unlike `SetFormatter()`, `UpdateConfiguration()` currently performs no null validation. This test should document the current behavior. A future implementation could validate the formatter consistently with `SetFormatter()`.

# File Writing Integration-Boundary Tests

## Appender should create the active log file during initialization

Validated by:

* `Constructor_WithValidConfiguration_ShouldCreateActiveLogFile`

This behavior is performed by `FileLifecycleManager` during construction.

## Appender should write formatted content to the configured directory

Validated by:

* `Append_WithValidMessage_ShouldWriteToConfiguredDirectory`

## Appender should create the configured directory when it does not exist

Validated by:

* `Constructor_WithMissingDirectory_ShouldCreateDirectory`

## Appender should preserve existing log content when appending

Validated by:

* `Append_WithExistingLogContent_ShouldAppendInsteadOfOverwrite`

These tests verify the observable filesystem behavior resulting from the `FileAppender` → `FileLifecycleManager` boundary.

# Error Handling Tests

## Appender should propagate formatter exceptions

Validated by:

* `Append_WhenFormatterThrows_ShouldPropagateException`

The appender does not catch formatter exceptions.

## Appender should propagate filesystem exceptions

Validated by:

* `Append_WhenFileWriteFails_ShouldPropagateException`

The appender does not catch exceptions raised by the file lifecycle infrastructure.

## Appender should not perform formatting when the message is null

Validated by:

* `Append_WithNullMessage_ShouldNotInvokeFormatter`

## Appender should not perform formatting when the message is disabled

Validated by:

* `Append_WithDisabledMessage_ShouldNotInvokeFormatter`

These verify that invalid/filtered messages fail fast before entering the formatting and filesystem path.

# Rolling Boundary Tests

## Appender should allow the rolling strategy to determine whether a file should roll

Validated by:

* `Append_WhenRollingStrategyRequestsRoll_ShouldWriteToNewActiveFile`

The rolling decision belongs to `IRollingStrategy`; the actual archive/rotation operation belongs to `FileLifecycleManager`.

## Appender should continue writing successfully when rolling is not required

Validated by:

* `Append_WhenRollingStrategyDoesNotRequestRoll_ShouldWriteToExistingActiveFile`

## Appender should support multiple writes across a rolling boundary

Validated by:

* `Append_WhenRollingOccursBetweenWrites_ShouldPreservePreviousLogAsArchive`

> **Scope:** These are **integration-boundary tests**, not tests of the rolling algorithm itself. Detailed rolling behavior belongs to `FileLifecycleManager` and the individual `IRollingStrategy` implementations.

# Thread-Safety Boundary Tests

## Concurrent append operations should not corrupt log output

Validated by:

* `Append_Concurrently_ShouldPreserveAllMessages`

The actual synchronization is implemented by `FileLifecycleManager` using `_syncRoot`.

The test should verify observable behavior such as:

* All expected messages are present.
* No messages are partially lost.
* Individual formatted entries remain intact.

> **Scope:** Locking implementation and concurrency correctness belong primarily to `FileLifecycleManager`. `FileAppender` itself contains no synchronization mechanism.

# Test Scope

These tests validate the public behavior of **FileAppender** and its observable interaction with the file lifecycle boundary.

The following responsibilities are intentionally tested separately within **FileLifecycleManager**:

* Active file creation implementation
* Directory creation
* Append-mode file I/O
* File existence recovery
* File locking implementation
* Rolling execution
* Archive file movement
* Unique archive naming
* Archive compression
* Retention cleanup
* Filesystem lifecycle management
* Concurrent file-write synchronization

The following responsibilities are intentionally tested separately within **IRollingStrategy** implementations:

* Daily rolling decision
* File-size rolling decision
* Rolling threshold calculations
* Boundary conditions around rolling thresholds

The following responsibilities are intentionally tested separately within **IFileNamingStrategy** implementations:

* Active file-name generation
* Timestamp formatting
* Date formatting
* Rolled file-name generation
* Archive index handling

The following responsibilities are intentionally tested separately within formatter implementations:

* `LogMessage` field formatting
* Pattern parsing
* Timestamp formatting
* Source formatting
* Correlation ID formatting
* Invalid formatting patterns

# Coverage Summary

| Area                          | Covered |
| ----------------------------- | :-----: |
| Initialization                |    ✅    |
| Null message handling         |    ✅    |
| Message filtering             |    ✅    |
| Log-level comparison          |    ✅    |
| Log-level management          |    ✅    |
| Formatter management          |    ✅    |
| Message formatting delegation |    ✅    |
| File writing boundary         |    ✅    |
| Multiple message append       |    ✅    |
| Configuration update          |    ✅    |
| Rolling boundary              |    ✅    |
| Error propagation             |    ✅    |
| Filesystem boundary           |    ✅    |
| Thread-safety boundary        |    ✅    |
| Rolling implementation        |    ❌*   |
| File naming implementation    |    ❌*   |
| Archive implementation        |    ❌*   |
| Retention implementation      |    ❌*   |
| Formatter implementation      |    ❌*   |

* Intentionally covered by dedicated component-level test plans.

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>