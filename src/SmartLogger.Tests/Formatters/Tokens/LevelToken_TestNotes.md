# LevelToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                 |
| ------- | ---------- | ------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `LevelToken` class, validating token identification, log level rendering, padding behavior, and null input handling. |

# Objective

Validate that **LevelToken** correctly renders the log level of a `LogMessage` by:

* Exposing the correct token identifier.
* Rendering the configured log level.
* Padding shorter log level names for consistent alignment.
* Preserving log level names that already meet or exceed the padding width.
* Rejecting null log messages.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `LevelToken` is stateless and does not require shared/static state reset.
  * A fresh `LevelToken` instance may be created for each test.

# Token Identification Tests

## Token should expose the expected token identifier

Validated by:

* `Token_ShouldReturnLevelTokenIdentifier`

Verifies that the renderer exposes `%LEVEL` as its token identifier.

# Log Level Rendering Tests

## Renderer should return the log level for a standard log level

Validated by:

* `Render_WithStandardLogLevel_ShouldReturnFormattedLevel`

Verifies that a typical log level such as `INFO` is rendered correctly.

## Renderer should pad log levels shorter than five characters

Validated by:

* `Render_WithShortLogLevel_ShouldPadResultToFiveCharacters`

For example:

```text
INFO → "INFO "
```

Ensures shorter level names are right-padded with spaces to maintain visual alignment.

## Renderer should not modify log levels that are exactly five characters

Validated by:

* `Render_WithFiveCharacterLogLevel_ShouldReturnUnchangedLevel`

For example:

```text
ERROR → "ERROR"
```

## Renderer should preserve log levels longer than five characters

Validated by:

* `Render_WithLongLogLevel_ShouldNotTruncateLevel`

Verifies that `PadRight(5)` does not truncate values whose length exceeds the requested padding width.

# Input Validation Tests

## Renderer should reject a null log message

Validated by:

* `Render_WithNullMessage_ShouldThrowArgumentNullException`

Verifies that rendering cannot be performed without a `LogMessage`.

## Renderer should identify the invalid argument correctly

Validated by:

* `Render_WithNullMessage_ShouldIdentifyMessageParameter`

Verifies that the thrown `ArgumentNullException` identifies `message` as the invalid parameter.

# Rendering Consistency Tests

## Renderer should produce deterministic output for the same log level

Validated by:

* `Render_WithSameMessage_ShouldReturnSameResult`

Ensures repeated rendering of the same message produces identical output.

## Renderer should render different log levels according to their values

Validated by:

* `Render_WithDifferentLogLevels_ShouldReturnCorrespondingValues`

Verifies that the renderer does not hard-code a particular log level and correctly derives the output from `LogMessage.LogLevel`.

# Test Scope

These tests validate only the public behavior of **LevelToken**.

The following responsibilities are intentionally tested separately within other components:

* Token discovery and token registration.
* Token parsing from format strings.
* `LogMessage` construction and validation.
* Log level semantics and severity ordering.
* Overall log message formatting.
* Combination of multiple token renderers.
* Formatter output composition.
* Thread safety and concurrent rendering.

# Coverage Summary

| Area                        | Covered |
| --------------------------- | :-----: |
| Token identification        |    ✅    |
| Standard level rendering    |    ✅    |
| Padding behavior            |    ✅    |
| Exact-width level rendering |    ✅    |
| Long level handling         |    ✅    |
| Null input validation       |    ✅    |
| Exception handling          |    ✅    |
| Parameter validation        |    ✅    |
| Rendering consistency       |    ✅    |
| Stateless behavior          |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>