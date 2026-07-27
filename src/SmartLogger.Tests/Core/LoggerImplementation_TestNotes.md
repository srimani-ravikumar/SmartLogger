# LoggerImplementation Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                                                                                              |
------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
1.0.0   | 2026-07-11 | Srimani | Initial Draft | Defined the unit test coverage for the `LoggerImplementation` class, validating the logging pipeline, log level filtering, message construction, filter evaluation, appender dispatch, configuration updates, and management operations. |

# Objective

Validate that **LoggerImplementation** correctly serves as the core logging engine within the SmartLogger framework by:

* Constructing `LogMessage` instances.
* Applying minimum log level filtering.
* Evaluating configured filters.
* Dispatching log messages to eligible appenders.
* Supporting convenience logging APIs.
* Managing appenders and filters.
* Updating configuration dynamically.
* Guarding against invalid usage.
* Handling invalid input appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a new `LoggerImplementation` instance.
  * Mock appenders and filters are recreated for every test.
  * `LogContext.BeginCorrelationScope()` is disposed after each test when used.

# Constructor Initialization Tests

## Default constructor should initialize with default settings

Validated by:

* `Constructor_Default_ShouldInitializeDefaultLogger`

Verifies:

* Logger name defaults to `"DefaultLogger"`.
* Effective minimum log level defaults to `INFO`.
* Default console appender is attached.

---

## Constructor should initialize with a custom logger name

Validated by:

* `Constructor_WithLoggerName_ShouldInitializeLogger`

---

## Constructor should initialize with a custom minimum log level

Validated by:

* `Constructor_WithLogLevel_ShouldInitializeLogger`

---

## Constructor should optionally disable the default console appender

Validated by:

* `Constructor_WithDefaultAppenderDisabled_ShouldNotAttachConsoleAppender`

# Logging Pipeline Tests

## Logger should construct a valid log message

Validated by:

* `Log_WithValidInput_ShouldConstructLogMessage`

Verifies that the generated `LogMessage` contains:

* Log level
* Message
* Source
* Correlation ID
* Thread ID
* Timestamp

---

## Logger should dispatch log messages to enabled appenders

Validated by:

* `Log_WithEnabledAppender_ShouldAppendMessage`

---

## Logger should dispatch the same log message to multiple appenders

Validated by:

* `Log_WithMultipleEnabledAppenders_ShouldAppendToAll`

---

## Logger should preserve correlation ID from LogContext

Validated by:

* `Log_WithCorrelationScope_ShouldPopulateCorrelationId`

---

## Logger should populate the logger name as the message source

Validated by:

* `Log_ShouldPopulateSourceName`

---

## Logger should populate the current managed thread ID

Validated by:

* `Log_ShouldPopulateCurrentThreadId`

---

## Logger should populate a UTC timestamp

Validated by:

* `Log_ShouldPopulateUtcTimestamp`

# Log Level Filtering Tests

## Logger should ignore messages below the effective minimum level

Validated by:

* `Log_BelowEffectiveMinimumLevel_ShouldNotInvokeFiltersOrAppenders`

Verifies the fast-fail optimization.

---

## Logger should process messages at the effective minimum level

Validated by:

* `Log_AtEffectiveMinimumLevel_ShouldDispatchLog`

---

## Logger should process messages above the effective minimum level

Validated by:

* `Log_AboveEffectiveMinimumLevel_ShouldDispatchLog`

---

## Logger should support changing the effective log level

Validated by:

* `SetLogLevel_ShouldUpdateEffectiveMinimumLevel`

# Filter Evaluation Tests

## Logger should evaluate all configured filters

Validated by:

* `Log_WithFilters_ShouldEvaluateFilters`

---

## Logger should stop processing when any filter rejects the message

Validated by:

* `Log_WhenFilterRejects_ShouldNotInvokeAppenders`

---

## Logger should dispatch messages when all filters approve

Validated by:

* `Log_WhenAllFiltersApprove_ShouldInvokeAppenders`

---

## Logger should short-circuit after the first rejecting filter

Validated by:

* `Log_WhenFirstFilterRejects_ShouldNotEvaluateRemainingFilters`

# Appender Dispatch Tests

## Logger should invoke only enabled appenders

Validated by:

* `Log_WithMixedAppenderStates_ShouldInvokeOnlyEnabledAppenders`

---

## Logger should skip disabled appenders

Validated by:

* `Log_WithDisabledAppender_ShouldNotAppend`

---

## Logger should dispatch identical log message instances to every enabled appender

Validated by:

* `Log_WithMultipleEnabledAppenders_ShouldPassSameLogMessageInstance`

# Convenience Method Tests

## Debug() should log using DEBUG level

Validated by:

* `Debug_ShouldLogWithDebugLevel`

---

## Info() should log using INFO level

Validated by:

* `Info_ShouldLogWithInfoLevel`

---

## Warning() should log using WARNING level

Validated by:

* `Warning_ShouldLogWithWarningLevel`

---

## Error() should log using ERROR level

Validated by:

* `Error_ShouldLogWithErrorLevel`

---

## Fatal() should log using FATAL level

Validated by:

* `Fatal_ShouldLogWithFatalLevel`

# Appender Management Tests

## Logger should allow appenders to be added

Validated by:

* `AddAppender_WithValidAppender_ShouldAddAppender`

---

## Logger should allow appenders to be removed

Validated by:

* `RemoveAppender_WithExistingAppender_ShouldRemoveAppender`

---

## Logger should ignore removal of unknown appenders

Validated by:

* `RemoveAppender_WithUnknownAppender_ShouldNotThrow`

Current behavior delegates to `List<T>.Remove()`.

---

## Logger should expose attached appenders as a read-only collection

Validated by:

* `GetLogAppenders_ShouldReturnReadOnlyCollection`

# Filter Management Tests

## Logger should allow filters to be added

Validated by:

* `AddFilter_WithValidFilter_ShouldAddFilter`

---

## Logger should allow filters to be removed

Validated by:

* `RemoveFilter_WithExistingFilter_ShouldRemoveFilter`

---

## Logger should ignore removal of unknown filters

Validated by:

* `RemoveFilter_WithUnknownFilter_ShouldNotThrow`

Current behavior delegates to `List<T>.Remove()`.

---

## Logger should expose configured filters as a read-only collection

Validated by:

* `GetLogFilters_ShouldReturnReadOnlyCollection`

# Configuration Update Tests

## Logger should update the effective log level

Validated by:

* `UpdateConfiguration_ShouldReplaceEffectiveLogLevel`

---

## Logger should replace existing appenders

Validated by:

* `UpdateConfiguration_ShouldReplaceAppenderCollection`

---

## Logger should remove previous appenders during configuration updates

Validated by:

* `UpdateConfiguration_ShouldDiscardExistingAppenders`

---

## Logger should immediately use the updated configuration

Validated by:

* `UpdateConfiguration_AfterUpdate_ShouldUseNewConfiguration`

# Input Validation Tests

## Logger should reject null messages

Validated by:

* `Log_WithNullMessage_ShouldThrowInvalidOperationException`

Current behavior is propagated from `LogMessage.Builder.Build()`.

---

## Logger should reject empty messages

Validated by:

* `Log_WithEmptyMessage_ShouldThrowInvalidOperationException`

---

## Logger should reject whitespace messages

Validated by:

* `Log_WithWhitespaceMessage_ShouldThrowInvalidOperationException`

---

## Logger should support Unicode log messages

Validated by:

* `Log_WithUnicodeMessage_ShouldAppendSuccessfully`

---

## Logger should support very long log messages

Validated by:

* `Log_WithVeryLongMessage_ShouldAppendSuccessfully`

---

## Logger should currently allow null appenders to be added

Validated by:

* `AddAppender_WithNullAppender_ShouldAllowAddition`

> **Note:** This reflects the current implementation. Logging subsequently results in a `NullReferenceException` when the pipeline attempts to invoke `IsEnabled()`.

---

## Logger should currently allow null filters to be added

Validated by:

* `AddFilter_WithNullFilter_ShouldAllowAddition`

> **Note:** This reflects the current implementation. Logging subsequently results in a `NullReferenceException` when `ShouldLog()` is invoked.

---

## Logger should reject null appender collections during configuration updates

Validated by:

* `UpdateConfiguration_WithNullAppenderCollection_ShouldThrowArgumentNullException`

(Currently propagated from `List<T>.AddRange()`.)

# Thread Safety Tests

## Logger should snapshot appenders before dispatching

Validated by:

* `Log_WhenAppenderCollectionChangesDuringLogging_ShouldUseSnapshot`

Verifies that appender enumeration is performed over a stable snapshot.

---

## Logger should continue dispatching without collection modification exceptions

Validated by:

* `Log_DuringConcurrentConfigurationUpdate_ShouldNotThrowCollectionModifiedException`

---

## Logger should replace appenders atomically during configuration updates

Validated by:

* `UpdateConfiguration_ShouldAtomicallyReplaceAppenderCollection`

# Test Scope

These tests validate only the public and internal observable behavior of **LoggerImplementation**.

The following responsibilities are intentionally tested separately within their respective components:

* `LogMessage` construction and builder validation
* `LogContext` correlation scope behavior
* Individual `ILogAppender` implementations
* Individual `ILogFilter` implementations
* Output formatting
* File/database/console writing
* Asynchronous appenders
* Rolling policies

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Constructor initialization |    ✅    |
| Logging pipeline           |    ✅    |
| Log message construction   |    ✅    |
| Log level filtering        |    ✅    |
| Filter evaluation          |    ✅    |
| Appender dispatch          |    ✅    |
| Convenience methods        |    ✅    |
| Appender management        |    ✅    |
| Filter management          |    ✅    |
| Configuration updates      |    ✅    |
| Input validation           |    ✅    |
| Exception handling         |    ✅    |
| Correlation ID propagation |    ✅    |
| Thread safety              |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>