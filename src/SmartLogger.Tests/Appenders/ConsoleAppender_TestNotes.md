# ConsoleAppender Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                                  |
------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
1.0.0   | 2026-09-08 | Srimani | Initial Draft | Defined the unit test coverage for the `ConsoleAppender` class, validating initialization, log-level filtering, console routing, formatting, configuration, and thread safety. |

# Objective

Validate that **ConsoleAppender** correctly acts as a console-based log destination within the SmartLogger framework by:

* Initializing with the expected default configuration.
* Accepting explicit log-level and formatter configuration.
* Filtering messages according to the configured severity threshold.
* Formatting eligible messages before writing.
* Routing `ERROR` and `FATAL` messages to standard error.
* Routing all other enabled levels to standard output.
* Allowing runtime log-level changes.
* Allowing runtime formatter replacement.
* Guarding against invalid formatter input.
* Maintaining thread-safe console writes.
* Propagating formatter failures without silently losing errors.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:
  * `Console.SetOut()` and `Console.SetError()` are used to capture console output.
  * Original `Console.Out` and `Console.Error` streams must be restored after each test.
  * Console-output tests must avoid interference from parallel test execution because console streams are process-wide.
* Test Dependency:
  * `ILogOutputFormatterStrategy` is mocked using **Moq**.
  * `LogMessage` instances are created through `LogMessage.Builder`.

# Initialization Tests

## Framework should initialize a console appender with INFO as the default log level

Validated by:

* `Constructor_WithoutArguments_ShouldSetDefaultLogLevelToInfo`

Verifies that the parameterless constructor initializes the appender with `LogLevel.INFO`.

## Framework should initialize a default formatter

Validated by:

* `Constructor_WithoutArguments_ShouldInitializeDefaultFormatter`

Verifies that the parameterless constructor creates and assigns a formatter through `FormatterFactory` using the expected console/plain-text/simple configuration.

## Framework should initialize with the supplied log level

Validated by:

* `Constructor_WithLogLevel_ShouldSetConfiguredLogLevel`

Verifies that an explicitly supplied log level becomes the active threshold.

## Framework should initialize with the supplied formatter

Validated by:

* `Constructor_WithFormatter_ShouldSetConfiguredFormatter`

Verifies that the formatter supplied to the constructor is retained and returned by `GetFormatter()`.

# Log Level Tests

## Appender should enable messages at or above the configured threshold

Validated by:

* `IsEnabled_WithLevelEqualToThreshold_ShouldReturnTrue`
* `IsEnabled_WithLevelAboveThreshold_ShouldReturnTrue`

For example, with `WARNING` configured as the threshold, `WARNING`, `ERROR`, and `FATAL` must be enabled.

## Appender should disable messages below the configured threshold

Validated by:

* `IsEnabled_WithLevelBelowThreshold_ShouldReturnFalse`

For example, with `WARNING` configured as the threshold, `DEBUG` and `INFO` must be disabled.

## Appender should correctly evaluate every defined log level

Validated by:

* `IsEnabled_WithInfoThreshold_ShouldFollowLogLevelOrdering`
* `IsEnabled_WithWarningThreshold_ShouldFollowLogLevelOrdering`
* `IsEnabled_WithErrorThreshold_ShouldFollowLogLevelOrdering`
* `IsEnabled_WithFatalThreshold_ShouldFollowLogLevelOrdering`

Verifies the complete `NONE < DEBUG < INFO < WARNING < ERROR < FATAL` ordering used by `IsGreaterOrEqual()`.

## Appender should update its threshold when SetLogLevel is called

Validated by:

* `SetLogLevel_WithNewLevel_ShouldUpdateThreshold`

Verifies that subsequent `IsEnabled()` calls use the newly configured threshold.

## Appender should apply the new threshold to subsequent append operations

Validated by:

* `SetLogLevel_ThenAppend_ShouldUseUpdatedThreshold`

Verifies that changing the threshold immediately affects whether messages are written.

## Appender should return the current configured threshold

Validated by:

* `GetLogLevel_WithAnyArgument_ShouldReturnConfiguredThreshold`

The supplied argument does not affect the result because the current implementation returns the appender's active threshold.

# LogLevel.NONE Tests

## Appender should follow the current comparison semantics when NONE is configured as the threshold

Validated by:

* `IsEnabled_WithNoneThreshold_ShouldEnableAllDefinedLevels`

The current implementation performs an integer comparison, therefore every defined level is greater than or equal to `NONE`.

> **Note:** This behavior conflicts with the `LogLevel.NONE` documentation, which states that `NONE` disables logging. The test reflects the current implementation. If `NONE` is intended to disable logging, the filtering logic should be changed and the test updated accordingly.

## Appender should handle NONE as a message level according to the current threshold comparison

Validated by:

* `IsEnabled_WithNoneMessageLevel_ShouldFollowNumericComparison`

Verifies that `LogLevel.NONE` is treated according to `IsGreaterOrEqual()` rather than being independently rejected.

# Append Input Validation Tests

## Appender should ignore a null log message

Validated by:

* `Append_WithNullMessage_ShouldNotWriteOutput`

Verifies that a null message causes an immediate return without invoking the formatter or writing to either console stream.

## Appender should not invoke the formatter for a disabled message

Validated by:

* `Append_WithLevelBelowThreshold_ShouldNotFormatMessage`

Verifies that filtering occurs before formatting.

## Appender should not write disabled messages

Validated by:

* `Append_WithLevelBelowThreshold_ShouldNotWriteOutput`

Verifies that messages below the configured threshold produce no console output.

# Formatting Tests

## Appender should format an enabled message before writing

Validated by:

* `Append_WithEnabledMessage_ShouldFormatMessage`

Verifies that the configured formatter is invoked for an enabled message.

## Appender should write the formatted result to the console

Validated by:

* `Append_WithEnabledMessage_ShouldWriteFormattedMessage`

Verifies that the exact string returned by the formatter is written using `WriteLine()`.

## Appender should invoke the formatter exactly once per appended enabled message

Validated by:

* `Append_WithEnabledMessage_ShouldFormatExactlyOnce`

Verifies that a single append operation performs exactly one formatting operation.

## Appender should not format a null message

Validated by:

* `Append_WithNullMessage_ShouldNotInvokeFormatter`

Verifies the fast-exit behavior before formatter access.

## Appender should propagate formatter exceptions

Validated by:

* `Append_WhenFormatterThrows_ShouldPropagateException`

Verifies that formatter failures are not swallowed by the appender.

## Appender should not write output when formatting fails

Validated by:

* `Append_WhenFormatterThrows_ShouldNotWriteOutput`

Verifies that console I/O is not attempted when formatting does not successfully produce a result.

## Appender should write an empty formatted string if the formatter returns one

Validated by:

* `Append_WhenFormatterReturnsEmptyString_ShouldWriteEmptyLine`

Verifies that the appender does not impose additional validation on formatter output.

## Appender should write a null formatted result according to current WriteLine behavior

Validated by:

* `Append_WhenFormatterReturnsNull_ShouldWriteEmptyLine`

Verifies current behavior when a formatter returns `null`; `Console.WriteLine(string)` handles the value rather than the appender rejecting it.

# Console Routing Tests

## Appender should route DEBUG messages to standard output

Validated by:

* `Append_WithDebugMessage_ShouldWriteToStandardOutput`

## Appender should route INFO messages to standard output

Validated by:

* `Append_WithInfoMessage_ShouldWriteToStandardOutput`

## Appender should route WARNING messages to standard output

Validated by:

* `Append_WithWarningMessage_ShouldWriteToStandardOutput`

## Appender should route ERROR messages to standard error

Validated by:

* `Append_WithErrorMessage_ShouldWriteToStandardError`

## Appender should route FATAL messages to standard error

Validated by:

* `Append_WithFatalMessage_ShouldWriteToStandardError`

## Appender should not write ERROR messages to standard output

Validated by:

* `Append_WithErrorMessage_ShouldNotWriteToStandardOutput`

## Appender should not write FATAL messages to standard output

Validated by:

* `Append_WithFatalMessage_ShouldNotWriteToStandardOutput`

## Appender should route NONE according to the current implementation

Validated by:

* `Append_WithNoneMessage_ShouldWriteToStandardOutput_WhenEnabled`

`NONE` is not part of the special `ERROR`/`FATAL` routing condition, therefore it is routed to `Console.Out` when enabled.

# Formatter Configuration Tests

## Appender should return the currently configured formatter

Validated by:

* `GetFormatter_ShouldReturnConfiguredFormatter`

## Appender should replace the formatter when SetFormatter is called

Validated by:

* `SetFormatter_WithValidFormatter_ShouldReplaceExistingFormatter`

## Appender should use the newly assigned formatter for subsequent messages

Validated by:

* `SetFormatter_ThenAppend_ShouldUseNewFormatter`

Verifies that changing the formatter affects future append operations.

## Appender should reject a null formatter

Validated by:

* `SetFormatter_WithNullFormatter_ShouldThrowArgumentNullException`

Verifies explicit argument validation in `SetFormatter()`.

## Appender should retain the existing formatter when SetFormatter receives null

Validated by:

* `SetFormatter_WithNullFormatter_ShouldNotReplaceExistingFormatter`

Verifies that the assignment does not occur when validation fails.

# Runtime Configuration Tests

## Appender should allow independent log-level and formatter configuration

Validated by:

* `SetLogLevel_AndSetFormatter_ShouldUpdateBothConfigurations`

Verifies that changing one configuration does not unintentionally alter the other.

## Appender should use the latest configuration for subsequent messages

Validated by:

* `SetLogLevel_AndSetFormatter_ThenAppend_ShouldUseLatestConfiguration`

Verifies that both runtime configuration changes are reflected in subsequent append operations.

# Boundary Tests

## Appender should correctly handle the lowest defined threshold

Validated by:

* `Append_WithNoneThreshold_ShouldFollowDefinedComparisonBehavior`

Verifies behavior at the lowest enum value.

## Appender should correctly handle the highest defined threshold

Validated by:

* `Append_WithFatalThreshold_ShouldOnlyWriteFatalMessages`

Verifies that `FATAL` as the threshold prevents all lower severity messages from being written.

## Appender should correctly handle messages at the exact threshold

Validated by:

* `Append_WithMessageEqualToThreshold_ShouldWriteMessage`

Verifies the inclusive `>=` threshold semantics.

## Appender should correctly handle messages immediately below the threshold

Validated by:

* `Append_WithMessageImmediatelyBelowThreshold_ShouldNotWriteMessage`

Verifies the filtering boundary.

## Appender should correctly handle messages immediately above the threshold

Validated by:

* `Append_WithMessageImmediatelyAboveThreshold_ShouldWriteMessage`

Verifies the opposite side of the filtering boundary.

## Appender should handle unusual but valid message content

Validated by:

* `Append_WithUnicodeMessage_ShouldWriteFormattedMessage`
* `Append_WithVeryLongMessage_ShouldWriteFormattedMessage`
* `Append_WithSpecialCharacters_ShouldWriteFormattedMessage`

These tests verify that the appender passes message content to the formatter without imposing additional content restrictions.

# Concurrency Tests

## Appender should safely handle concurrent append operations

Validated by:

* `Append_Concurrently_ShouldCompleteWithoutException`

Verifies that multiple threads can append messages to the same appender without causing synchronization-related failures.

## Appender should prevent interleaved console writes

Validated by:

* `Append_Concurrently_ShouldWriteCompleteFormattedMessages`

Verifies that each formatted message is written as a complete console line rather than having output fragments interleaved between threads.

## Appender should support concurrent reads and log-level updates

Validated by:

* `SetLogLevel_AndIsEnabled_Concurrently_ShouldCompleteWithoutException`

Verifies safe access to the frequently read log-level field while configuration is being updated.

## Appender should support concurrent formatter replacement and append operations

Validated by:

* `SetFormatter_AndAppend_Concurrently_ShouldCompleteWithoutException`

Verifies that formatter replacement does not cause invalid formatter references or runtime failures during concurrent append operations.

> **Note:** The implementation captures the formatter reference before formatting. This ensures that a formatter replacement occurring during formatting does not invalidate the formatter instance being used by the current append operation.

# Test Scope

These tests validate only the public behavior of **ConsoleAppender**.

The following responsibilities are intentionally tested separately within their respective components:

* `LogLevelExtensions`:
  * Numeric severity comparison.
  * `IsGreaterOrEqual()` behavior.
* `LogMessage`:
  * Builder validation.
  * Default property values.
  * Message construction.
* `FormatterFactory`:
  * Formatter selection.
  * Formatter creation.
  * Formatter configuration validation.
* Individual `ILogOutputFormatterStrategy` implementations:
  * Formatting rules.
  * Layout generation.
  * Output pattern behavior.
* `ILogAppender` consumers:
  * Appender selection.
  * Appender lifecycle.
  * Appender configuration propagation.

ConsoleAppender tests should mock `ILogOutputFormatterStrategy` rather than retesting individual formatter implementations.

# Coverage Summary

| Area                                  | Covered |
| ------------------------------------- | :-----: |
| Default initialization                |    ✅    |
| Explicit initialization               |    ✅    |
| Default log level                     |    ✅    |
| Log-level filtering                   |    ✅    |
| Log-level boundary behavior           |    ✅    |
| LogLevel.NONE behavior                |    ✅    |
| Runtime log-level configuration       |    ✅    |
| Log-level retrieval                   |    ✅    |
| Null message handling                 |    ✅    |
| Message formatting                    |    ✅    |
| Formatter replacement                 |    ✅    |
| Null formatter validation             |    ✅    |
| Formatter exception propagation       |    ✅    |
| Standard output routing               |    ✅    |
| Standard error routing                |    ✅    |
| ERROR/FATAL routing                   |    ✅    |
| Special message content               |    ✅    |
| Concurrent append operations          |    ✅    |
| Console write synchronization         |    ✅    |
| Concurrent configuration changes      |    ✅    |
| Dependency interaction                |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>