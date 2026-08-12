# LogLevel Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                               |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-08-12 | Srimani | Initial Draft | Defined concise unit test coverage for the `LogLevel` enum and `LogLevelExtensions.IsGreaterOrEqual` comparison behavior. |

# Objective

Validate that **LogLevel** and **LogLevelExtensions.IsGreaterOrEqual** correctly represent and compare logging severity levels by:

* Preserving the defined severity ordering.
* Returning `true` when the current level is greater than or equal to the threshold.
* Returning `false` when the current level is below the threshold.
* Correctly handling boundary values.
* Correctly comparing undefined enum values based on their underlying numeric value.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * `LogLevelExtensions.IsGreaterOrEqual` is stateless and has no external dependencies.
  * No `[SetUp]` or `[TearDown]` is required.

# LogLevel Definition Tests

## Framework should maintain the expected severity ordering

Validated by:

* `LogLevel_ShouldHaveExpectedNumericValues`

Verifies that the defined severity levels retain the expected numeric values:

```text
DEBUG = 1
INFO = 2
WARNING = 3
ERROR = 4
FATAL = 5
```

This is important because `IsGreaterOrEqual` relies on the underlying numeric ordering.

# LogLevel Comparison Tests

## Framework should correctly compare all defined severity levels

Validated by:

* `IsGreaterOrEqual_WithAllLogLevels_ShouldReturnExpectedResult`

Uses parameterized test cases to cover:

* Equality comparisons.
* Higher current level against lower threshold.
* Lower current level against higher threshold.
* Lowest boundary (`DEBUG`).
* Highest boundary (`FATAL`).
* All combinations of the five defined severity levels.

The complete comparison matrix is covered through test cases rather than separate test methods.

Expected behavior:

```text
current >= threshold
```

must return:

* `true` when `current` is equal to `threshold`.
* `true` when `current` is more severe than `threshold`.
* `false` when `current` is less severe than `threshold`.

# Undefined LogLevel Tests

## Framework should compare undefined enum values using their underlying numeric value

Validated by:

* `IsGreaterOrEqual_WithUndefinedValues_ShouldCompareUnderlyingValue`

C# permits undefined enum values through explicit casting. The extension method should continue to perform numeric comparison without throwing exceptions.

The test covers:

```text
(LogLevel)10 >= FATAL       → true
(LogLevel)0  >= DEBUG       → false
(LogLevel)10 >= (LogLevel)10 → true
FATAL >= (LogLevel)10       → false
```

This verifies:

* Undefined value greater than a defined value.
* Undefined value lower than a defined value.
* Equality between undefined values.
* Defined value lower than an undefined value.

# Test Scope

These tests validate only:

* `LogLevel` enum definitions.
* Numeric severity ordering.
* `LogLevelExtensions.IsGreaterOrEqual`.

The following are intentionally outside the scope:

* Logger creation.
* Logger configuration.
* Log filtering.
* Log formatting.
* Appenders.
* File/console output.
* Async logging.
* Rolling policies.
* Logger caching.
* Thread safety.

Those behaviors should be tested by their respective components.

# Coverage Summary

| Area                         | Covered |
| ---------------------------- | :-----: |
| Enum definitions             |    ✅    |
| Numeric severity ordering    |    ✅    |
| Equality comparison          |    ✅    |
| Greater-than comparison      |    ✅    |
| Less-than comparison         |    ✅    |
| All valid level combinations |    ✅    |
| `DEBUG` boundary             |    ✅    |
| `FATAL` boundary             |    ✅    |
| Undefined enum values        |    ✅    |
| Exception handling           |   N/A   |
| External dependencies        |   N/A   |
| Static state                 |   N/A   |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
