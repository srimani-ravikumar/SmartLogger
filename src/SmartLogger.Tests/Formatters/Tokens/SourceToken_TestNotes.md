# SourceToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                 |
| ------- | ---------- | ------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `SourceToken` class, validating token identification, source rendering, value preservation, and null input handling. |

# Objective

Validate that **SourceToken** correctly renders the source of a `LogMessage` by:

* Exposing the correct token identifier.
* Returning the exact source value.
* Preserving empty source values.
* Preserving whitespace and special characters.
* Supporting Unicode source names.
* Rejecting a null `LogMessage`.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `SourceToken` is stateless and does not require shared/static state reset.
  * A fresh `SourceToken` instance may be created for each test.

# Token Identification Tests

## Token should expose the expected token identifier

Validated by:

* `Token_ShouldReturnSourceTokenIdentifier`

Verifies that the renderer exposes `%SOURCE` as its token identifier.

# Source Rendering Tests

## Renderer should return the source value

Validated by:

* `Render_WithValidSource_ShouldReturnSource`

Verifies that the renderer returns the exact value provided by `LogMessage.Source`.

## Renderer should preserve an empty source

Validated by:

* `Render_WithEmptySource_ShouldReturnEmptyString`

Verifies that an explicitly empty source is returned unchanged.

## Renderer should preserve whitespace in the source

Validated by:

* `Render_WithWhitespaceSource_ShouldPreserveWhitespace`

Verifies that the renderer does not trim or otherwise modify whitespace in the source.

## Renderer should preserve special characters in the source

Validated by:

* `Render_WithSpecialCharacters_ShouldReturnSourceUnchanged`

Verifies that special characters are returned without modification.

## Renderer should support Unicode source names

Validated by:

* `Render_WithUnicodeSource_ShouldReturnSourceUnchanged`

Verifies that Unicode characters in logger/source names are preserved correctly.

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

## Renderer should produce deterministic output for the same source

Validated by:

* `Render_WithSameMessage_ShouldReturnSameResult`

Ensures repeated rendering of the same `LogMessage` produces identical output.

## Renderer should render different sources independently

Validated by:

* `Render_WithDifferentSources_ShouldReturnCorrespondingSource`

Verifies that the renderer does not retain or reuse previously rendered source values.

# Test Scope

These tests validate only the public behavior of **SourceToken**.

The following responsibilities are intentionally tested separately within other components:

* `LogMessage` construction and validation.
* Logger/source name generation.
* Token discovery and token registration.
* Token parsing from format strings.
* Combination of multiple token renderers.
* Overall formatter output composition.
* Thread safety and concurrent rendering.

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Token identification       |    ✅    |
| Valid source rendering     |    ✅    |
| Empty source handling      |    ✅    |
| Whitespace preservation    |    ✅    |
| Special character handling |    ✅    |
| Unicode handling           |    ✅    |
| Null input validation      |    ✅    |
| Exception handling         |    ✅    |
| Parameter validation       |    ✅    |
| Rendering consistency      |    ✅    |
| Stateless behavior         |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>