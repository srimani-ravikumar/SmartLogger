# CorrelationToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                             |
| ------- | ---------- | ------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `CorrelationToken` strategy, validating token identification, correlation ID rendering, fallback behavior, and input validation. |

# Objective

Validate that **CorrelationToken** correctly renders the correlation identifier associated with a `LogMessage` by:

* Exposing the correct token identifier.
* Returning the configured correlation ID.
* Falling back to `"N/A"` when no correlation ID is available.
* Preserving valid correlation ID values.
* Rejecting a null `LogMessage`.
* Handling boundary and special input values appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates an independent `CorrelationToken` instance.
  * `LogMessage.Builder` is used to construct valid `LogMessage` instances.
  * Since `LogMessage.Builder.Build()` requires a log level and message, tests supplying a correlation ID also provide the minimum required values.

# Token Identification Tests

## Strategy should expose the correct correlation token identifier

Validated by:

* `Token_ShouldReturnCorrelationTokenIdentifier`

Verifies that the strategy identifies itself using:

```text
%CORRELATION
```

# Correlation ID Rendering Tests

## Strategy should return the configured correlation ID

Validated by:

* `Render_WithValidCorrelationId_ShouldReturnCorrelationId`

Verifies that a valid correlation ID is returned without modification.

## Strategy should preserve the original correlation ID value

Validated by:

* `Render_WithCorrelationIdContainingWhitespace_ShouldPreserveOriginalValue`

Ensures that a non-whitespace correlation ID containing leading or trailing whitespace is returned exactly as provided rather than being trimmed.

## Strategy should support Unicode correlation IDs

Validated by:

* `Render_WithUnicodeCorrelationId_ShouldReturnCorrelationId`

Verifies that Unicode characters in the correlation ID are rendered correctly.

## Strategy should support very long correlation IDs

Validated by:

* `Render_WithVeryLongCorrelationId_ShouldReturnCorrelationId`

Ensures that the renderer does not impose an artificial length restriction on correlation IDs.

# Fallback Rendering Tests

## Strategy should return "N/A" when correlation ID is null

Validated by:

* `Render_WithNullCorrelationId_ShouldReturnNA`

## Strategy should return "N/A" when correlation ID is empty

Validated by:

* `Render_WithEmptyCorrelationId_ShouldReturnNA`

## Strategy should return "N/A" when correlation ID contains only whitespace

Validated by:

* `Render_WithWhitespaceCorrelationId_ShouldReturnNA`

Verifies that `null`, empty, and whitespace-only correlation IDs are treated as missing correlation information.

# Input Validation Tests

## Strategy should reject a null log message

Validated by:

* `Render_WithNullMessage_ShouldThrowArgumentNullException`

Verifies that the renderer explicitly rejects a null `LogMessage` instead of attempting to access its properties.

## Strategy should provide the correct parameter name when log message is null

Validated by:

* `Render_WithNullMessage_ShouldIdentifyMessageParameter`

Ensures that the thrown `ArgumentNullException` identifies the `message` parameter correctly.

# Test Scope

These tests validate only the public behavior of **CorrelationToken**.

The following responsibilities are intentionally tested separately within their respective components:

* `LogMessage` construction and validation.
* `LogMessage.Builder` behavior.
* Correlation ID generation.
* Correlation ID propagation across execution flows.
* Token discovery and strategy registration.
* Token replacement within layouts.
* Rendering of other token types.
* Complete layout formatting.
* Formatter integration.

The tests should use `LogMessage.Builder` only as a means of supplying input to `CorrelationToken`; they should not duplicate or validate the builder's own behavior.

# Coverage Summary

| Area                               | Covered |
| ---------------------------------- | :-----: |
| Token identification               |    ✅    |
| Valid correlation ID rendering     |    ✅    |
| Null correlation ID fallback       |    ✅    |
| Empty correlation ID fallback      |    ✅    |
| Whitespace correlation ID fallback |    ✅    |
| Whitespace preservation            |    ✅    |
| Unicode correlation IDs            |    ✅    |
| Very long correlation IDs          |    ✅    |
| Null message validation            |    ✅    |
| Exception parameter validation     |    ✅    |
| Layout integration                 |    ❌    |
| Token registration/discovery       |    ❌    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>