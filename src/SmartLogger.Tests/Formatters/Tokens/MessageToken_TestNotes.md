# MessageToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                   |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `MessageToken` class, validating token identification, message rendering, null message handling, and input validation. |

# Objective

Validate that **MessageToken** correctly renders the message content of a `LogMessage` by:

* Exposing the correct token identifier.
* Returning the original message content.
* Preserving empty message values.
* Safely handling a null message content.
* Rejecting a null `LogMessage`.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `MessageToken` is stateless and does not require shared/static state reset.
  * A fresh `MessageToken` instance may be created for each test.

# Token Identification Tests

## Token should expose the expected token identifier

Validated by:

* `Token_ShouldReturnMessageTokenIdentifier`

Verifies that the renderer exposes `%MESSAGE` as its token identifier.

# Message Rendering Tests

## Renderer should return the message content

Validated by:

* `Render_WithValidMessage_ShouldReturnMessageContent`

Verifies that the renderer returns the exact message text provided by `LogMessage.Message`.

## Renderer should preserve an empty message

Validated by:

* `Render_WithEmptyMessage_ShouldReturnEmptyString`

Verifies that an explicitly empty message is returned as an empty string.

## Renderer should fallback to an empty string when message content is null

Validated by:

* `Render_WithNullMessageContent_ShouldReturnEmptyString`

Verifies the null-coalescing behavior:

```text
message.Message ?? string.Empty
```

## Renderer should preserve whitespace in the message

Validated by:

* `Render_WithWhitespaceMessage_ShouldPreserveWhitespace`

Verifies that the renderer returns whitespace exactly as provided and does not trim or otherwise modify the message.

## Renderer should preserve special characters

Validated by:

* `Render_WithSpecialCharacters_ShouldReturnMessageUnchanged`

Verifies that characters such as:

```text
Hello\nWorld\t!
```

are returned without modification.

## Renderer should support Unicode message content

Validated by:

* `Render_WithUnicodeMessage_ShouldReturnMessageUnchanged`

Verifies that Unicode characters are preserved correctly.

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

## Renderer should produce deterministic output for the same message

Validated by:

* `Render_WithSameMessage_ShouldReturnSameResult`

Ensures repeated rendering of the same `LogMessage` produces identical output.

## Renderer should render different messages independently

Validated by:

* `Render_WithDifferentMessages_ShouldReturnCorrespondingContent`

Verifies that the renderer does not retain or reuse previously rendered message content.

# Test Scope

These tests validate only the public behavior of **MessageToken**.

The following responsibilities are intentionally tested separately within other components:

* `LogMessage` construction and validation.
* Message formatting or transformation.
* Token discovery and token registration.
* Token parsing from format strings.
* Combination of multiple token renderers.
* Overall formatter output composition.
* Thread safety and concurrent rendering.

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Token identification       |    ✅    |
| Valid message rendering    |    ✅    |
| Empty message handling     |    ✅    |
| Null message content       |    ✅    |
| Whitespace preservation    |    ✅    |
| Special character handling |    ✅    |
| Unicode handling           |    ✅    |
| Null input validation      |    ✅    |
| Exception handling         |    ✅    |
| Parameter validation       |    ✅    |
| Rendering consistency      |    ✅    |
| Stateless behavior         |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>