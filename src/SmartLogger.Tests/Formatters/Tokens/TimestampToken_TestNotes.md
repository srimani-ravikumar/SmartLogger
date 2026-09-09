# TimestampToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                              |
| ------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `TimestampToken` class, validating token identification, default and custom date formatting, invariant culture behavior, and null input handling. |

# Objective

Validate that **TimestampToken** correctly renders the timestamp of a `LogMessage` by:

* Exposing the correct token identifier.
* Applying the default timestamp format.
* Applying a supplied custom date format.
* Preserving millisecond precision when requested by the format.
* Producing culture-independent output.
* Supporting valid custom format strings.
* Rejecting a null `LogMessage`.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `TimestampToken` is stateless after construction.
  * A fresh `TimestampToken` instance should be created for each test when testing different date formats.

# Token Identification Tests

## Token should expose the expected token identifier

Validated by:

* `Token_ShouldReturnTimestampTokenIdentifier`

Verifies that the renderer exposes `%TIMESTAMP` as its token identifier.

# Default Formatting Tests

## Renderer should use the default date format

Validated by:

* `Render_WithDefaultFormat_ShouldReturnFormattedTimestamp`

Verifies that the default format:

```text
yyyy-MM-dd HH:mm:ss.fff
```

is applied correctly.

## Renderer should include milliseconds using the default format

Validated by:

* `Render_WithDefaultFormat_ShouldIncludeMilliseconds`

Verifies that millisecond precision is rendered as exactly three digits.

# Custom Formatting Tests

## Renderer should apply a custom date format

Validated by:

* `Render_WithCustomFormat_ShouldReturnFormattedTimestamp`

Verifies that the supplied date format is used instead of the default format.

## Renderer should support date-only formatting

Validated by:

* `Render_WithDateOnlyFormat_ShouldReturnFormattedDate`

Verifies that custom formats can render only the date portion.

## Renderer should support time-only formatting

Validated by:

* `Render_WithTimeOnlyFormat_ShouldReturnFormattedTime`

Verifies that custom formats can render only the time portion.

## Renderer should support literal text in custom formats

Validated by:

* `Render_WithLiteralTextFormat_ShouldReturnFormattedTimestamp`

Verifies that literal content within a valid .NET date format is preserved.

## Renderer should honor the configured format consistently

Validated by:

* `Render_CalledMultipleTimes_ShouldUseConfiguredFormat`

Ensures the format supplied during construction remains unchanged across multiple render operations.

# Culture Handling Tests

## Renderer should produce culture-independent output

Validated by:

* `Render_WithDifferentCurrentCultures_ShouldReturnSameResult`

Verifies that rendering uses `CultureInfo.InvariantCulture` rather than the process's current culture.

The same timestamp and format should produce identical output regardless of the current system culture.

## Renderer should not depend on the current UI culture

Validated by:

* `Render_WithDifferentCurrentUICultures_ShouldReturnSameResult`

Ensures timestamp rendering remains deterministic when the current UI culture changes.

# Timestamp Boundary Tests

## Renderer should correctly format midnight timestamps

Validated by:

* `Render_WithMidnightTimestamp_ShouldReturnCorrectResult`

Verifies correct handling of the beginning of a day.

## Renderer should correctly format end-of-day timestamps

Validated by:

* `Render_WithEndOfDayTimestamp_ShouldReturnCorrectResult`

Verifies correct handling of timestamps close to midnight.

## Renderer should correctly format timestamps with zero milliseconds

Validated by:

* `Render_WithZeroMilliseconds_ShouldRenderThreeZeroDigits`

Verifies that the default format renders zero milliseconds as `000`.

## Renderer should correctly format timestamps with maximum millisecond precision

Validated by:

* `Render_WithMaximumMilliseconds_ShouldRenderCorrectValue`

Verifies correct rendering of timestamps containing `999` milliseconds.

# Input Validation Tests

## Renderer should reject a null log message

Validated by:

* `Render_WithNullMessage_ShouldThrowArgumentNullException`

Verifies that rendering cannot be performed without a `LogMessage`.

## Renderer should identify the invalid argument correctly

Validated by:

* `Render_WithNullMessage_ShouldIdentifyMessageParameter`

Verifies that the thrown `ArgumentNullException` identifies `message` as the invalid parameter.

# Test Scope

These tests validate only the public behavior of **TimestampToken**.

The following responsibilities are intentionally tested separately within other components:

* `LogMessage` construction and timestamp assignment.
* System clock/time generation.
* Time zone conversion.
* UTC/local time semantics.
* Date format configuration validation.
* Token discovery and token registration.
* Token parsing from format strings.
* Combination of multiple token renderers.
* Overall formatter output composition.
* Thread safety and concurrent rendering.

# Coverage Summary

| Area                      | Covered |
| ------------------------- | :-----: |
| Token identification      |    ✅    |
| Default date formatting   |    ✅    |
| Custom date formatting    |    ✅    |
| Millisecond precision     |    ✅    |
| Date-only formatting      |    ✅    |
| Time-only formatting      |    ✅    |
| Literal format handling   |    ✅    |
| Culture independence      |    ✅    |
| Current culture variation |    ✅    |
| Midnight boundary         |    ✅    |
| End-of-day boundary       |    ✅    |
| Zero milliseconds         |    ✅    |
| Maximum milliseconds      |    ✅    |
| Null input validation     |    ✅    |
| Exception handling        |    ✅    |
| Parameter validation      |    ✅    |
| Deterministic rendering   |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>