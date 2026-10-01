# FileAppenderRegistry Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                     |
| ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `FileAppenderRegistry` class, validating appender creation, caching, async wrapping, configuration refresh, file identity, concurrent access, and static cache behavior. |

# Objective

Validate that **FileAppenderRegistry** correctly manages and reuses file-based appenders by:

* Creating a file appender when no cached instance exists.
* Returning the existing appender for the same file identity.
* Generating cache identity from file name and extension.
* Applying asynchronous wrapping only during initial creation.
* Refreshing log level and formatter configuration.
* Refreshing configuration for both direct and asynchronously wrapped file appenders.
* Maintaining a single logical appender under concurrent access.
* Preserving the current behavior when configuration differs for an existing file key.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `FileAppenderRegistry` maintains a static `ConcurrentDictionary`.
  * Tests must account for persistent registry state between test executions.
  * Each test should use a unique file identity where possible.
  * If test infrastructure permits controlled access to internal members, a registry reset mechanism should be used between tests.
* Filesystem:

  * `FileAppender` creates the configured directory and active file during creation.
  * Tests should use isolated temporary directories.
* Concurrency:

  * Concurrent `GetOrCreate()` calls should be tested using multiple producer threads/tasks.

# Appender Creation Tests

## Registry should create a file appender when no cached appender exists

Validated by:

* `GetOrCreate_WithNewFileConfiguration_ShouldCreateAppender`

## Registry should return a non-null appender after creation

Validated by:

* `GetOrCreate_WithValidConfiguration_ShouldReturnAppender`

## Registry should create a synchronous FileAppender when async mode is disabled

Validated by:

* `GetOrCreate_WithAsyncDisabled_ShouldReturnFileAppender`

Verifies that the returned instance is a direct `FileAppender` rather than an `AsyncAppenderWrapper`.

## Registry should create an asynchronous wrapper when async mode is enabled

Validated by:

* `GetOrCreate_WithAsyncEnabled_ShouldReturnAsyncAppenderWrapper`

Verifies that the created file appender is wrapped exactly once.

## Registry should use the supplied log level during appender creation

Validated by:

* `GetOrCreate_WithLogLevel_ShouldCreateAppenderWithConfiguredLogLevel`

## Registry should use the supplied formatter during appender creation

Validated by:

* `GetOrCreate_WithFormatter_ShouldCreateAppenderWithConfiguredFormatter`

## Registry should use the supplied rolling strategy during appender creation

Validated by:

* `GetOrCreate_WithRollingStrategy_ShouldCreateAppenderWithConfiguredStrategy`

## Registry should use the supplied naming strategy during appender creation

Validated by:

* `GetOrCreate_WithNamingStrategy_ShouldCreateAppenderSuccessfully`

# Cache Tests

## Registry should return the same appender for the same file identity

Validated by:

* `GetOrCreate_WithSameFileKey_ShouldReturnSameInstance`

The same logical file must map to the same cached appender.

## Registry should reuse the cached appender instead of creating another one

Validated by:

* `GetOrCreate_WithExistingFileKey_ShouldReuseCachedAppender`

## Registry should create different appenders for different file identities

Validated by:

* `GetOrCreate_WithDifferentFileNames_ShouldReturnDifferentInstances`

## Registry should distinguish files by extension

Validated by:

* `GetOrCreate_WithSameFileNameDifferentExtension_ShouldReturnDifferentInstances`

For example:

```text
Application.log
Application.txt
```

must produce different cache keys.

## Registry should use file name and extension as the cache identity

Validated by:

* `GetOrCreate_ShouldUseFileNameAndExtensionAsCacheKey`

The current key is:

```text
{FileName}.{Extension}
```

Other configuration properties do not participate in cache identity.

# Configuration Refresh Tests

## Registry should refresh log level on initial creation

Validated by:

* `GetOrCreate_ShouldRefreshLogLevelAfterCreation`

Although the log level is passed into `FileAppender` construction, `RefreshConfiguration()` is also called immediately afterward.

## Registry should refresh formatter on initial creation

Validated by:

* `GetOrCreate_ShouldRefreshFormatterAfterCreation`

## Registry should update the cached appender's log level

Validated by:

* `GetOrCreate_WithExistingAppender_ShouldRefreshLogLevel`

Example:

```text
First call:
INFO

Second call:
ERROR

Result:
cached appender uses ERROR
```

## Registry should update the cached appender's formatter

Validated by:

* `GetOrCreate_WithExistingAppender_ShouldRefreshFormatter`

## Registry should preserve the cached appender instance during configuration refresh

Validated by:

* `GetOrCreate_WithExistingAppender_ShouldRefreshConfigurationWithoutRecreatingAppender`

The second call must return the original cached instance.

# Async Configuration Tests

## Registry should wrap an appender only during initial creation

Validated by:

* `GetOrCreate_WithAsyncEnabled_ShouldWrapAppenderOnlyOnce`

## Registry should not create a second async wrapper for an existing async appender

Validated by:

* `GetOrCreate_WithExistingAsyncAppender_ShouldNotDoubleWrap`

Example:

```text
First call:

FileAppender
    ↓
AsyncAppenderWrapper


Second call:

same AsyncAppenderWrapper
```

The registry must not produce:

```text
AsyncAppenderWrapper
    ↓
AsyncAppenderWrapper
        ↓
FileAppender
```

## Registry should preserve an existing synchronous appender when a later request enables async

Validated by:

* `GetOrCreate_FirstSyncThenAsync_ShouldReturnExistingSyncAppender`

> **Note:** `asyncEnabled` is not part of the cache key. Therefore, once a synchronous appender exists for a file, a later request with `asyncEnabled = true` does not retrofit asynchronous behavior.

## Registry should preserve an existing asynchronous appender when a later request disables async

Validated by:

* `GetOrCreate_FirstAsyncThenSync_ShouldReturnExistingAsyncAppender`

> **Note:** This reflects the current cache contract.

# Configuration Difference Tests

## Registry should ignore log-level differences for cache identity

Validated by:

* `GetOrCreate_SameFileDifferentLogLevel_ShouldReturnSameAppender`

However, the cached appender's log level **is refreshed** to the latest supplied value.

## Registry should ignore formatter differences for cache identity

Validated by:

* `GetOrCreate_SameFileDifferentFormatter_ShouldReturnSameAppender`

However, the cached appender's formatter **is refreshed** to the latest supplied formatter.

## Registry should ignore rolling-strategy differences for cache identity

Validated by:

* `GetOrCreate_SameFileDifferentRollingStrategy_ShouldReturnSameAppender`

> **Note:** Unlike log level and formatter, the rolling strategy is **not refreshed** by `RefreshConfiguration()`.

Therefore, the rolling strategy supplied during the first creation remains associated with the underlying `FileAppender`.

## Registry should ignore naming-strategy differences for cache identity

Validated by:

* `GetOrCreate_SameFileDifferentNamingStrategy_ShouldReturnSameAppender`

The naming strategy is only used when the underlying `FileAppender` is initially constructed.

## Registry should ignore async configuration differences for cache identity

Validated by:

* `GetOrCreate_SameFileDifferentAsyncSetting_ShouldReturnSameAppender`

# Refresh Path Tests

## Registry should refresh a direct FileAppender

Validated by:

* `GetOrCreate_WithCachedFileAppender_ShouldRefreshConfiguration`

The following path should be exercised:

```text
FileAppender
    ↓
RefreshConfiguration()
    ↓
FileAppender.UpdateConfiguration()
```

## Registry should refresh the underlying FileAppender inside an AsyncAppenderWrapper

Validated by:

* `GetOrCreate_WithCachedAsyncWrapper_ShouldRefreshInnerFileAppender`

The following path should be exercised:

```text
AsyncAppenderWrapper
        ↓
InnerAppender
        ↓
FileAppender
        ↓
UpdateConfiguration()
```

## Registry should not replace the async wrapper during refresh

Validated by:

* `GetOrCreate_WithCachedAsyncWrapper_ShouldPreserveWrapperInstance`

## Registry should not modify rolling configuration during refresh

Validated by:

* `GetOrCreate_WithExistingAppender_ShouldNotRefreshRollingStrategy`

This documents the current `RefreshConfiguration()` signature:

```csharp
RefreshConfiguration(
    appender,
    logLevel,
    formatter,
    rollingStrategy);
```

Although `rollingStrategy` is passed into the method, it is currently unused.

# Input Validation and Failure Tests

## Registry should reject a null configuration

Validated by:

* `GetOrCreate_WithNullConfiguration_ShouldThrow`

> **Note:** The current implementation accesses `config.Destination` immediately, so the exact exception should reflect the current implementation rather than assuming explicit `ArgumentNullException` validation.

## Registry should reject a configuration without file configuration

Validated by:

* `GetOrCreate_WithNullFileConfiguration_ShouldThrow`

The implementation currently performs:

```csharp
var fileConfig = config.Destination.File!;
```

The null-forgiving operator only affects compilation; it does not prevent a runtime null reference.

## Registry should propagate invalid file configuration errors

Validated by:

* `GetOrCreate_WithInvalidFileConfiguration_ShouldPropagateException`

The registry does not catch exceptions from `FileAppender` construction or filesystem initialization.

# Concurrent Access Tests

## Concurrent requests for the same file should return the same logical appender

Validated by:

* `GetOrCreate_ConcurrentlyWithSameFile_ShouldReturnSameInstance`

Multiple callers requesting the same file identity should converge on the cached appender.

## Concurrent requests should not corrupt the cache

Validated by:

* `GetOrCreate_Concurrently_ShouldNotThrowCollectionExceptions`

The underlying cache uses `ConcurrentDictionary`.

## Concurrent requests for different files should create independent appenders

Validated by:

* `GetOrCreate_ConcurrentlyWithDifferentFiles_ShouldCreateDifferentInstances`

## Concurrent creation should not result in multiple observable cached instances

Validated by:

* `GetOrCreate_ConcurrentlyWithSameFile_ShouldCreateSingleCachedInstance`

This validates the intended lazy initialization behavior provided by `ConcurrentDictionary.GetOrAdd`.

> **Important:** `ConcurrentDictionary.GetOrAdd()` may invoke its value factory more than once under contention. The test should therefore focus on the **returned cached value** and externally observable registry behavior, rather than assuming the value factory itself executes exactly once.

# Static Cache Lifecycle Tests

## Cached appender should remain available across repeated calls

Validated by:

* `GetOrCreate_AfterPreviousCreation_ShouldReturnCachedAppender`

## Tests should isolate static registry state

Validated by:

* `TestSetup_ShouldPreventStaticCacheContamination`

Because `_cache` is static and there is currently no public/internal reset method, test isolation requires either:

* Unique file keys for every test, or
* A dedicated internal reset mechanism exposed specifically for tests.

> **Design Recommendation:** Consider adding an internal `Clear()`/`Reset()` method guarded for test infrastructure, or moving cache lifecycle management behind an injectable registry abstraction. Static mutable state makes deterministic unit testing harder.

# Test Scope

These tests validate the behavior of **FileAppenderRegistry** as an appender factory and cache.

The following responsibilities are intentionally tested separately within **FileAppender**:

* Message filtering
* Message formatting
* File writing
* File creation
* File append behavior
* Log-level comparison
* Formatter management
* File lifecycle management

The following responsibilities are intentionally tested separately within **AsyncAppenderWrapper**:

* Background worker processing
* Queue management
* FIFO behavior
* Queue backpressure
* Worker lifecycle
* Shutdown/draining
* Concurrent producers
* Worker exception isolation

The following responsibilities are intentionally tested separately within **IRollingStrategy** implementations:

* Daily rolling decisions
* Size-based rolling decisions
* Rolling threshold calculations

The following responsibilities are intentionally tested separately within **IFileNamingStrategy** implementations:

* File-name generation
* Date formatting
* Timestamp formatting
* Archive-name generation

# Implementation Notes

## Current cache identity

The cache key is:

```text
FileName + "." + Extension
```

Therefore:

```text
Application.log
```

and:

```text
Application.log
```

share an appender, while:

```text
Application.log
Application.txt
```

do not.

## Current configuration behavior

The implementation has an important asymmetry:

| Configuration    | Cache Identity | Refreshed on Cache Hit |
| ---------------- | -------------: | ---------------------: |
| File name        |              ✅ |                      ❌ |
| Extension        |              ✅ |                      ❌ |
| Log level        |              ❌ |                      ✅ |
| Formatter        |              ❌ |                      ✅ |
| Rolling strategy |              ❌ |                      ❌ |
| Naming strategy  |              ❌ |                      ❌ |
| Async enabled    |              ❌ |                      ❌ |

This should be explicitly represented in the test suite because it is the current implementation contract.

## Current async behavior

`asyncEnabled` affects only **initial appender creation**.

Once an appender is cached, changing the async setting has no effect.

## Current refresh behavior

`RefreshConfiguration()` accepts `rollingStrategy`, but does not use it.

Therefore, configuration reload can change:

```text
LogLevel
Formatter
```

but cannot change:

```text
RollingStrategy
NamingStrategy
Async behavior
```

without recreating the cached appender.

# Coverage Summary

| Area                             | Covered |
| -------------------------------- | :-----: |
| Appender creation                |    ✅    |
| File identity                    |    ✅    |
| Cache hit                        |    ✅    |
| Cache miss                       |    ✅    |
| File-name differentiation        |    ✅    |
| Extension differentiation        |    ✅    |
| Synchronous creation             |    ✅    |
| Asynchronous creation            |    ✅    |
| Async double-wrapping prevention |    ✅    |
| Log-level refresh                |    ✅    |
| Formatter refresh                |    ✅    |
| Rolling-strategy behavior        |    ✅    |
| Naming-strategy behavior         |    ✅    |
| Configuration differences        |    ✅    |
| Direct FileAppender refresh      |    ✅    |
| Async inner FileAppender refresh |    ✅    |
| Invalid configuration            |    ✅    |
| Concurrent access                |    ✅    |
| Static cache behavior            |    ✅    |
| Thread-safe cache access         |    ✅    |
| File I/O implementation          |    ❌*   |
| Async worker implementation      |    ❌*   |
| Rolling implementation           |    ❌*   |
| Naming implementation            |    ❌*   |

* Intentionally covered by dedicated component-level test plans.

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>