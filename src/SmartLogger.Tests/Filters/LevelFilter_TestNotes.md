# LevelFilter Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                           |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `LevelFilter` class, validating default behavior, log-level filtering, null handling, threshold management, and dynamic configuration changes. |

# Objective

Validate that **LevelFilter** correctly filters log messages based on their `LogLevel` by:

* Applying the default `DEBUG` threshold.
* Supporting a custom minimum log level.
* Allowing messages equal to or above the configured threshold.
* Rejecting messages below the configured threshold.
* Safely rejecting null messages.
* Supporting dynamic log-level updates.
* Correctly exposing the current filtering threshold.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * A new `LevelFilter` instance should be created for every test.
  * No shared or static state exists in `LevelFilter`.
* Test Data:

  * `LogMessage` instances should be created through `LogMessage.Builder`.
  * Tests should use the complete `LogLevel` range where boundary behavior is relevant.

# Initialization Tests

## Filter should default to DEBUG level

Validated by:

* `Constructor_WithoutLevel_ShouldDefaultToDebug`

Verifies that the parameterless constructor initializes the threshold to `LogLevel.DEBUG`.

## Filter should initialize with the supplied log level

Validated by:

* `Constructor_WithLogLevel_ShouldSetThreshold`

## Filter should preserve NONE as the configured threshold

Validated by:

* `Constructor_WithNoneLevel_ShouldSetThresholdToNone`

## Filter should preserve FATAL as the configured threshold

Validated by:

* `Constructor_WithFatalLevel_ShouldSetThresholdToFatal`

# Message Filtering Tests

## Filter should reject a null message

Validated by:

* `ShouldLog_WithNullMessage_ShouldReturnFalse`

Null messages must never pass the filter.

## Filter should allow a message at the exact threshold

Validated by:

* `ShouldLog_WithEqualLevel_ShouldReturnTrue`

The filtering rule is inclusive:

```text
message level >= configured threshold
```

## Filter should allow a message above the threshold

Validated by:

* `ShouldLog_WithHigherLevel_ShouldReturnTrue`

## Filter should reject a message below the threshold

Validated by:

* `ShouldLog_WithLowerLevel_ShouldReturnFalse`

# Log-Level Boundary Tests

## DEBUG threshold should allow DEBUG and all higher levels

Validated by:

* `ShouldLog_WithDebugThreshold_ShouldAllowDebugAndAbove`

Expected:

```text
DEBUG    → true
INFO     → true
WARNING  → true
ERROR    → true
FATAL    → true
```

## INFO threshold should reject DEBUG and allow INFO and above

Validated by:

* `ShouldLog_WithInfoThreshold_ShouldFilterBelowInfo`

Expected:

```text
DEBUG    → false
INFO     → true
WARNING  → true
ERROR    → true
FATAL    → true
```

## WARNING threshold should reject levels below WARNING

Validated by:

* `ShouldLog_WithWarningThreshold_ShouldFilterBelowWarning`

## ERROR threshold should reject levels below ERROR

Validated by:

* `ShouldLog_WithErrorThreshold_ShouldFilterBelowError`

## FATAL threshold should allow only FATAL

Validated by:

* `ShouldLog_WithFatalThreshold_ShouldAllowOnlyFatal`

## NONE threshold should allow every defined log level

Validated by:

* `ShouldLog_WithNoneThreshold_ShouldAllowAllLevels`

Because `NONE = 0`, all defined levels satisfy:

```text
(int)message.LogLevel >= (int)LogLevel.NONE
```

# NONE Message Tests

## NONE-level message should be rejected by a DEBUG threshold

Validated by:

* `ShouldLog_WithNoneMessageAndDebugThreshold_ShouldReturnFalse`

## NONE-level message should be rejected by an INFO threshold

Validated by:

* `ShouldLog_WithNoneMessageAndInfoThreshold_ShouldReturnFalse`

## NONE-level message should be accepted by a NONE threshold

Validated by:

* `ShouldLog_WithNoneMessageAndNoneThreshold_ShouldReturnTrue`

These tests document the current numeric comparison semantics rather than treating `NONE` as a special disabled-message state.

# Log-Level Management Tests

## Filter should update its threshold using SetLogLevel

Validated by:

* `SetLogLevel_ShouldUpdateThreshold`

## Filter should return the current threshold using GetLogLevel

Validated by:

* `GetLogLevel_ShouldReturnCurrentThreshold`

## Filter should apply the updated threshold to subsequent messages

Validated by:

* `SetLogLevel_ShouldAffectSubsequentFiltering`

Example:

```text
Initial threshold: INFO

WARNING → allowed

SetLogLevel(ERROR)

WARNING → rejected
ERROR   → allowed
```

## Filter should support changing threshold multiple times

Validated by:

* `SetLogLevel_CalledMultipleTimes_ShouldUseLatestThreshold`

Verifies that only the latest configured level controls filtering.

# Complete Level Matrix Tests

## Filter should follow the inclusive level comparison for every threshold

Validated by:

* `ShouldLog_ForEveryThreshold_ShouldFollowExpectedComparison`

The complete matrix should verify:

| Threshold | NONE | DEBUG | INFO | WARNING | ERROR | FATAL |
| --------- | :--: | :---: | :--: | :-----: | :---: | :---: |
| NONE      |   ✅  |   ✅   |   ✅  |    ✅    |   ✅   |   ✅   |
| DEBUG     |   ❌  |   ✅   |   ✅  |    ✅    |   ✅   |   ✅   |
| INFO      |   ❌  |   ❌   |   ✅  |    ✅    |   ✅   |   ✅   |
| WARNING   |   ❌  |   ❌   |   ❌  |    ✅    |   ✅   |   ✅   |
| ERROR     |   ❌  |   ❌   |   ❌  |    ❌    |   ✅   |   ✅   |
| FATAL     |   ❌  |   ❌   |   ❌  |    ❌    |   ❌   |   ✅   |

This provides complete behavioral coverage without creating six separate implementations of the same comparison rule.

# Test Scope

These tests validate only the behavior of **LevelFilter**.

The following responsibility is intentionally tested separately within **LogLevelExtensions**:

* Numeric ordering of `LogLevel`.
* `IsGreaterOrEqual()` comparison semantics.

The following responsibilities are intentionally outside the scope of **LevelFilter**:

* Log message construction and validation.
* Message formatting.
* Appender behavior.
* Logger behavior.
* File/console output.
* Asynchronous processing.

# Coverage Summary

| Area                               | Covered |
| ---------------------------------- | :-----: |
| Default initialization             |    ✅    |
| Custom initialization              |    ✅    |
| Null message handling              |    ✅    |
| Equal-level filtering              |    ✅    |
| Higher-level filtering             |    ✅    |
| Lower-level filtering              |    ✅    |
| NONE threshold                     |    ✅    |
| NONE message                       |    ✅    |
| DEBUG threshold                    |    ✅    |
| INFO threshold                     |    ✅    |
| WARNING threshold                  |    ✅    |
| ERROR threshold                    |    ✅    |
| FATAL threshold                    |    ✅    |
| SetLogLevel                        |    ✅    |
| GetLogLevel                        |    ✅    |
| Dynamic threshold changes          |    ✅    |
| Complete level matrix              |    ✅    |
| LogLevel comparison implementation |    ❌*   |

* Intentionally covered by dedicated `LogLevelExtensions` tests.

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>