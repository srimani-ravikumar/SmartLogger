# DailyRollingStrategy Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                   |
| ------- | ---------- | ------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `DailyRollingStrategy` class, validating daily boundary detection, rolling state management, repeated evaluations, and input handling. |

# Objective

Validate that **DailyRollingStrategy** correctly determines whether a log file should be rolled based on the current calendar day by:

* Detecting when the calendar day changes.
* Returning `false` while the current day remains unchanged.
* Returning `true` when a new day is detected.
* Updating its internal day state after a roll.
* Preventing repeated rolls for the same day.
* Maintaining independent rolling state across strategy instances.
* Handling the active file path parameter according to the current contract.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a new `DailyRollingStrategy`.
  * Tests involving date transitions should avoid relying on uncontrolled wall-clock changes where possible.
  * Since the current implementation directly uses `DateTime.Today`, deterministic date-transition testing may require a clock abstraction in a future implementation.

# Initial State Tests

## Strategy should not roll immediately after initialization

Validated by:

* `ShouldRoll_OnInitialEvaluation_ShouldReturnFalse`

The strategy initializes its internal state using:

```csharp
DateTime.Today
```

Therefore, an immediate evaluation on the same day should not trigger a roll.

# Daily Boundary Detection Tests

## Strategy should not roll when evaluated on the same day

Validated by:

* `ShouldRoll_OnSameDay_ShouldReturnFalse`

Verifies that repeated calls during the same calendar day continue to return:

```text
false
```

## Strategy should roll when the calendar day changes

Validated by:

* `ShouldRoll_WhenDayChanges_ShouldReturnTrue`

When the current date becomes later than the internally tracked day, the strategy should detect the daily boundary and request a roll.

## Strategy should update its current day after detecting a roll

Validated by:

* `ShouldRoll_WhenDayChanges_ShouldUpdateCurrentDay`

After detecting a new day, the strategy updates:

```csharp
_currentDay = DateTime.Today;
```

This ensures the detected day becomes the new rolling baseline.

## Strategy should roll only once after a day transition

Validated by:

* `ShouldRoll_WhenDayChanges_ShouldReturnTrueOnlyOnce`

The first evaluation after a day transition should return:

```text
true
```

Subsequent evaluations on the same day should return:

```text
false
```

This prevents multiple roll operations from being triggered for the same daily boundary.

# Repeated Evaluation Tests

## Strategy should remain stable across repeated evaluations on the same day

Validated by:

* `ShouldRoll_CalledMultipleTimesOnSameDay_ShouldAlwaysReturnFalse`

Verifies that repeated checks do not modify the internal rolling state unnecessarily.

## Strategy should detect each subsequent daily boundary

Validated by:

* `ShouldRoll_AcrossMultipleDays_ShouldRollOncePerDay`

Each new calendar day should independently become eligible for one rolling event.

> **Note:** With the current direct dependency on `DateTime.Today`, deterministic multi-day testing is difficult without controlling the system clock. This test is better supported after introducing a clock abstraction.

# Active File Path Tests

## Strategy should accept a valid active file path

Validated by:

* `ShouldRoll_WithValidActiveFilePath_ShouldEvaluateSuccessfully`

The active file path is accepted by the strategy as part of the `IRollingStrategy` contract.

The current implementation does not inspect or modify the path.

## Strategy should not depend on the active file path when determining daily rolling

Validated by:

* `ShouldRoll_WithDifferentActiveFilePaths_ShouldProduceSameRollingDecision`

For example:

```text
Logs/Application.log
Logs/OtherApplication.log
Archive/Application.log
```

should not affect the daily rolling decision.

The decision is based exclusively on the calendar day.

## Strategy should accept an empty active file path

Validated by:

* `ShouldRoll_WithEmptyActiveFilePath_ShouldEvaluateSuccessfully`

The current implementation does not use the supplied path, so an empty path should not affect the rolling decision.

> **Note:** This test documents the current contract. If `IRollingStrategy` later requires validation of the active file path, this behavior should be revisited.

## Strategy should accept a null active file path

Validated by:

* `ShouldRoll_WithNullActiveFilePath_ShouldEvaluateSuccessfully`

The current implementation does not access the supplied path, so `null` does not affect the rolling decision.

> **Note:** This reflects the current implementation. Explicit path validation would be appropriate only if the rolling strategy contract requires it.

# State Isolation Tests

## Separate strategy instances should maintain independent rolling state

Validated by:

* `ShouldRoll_WithSeparateInstances_ShouldMaintainIndependentState`

The `_currentDay` field belongs to each strategy instance and should not be shared across instances.

A roll detected by one strategy instance must not affect another instance.

# Thread Safety Tests

## Strategy should not be assumed to provide thread-safe rolling state management

Validated by:

* `ShouldRoll_ConcurrentEvaluation_ShouldNotBeConsideredThreadSafe`

The current implementation uses mutable state:

```csharp
private DateTime _currentDay;
```

without synchronization.

Thread-safety should therefore **not be considered part of the current class contract**.

Concurrent rolling behavior should be addressed at the higher-level rolling/file coordination component if that component guarantees synchronization.

> **Note:** This is primarily a design-scope observation rather than a conventional unit test. A concurrency test should only be added if `IRollingStrategy` explicitly requires thread safety.

# Test Scope

These tests validate only the responsibilities of **DailyRollingStrategy**.

The following responsibilities are intentionally tested separately within their respective components:

* File creation
* File writing
* Active file path management
* File naming
* Timestamp-based naming
* Size-based rolling
* File archival
* Archive compression
* Retention cleanup
* Filesystem operations
* Rolling orchestration
* Concurrent rolling coordination
* Logger lifecycle management

`DailyRollingStrategy` is responsible only for answering:

```text
"Has the calendar day changed since the last rolling check?"
```

# Implementation Considerations

The current implementation directly depends on:

```csharp
DateTime.Today
```

This creates a **testability boundary** because unit tests cannot deterministically move the application between calendar days.

For a production-grade logging framework, consider introducing a clock abstraction such as:

```csharp
TimeProvider
```

Then the strategy can evaluate the current date through the injected clock.

This would allow deterministic tests such as:

```text
Initial date → 2026-09-09
              ↓
Advance clock
              ↓
2026-09-10
              ↓
ShouldRoll() → true
              ↓
ShouldRoll() → false
```

without modifying the system clock or relying on `Thread.Sleep()`.

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Initial state              |    ✅    |
| Same-day evaluation        |    ✅    |
| Daily boundary detection   |    ✅    |
| Roll decision              |    ✅    |
| State update               |    ✅    |
| Single roll per day        |    ✅    |
| Repeated evaluation        |    ✅    |
| Multiple daily boundaries  |    ✅    |
| Active file path handling  |    ✅    |
| Path independence          |    ✅    |
| Null / empty path behavior |    ✅    |
| State isolation            |    ✅    |
| Thread-safety scope        |    ⚠️   |
| Filesystem interaction     |   N/A   |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
