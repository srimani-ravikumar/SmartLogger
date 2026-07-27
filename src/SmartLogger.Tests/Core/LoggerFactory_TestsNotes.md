# LoggerFactory Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                                                                  |
------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
1.0.0   | 2026-07-11 | Srimani | Initial Draft | Defined the unit test coverage for the `LoggerFactory` class, validating logger creation, caching, configuration resolution, appender composition, configuration updates, thread safety, and error handling. |

# Objective

Validate that **LoggerFactory** correctly acts as the composition root for the SmartLogger framework by:

* Creating configured logger instances.
* Caching logger instances.
* Resolving effective log levels.
* Constructing appenders from configuration.
* Supporting asynchronous logging.
* Refreshing existing logger configuration during runtime configuration updates.
* Handling invalid configuration and unsupported scenarios appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a new `LoggerFactory` instance with an isolated configuration.
  * File appender registry state should be reset between tests to avoid cache interference.
  * Tests accessing internal members use `InternalsVisibleTo`.

---

# Constructor Tests

## Factory should initialize successfully with a valid configuration provider

Validated by:

* `Constructor_WithValidProvider_ShouldLoadConfiguration`

Verifies that the configuration is loaded during factory construction.

## Factory should reject a null configuration provider

Validated by:

* `Constructor_WithNullProvider_ShouldThrowArgumentNullException`

## Factory should propagate configuration loading failures

Validated by:

* `Constructor_WhenProviderLoadThrows_ShouldPropagateException`

Ensures initialization does not suppress configuration provider failures.

---

# Logger Creation Tests

## Factory should create a logger for a valid logger name

Validated by:

* `GetOrCreateLogger_WithValidName_ShouldReturnLogger`

## Factory should create a logger only once per logical logger name

Validated by:

* `GetOrCreateLogger_WithSameName_ShouldReturnCachedInstance`

Verifies that repeated requests return the same cached logger instance.

## Factory should create separate logger instances for different logger names

Validated by:

* `GetOrCreateLogger_WithDifferentNames_ShouldReturnDifferentInstances`

## Factory should support creating loggers with very long names

Validated by:

* `GetOrCreateLogger_WithVeryLongName_ShouldReturnLogger`

## Factory should support Unicode logger names

Validated by:

* `GetOrCreateLogger_WithUnicodeName_ShouldReturnLogger`

## Factory should support empty logger names

Validated by:

* `GetOrCreateLogger_WithEmptyName_ShouldReturnLogger`

## Factory should reject a null logger name

Validated by:

* `GetOrCreateLogger_WithNullName_ShouldThrowArgumentNullException`

(Currently propagated by `ConcurrentDictionary`.)

---

# Logger Cache Tests

## Factory should cache logger instances

Validated by:

* `GetOrCreateLogger_WithRepeatedRequests_ShouldReturnSameInstance`

## Factory should preserve cached logger instances after configuration updates

Validated by:

* `UpdateConfiguration_ShouldReuseExistingLoggerInstances`

Configuration updates must refresh existing logger configuration rather than replacing cached logger objects.

---

# Log Level Resolution Tests

## Factory should apply the root log level when no appenders are configured

Validated by:

* `GetOrCreateLogger_WithoutAppenders_ShouldUseRootLogLevel`

## Factory should resolve the lowest configured appender log level

Validated by:

* `GetOrCreateLogger_WithMultipleAppenders_ShouldUseLowestAppenderLogLevel`

Verifies that the effective minimum log level is derived from the minimum configured appender threshold.

## Factory should fall back to the root log level when appender log levels are unspecified

Validated by:

* `GetOrCreateLogger_WithAppenderWithoutLevel_ShouldUseRootLogLevel`

## Factory should apply logger-specific overrides

Validated by:

* `GetOrCreateLogger_WithLoggerOverride_ShouldUseOverrideLogLevel`

## Factory should use the last matching logger override

Validated by:

* `GetOrCreateLogger_WithMultipleOverrides_ShouldUseLastMatchingOverride`

Reflects the current implementation using `LastOrDefault()`.

---

# Console Appender Tests

## Factory should attach configured console appenders

Validated by:

* `GetOrCreateLogger_WithConsoleAppender_ShouldAttachConsoleAppender`

## Factory should avoid adding the default console appender when appenders are explicitly configured

Validated by:

* `GetOrCreateLogger_WithConfiguredAppender_ShouldDisableDefaultConsoleAppender`

## Factory should enable the default console appender when no appenders are configured

Validated by:

* `GetOrCreateLogger_WithoutConfiguredAppenders_ShouldEnableDefaultConsoleAppender`

---

# File Appender Tests

## Factory should create file appenders for file destinations

Validated by:

* `GetOrCreateLogger_WithFileAppender_ShouldAttachFileAppender`

## Factory should reuse cached file appenders for identical file targets

Validated by:

* `GetOrCreateLogger_WithSharedFileConfiguration_ShouldReuseCachedAppender`

Verifies that file appenders are shared based on file identity through `FileAppenderRegistry`.

## Factory should preserve file appender identity when formatter changes

Validated by:

* `GetOrCreateLogger_WithSameFileButDifferentFormatter_ShouldReuseCachedAppender`

Verifies that the existing file appender instance is reused while its formatter configuration is refreshed.

## Factory should preserve file appender identity when appender log level changes

Validated by:

* `GetOrCreateLogger_WithSameFileButDifferentLogLevel_ShouldReuseCachedAppender`

Verifies that the existing file appender instance is reused while its minimum log level is refreshed.

## Factory should preserve file appender identity when rolling strategy changes

Validated by:

* `GetOrCreateLogger_WithSameFileButDifferentRollingStrategy_ShouldReuseCachedAppender`

Verifies that the existing file appender instance is reused while its rolling strategy is refreshed.

## Factory should create independent file appenders for different file identities

Validated by:

* `GetOrCreateLogger_WithDifferentFileTargets_ShouldCreateDifferentFileAppenders`

---

# Async Appender Tests

## Factory should wrap non-file appenders when asynchronous logging is enabled

Validated by:

* `GetOrCreateLogger_WithAsyncEnabled_ShouldWrapConsoleAppender`

## Factory should not wrap appenders when asynchronous logging is disabled

Validated by:

* `GetOrCreateLogger_WithAsyncDisabled_ShouldNotWrapAppender`

## Factory should wrap file appenders only once

Validated by:

* `GetOrCreateLogger_WithAsyncFileAppender_ShouldNotDoubleWrapAppender`

Verifies the behavior implemented by `FileAppenderRegistry`.

---

# Configuration Update Tests

## Factory should reject a null configuration

Validated by:

* `UpdateConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException`

## Factory should update the effective log level of existing loggers

Validated by:

* `UpdateConfiguration_ShouldRefreshLoggerLogLevel`

## Factory should replace the appender collection of existing loggers

Validated by:

* `UpdateConfiguration_ShouldReplaceExistingAppenders`

Verifies that logger appenders are replaced rather than appended.

## Factory should preserve cached logger instances during configuration updates

Validated by:

* `UpdateConfiguration_ShouldRetainLoggerCache`

## Factory should apply updated logger overrides

Validated by:

* `UpdateConfiguration_WithUpdatedOverride_ShouldRefreshEffectiveLogLevel`

## Factory should rebuild the logger appender collection using the latest configuration

Validated by:

* `UpdateConfiguration_ShouldRebuildAppenderCollection`

Verifies that logger appenders are rebuilt from the latest configuration while preserving cached logger instances.

## Factory should refresh cached file appenders during configuration updates

Validated by:

* `UpdateConfiguration_ShouldRefreshCachedFileAppender`

Verifies that existing cached file appenders receive updated formatter, log level and rolling strategy without creating duplicate file writers.

## Factory should preserve cached file appender identity during configuration updates

Validated by:

* `UpdateConfiguration_ShouldReuseExistingFileAppender`

Ensures runtime configuration reload updates existing file appenders rather than replacing them.

## Factory should update every cached logger

Validated by:

* `UpdateConfiguration_WithMultipleCachedLoggers_ShouldRefreshAllLoggers`

## Factory should ignore cached objects that are not LoggerImplementation instances

Validated by:

* `UpdateConfiguration_WithNonLoggerImplementation_ShouldSkipUpdate`

Reflects the defensive type check implemented by the factory.

---

# Unsupported Configuration Tests

## Factory should reject unsupported output destinations

Validated by:

* `GetOrCreateLogger_WithUnsupportedDestination_ShouldThrowNotSupportedException`

## Factory should propagate formatter creation failures

Validated by:

* `GetOrCreateLogger_WithUnsupportedFormatter_ShouldThrowNotSupportedException`

## Factory should allow appenders without rolling strategies

Validated by:

* `GetOrCreateLogger_WithNoRollingStrategy_ShouldCreateFileAppender`

Verifies that `RollingFactory.Create()` returning `null` is supported.

---

# Exception Handling Tests

## Factory should propagate failures occurring during appender creation

Validated by:

* `GetOrCreateLogger_WhenAppenderCreationFails_ShouldPropagateException`

## Factory should propagate failures occurring during configuration update

Validated by:

* `UpdateConfiguration_WhenAppenderCreationFails_ShouldPropagateException`

## Factory should propagate formatter creation failures during configuration updates

Validated by:

* `UpdateConfiguration_WithUnsupportedFormatter_ShouldThrowNotSupportedException`

---

# Thread Safety Tests

## Factory should create only one logger instance during concurrent requests

Validated by:

* `GetOrCreateLogger_WhenCalledConcurrently_ShouldCreateSingleLoggerInstance`

Verifies the thread-safe behavior of `ConcurrentDictionary.GetOrAdd()`.

## Factory should safely create multiple independent loggers concurrently

Validated by:

* `GetOrCreateLogger_WithConcurrentDifferentNames_ShouldCreateIndependentLoggers`

## Factory should safely refresh logger configuration during concurrent access

Validated by:

* `UpdateConfiguration_DuringConcurrentLoggerAccess_ShouldRemainConsistent`

Verifies that runtime configuration updates do not corrupt the logger cache or file appender registry.

---

# Test Scope

These tests validate only the behavior of **LoggerFactory**.

The following responsibilities are intentionally tested separately within their respective components:

* Logger logging pipeline (`LoggerImplementation`)
* Message construction
* Log dispatch
* Log filtering
* Formatter selection (`FormatterFactory`)
* Layout selection (`LayoutFactory`)
* Rolling strategy selection (`RollingFactory`)
* File appender registry lifecycle and configuration refresh (`FileAppenderRegistry`)
* Individual appender implementations (`ConsoleAppender`, `FileAppender`, etc.)
* Asynchronous appender execution
* File rolling strategies

---

# Coverage Summary

| Area                          | Covered |
| ----------------------------- | :-----: |
| Factory construction          |    ✅    |
| Logger creation               |    ✅    |
| Logger caching                |    ✅    |
| Log level resolution          |    ✅    |
| Logger override resolution    |    ✅    |
| Console appender creation     |    ✅    |
| File appender creation        |    ✅    |
| File appender caching         |    ✅    |
| Runtime file appender refresh |    ✅    |
| Async appender composition    |    ✅    |
| Configuration updates         |    ✅    |
| Existing logger refresh       |    ✅    |
| Input validation              |    ✅    |
| Exception propagation         |    ✅    |
| Unsupported configuration     |    ✅    |
| Thread safety                 |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>