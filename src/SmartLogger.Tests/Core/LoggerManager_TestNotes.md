# LoggerManager Unit Tests

## Document Information

| Project     | Version | Date       | Author  | Status        | Description                                                                                                                                                            |
| ----------- | ------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SmartLogger | 1.0.0   | 2026-07-10 | Srimani | Initial Draft | Defined the unit test coverage for the `LoggerManager` class, validating initialization, logger retrieval, configuration reload, caching behavior, and error handling. |

# Objective

Validate that **LoggerManager** correctly acts as the public entry point for the SmartLogger framework by:

* Initializing the logging infrastructure.
* Retrieving configured logger instances.
* Caching logger instances.
* Reloading configuration.
* Guarding against invalid usage.
* Handling invalid input appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `LoggerManager.Reset()` is invoked in every `[SetUp]` because `LoggerManager` maintains static state.

# Initialization Tests

## Framework should initialize successfully with a valid configuration provider

Validated by:

* `Initialize_WithValidProvider_ShouldAllowLoggerCreation`

## Framework should reject a null configuration provider

Validated by:

* `Initialize_WithNullProvider_ShouldThrowArgumentNullException`

## Framework should allow re-initialization

Validated by:

* `Initialize_CalledTwice_ShouldReplaceExistingFactory`

Verifies that subsequent initialization replaces the existing factory without affecting future logger retrieval.

# Logger Retrieval Tests

## Framework should return a configured logger for a valid logger name

Validated by:

* `GetLogger_WithValidName_ShouldReturnLogger`

## Framework should cache logger instances

Validated by:

* `GetLogger_WithSameName_ShouldReturnSameInstance`

The same logical logger name must always return the same cached logger instance.

## Framework should create separate logger instances for different logger names

Validated by:

* `GetLogger_WithDifferentNames_ShouldReturnDifferentInstances`

## Framework should support retrieving a logger using a Type

Validated by:

* `GetLogger_WithValidType_ShouldReturnLogger`

## Framework should resolve identical logger instances for both overloads

Validated by:

* `GetLogger_WithType_ShouldReturnSameInstanceAsStringFullName`

Ensures that:

```text
GetLogger(typeof(MyClass))
```

and

```text
GetLogger(typeof(MyClass).FullName)
```

return the same cached logger instance.

# Input Validation Tests

## Framework should reject a null Type

Validated by:

* `GetLogger_WithNullType_ShouldThrowArgumentNullException`

## Framework should reject a null logger name

Validated by:

* `GetLogger_WithNullName_ShouldThrowArgumentNullException`

(Currently propagated from `ConcurrentDictionary`.)

## Framework should support empty logger names i.e. fallback to "DefaultLogger"

Validated by:

* `GetLogger_WithEmptyName_ShouldReturnLogger`

## Framework should support whitespace logger names i.e. fallback to "DefaultLogger"

Validated by:

* `GetLogger_WithWhitespaceName_ShouldReturnLogger`

## Framework should support very long logger names

Validated by:

* `GetLogger_WithVeryLongName_ShouldReturnLogger`

## Framework should support Unicode logger names

Validated by:

* `GetLogger_WithUnicodeName_ShouldReturnLogger`

# Initialization Guard Tests

## Framework should prevent logger retrieval before initialization

Validated by:

* `GetLogger_BeforeInitialize_ShouldThrowInvalidOperationException`
* `GetLogger_TypeBeforeInitialize_ShouldThrowInvalidOperationException`

Ensures consumers initialize the logging framework before requesting loggers.

## Framework should prevent configuration reload before initialization

Validated by:

* `ReloadConfiguration_BeforeInitialize_ShouldThrowInvalidOperationException`

# Configuration Reload Tests

## Framework should reload configuration using the supplied provider

Validated by:

* `ReloadConfiguration_WithValidProvider_ShouldCallLoad`

## Framework should invoke configuration loading for every reload request

Validated by:

* `ReloadConfiguration_ShouldCallLoadExactlyOnceDuringReload`
* `ReloadConfiguration_CalledMultipleTimes_ShouldCallLoadEachTime`

Verifies that every reload operation loads a fresh configuration snapshot.

## Framework should reject invalid reload requests

Validated by:

* `ReloadConfiguration_WithNullProvider_ShouldThrowNullReferenceException`

> **Note:** This reflects the current implementation. A future enhancement is to explicitly throw `ArgumentNullException` for consistency with `Initialize()`.

# Test Scope

These tests validate only the public behavior of **LoggerManager**.

The following responsibilities are intentionally tested separately within **LoggerFactory**:

* Logger creation
* Logger caching implementation
* Log level resolution
* Logger override resolution
* Appender creation
* Formatter selection
* Rolling policy creation
* Async appender wrapping
* Configuration propagation
* Thread safety

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Initialization                 |    ✅    |
| Re-initialization              |    ✅    |
| Logger retrieval by name       |    ✅    |
| Logger retrieval by type       |    ✅    |
| Logger caching                 |    ✅    |
| Input validation               |    ✅    |
| Exception handling             |    ✅    |
| Configuration reload           |    ✅    |
| Reload invocation verification |    ✅    |
| Static lifecycle management    |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
