# PatternLayout Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                |
| ------- | ---------- | ------- | ------------- | ---------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Unit test plan for PatternLayout token-based log rendering |

# Objective

Validate that `PatternLayout` correctly renders log messages by:

* Returning the raw message when the pattern is null, empty, or whitespace.
* Replacing registered tokens with their rendered values.
* Performing global replacement for every occurrence of a token.
* Preserving literal text and unsupported/unregistered tokens.
* Delegating token value generation to `ITokenRendererStrategy`.
* Handling null inputs and collaborator failures according to the current implementation.

# Test Environment

* **Framework:** NUnit
* **Mocking:** Moq
* **Runtime:** .NET
* **Class Under Test:** `PatternLayout`
* **Dependencies:** `TokenRegistry`, `ITokenRendererStrategy`, `LogMessage`

# Pattern Initialization

## 1. Constructor accepts a valid pattern

**Validated by:**

* `Constructor_WithValidPattern_CreatesInstanceSuccessfully`

## 2. Constructor accepts null pattern

**Validated by:**

* `Constructor_WithNullPattern_CreatesInstanceSuccessfully`

> Current implementation performs no validation in the constructor.

## 3. Constructor accepts null TokenRegistry

**Validated by:**

* `Constructor_WithNullTokenRegistry_CreatesInstanceSuccessfully`

> The failure occurs when `Render()` attempts to access the registry.

# Basic Rendering

## 4. Render returns pattern with token replaced

**Validated by:**

* `Render_WithRegisteredToken_ReplacesTokenWithRenderedValue`

Example:

```text
Pattern: "[%LEVEL] %MESSAGE"
Result:  "[INFO] Application started"
```

## 5. Render preserves literal text

**Validated by:**

* `Render_WithLiteralText_PreservesLiteralText`

## 6. Render supports multiple different tokens

**Validated by:**

* `Render_WithMultipleTokens_ReplacesAllRegisteredTokens`

## 7. Render preserves unknown/unregistered tokens

**Validated by:**

* `Render_WithUnknownToken_PreservesUnknownToken`

Example:

```text
Pattern: "[%LEVEL] %UNKNOWN %MESSAGE"
Result:  "[INFO] %UNKNOWN Application started"
```
# Token Replacement

## 8. Token replacement is global

**Validated by:**

* `Render_WithRepeatedToken_ReplacesEveryOccurrence`

Example:

```text
Pattern: "%LEVEL | %LEVEL | %LEVEL"
Result:  "INFO | INFO | INFO"
```

## 9. Token renderer is invoked for replacement

**Validated by:**

* `Render_WithRegisteredToken_InvokesTokenRenderer`

## 10. Same token renderer is used for all occurrences

**Validated by:**

* `Render_WithRepeatedToken_UsesRendererForEachReplacement`

> This captures the current `string.Replace(token.Token, token.Render(message))` behavior.

## 11. Token renderer receives the same LogMessage instance

**Validated by:**

* `Render_PassesOriginalLogMessageToTokenRenderer`

This is important because `PatternLayout` should not construct or modify the `LogMessage`.

# Pattern Edge Cases

## 12. Empty pattern returns raw message

**Validated by:**

* `Render_WithEmptyPattern_ReturnsRawMessage`

## 13. Whitespace-only pattern returns raw message

**Validated by:**

* `Render_WithWhitespacePattern_ReturnsRawMessage`

## 14. Null pattern returns raw message

**Validated by:**

* `Render_WithNullPattern_ReturnsRawMessage`

## 15. Pattern containing only a token

**Validated by:**

* `Render_WithTokenOnlyPattern_ReturnsRenderedToken`

## 16. Pattern containing no tokens

**Validated by:**

* `Render_WithLiteralOnlyPattern_ReturnsPatternUnchanged`

## 17. Pattern preserves whitespace

**Validated by:**

* `Render_WithWhitespaceAroundTokens_PreservesWhitespace`

## 18. Pattern preserves special characters and Unicode

**Validated by:**

* `Render_WithSpecialCharactersAndUnicode_PreservesContent`

# Token Rendering Results

## 19. Token returning empty string

**Validated by:**

* `Render_WhenTokenRendererReturnsEmptyString_RemovesToken`

## 20. Token returning whitespace

**Validated by:**

* `Render_WhenTokenRendererReturnsWhitespace_PreservesWhitespaceValue`

## 21. Token returning special characters

**Validated by:**

* `Render_WhenTokenRendererReturnsSpecialCharacters_PreservesRenderedValue`

## 22. Token returning Unicode

**Validated by:**

* `Render_WhenTokenRendererReturnsUnicode_PreservesRenderedValue`

# Multiple Token Dependencies

## 23. Multiple registered tokens are processed independently

**Validated by:**

* `Render_WithMultipleRegisteredTokens_ProcessesEachTokenIndependently`

## 24. Token processing does not alter unrelated literal content

**Validated by:**

* `Render_WithMultipleTokens_PreservesUnrelatedLiteralContent`

## 25. Token replacement follows registry enumeration

**Validated by:**

* `Render_WithMultipleTokens_ProducesExpectedCombinedResult`

> The implementation iterates through `TokenRegistry.Tokens.Values`; therefore the registry determines the available token set and enumeration order.

# Null / Failure Behavior

## 26. Render with null LogMessage

**Validated by:**

* `Render_WithNullMessage_ThrowsExpectedException`

For a non-empty pattern, the current implementation eventually passes `null` to `token.Render(message)`, or accesses `message.Message` for an empty pattern.

The test should capture the **actual current exception behavior**, rather than imposing new validation.

## 27. Null TokenRegistry

**Validated by:**

* `Render_WithNullTokenRegistry_ThrowsNullReferenceException`

## 28. Token renderer throws exception

**Validated by:**

* `Render_WhenTokenRendererThrows_PropagatesException`

`PatternLayout` does not catch token-rendering exceptions.

# State & Reusability

## 29. Multiple Render calls are independent

**Validated by:**

* `Render_CalledMultipleTimes_ProducesCorrectResults`

## 30. Rendering does not mutate the original pattern

**Validated by:**

* `Render_CalledMultipleTimes_DoesNotMutatePattern`

## 31. Same PatternLayout can render different messages

**Validated by:**

* `Render_WithDifferentMessages_ProducesCorrespondingResults`

# Test Scope

## PatternLayout Responsibilities

**In scope:**

* Pattern handling
* Empty/null/whitespace fallback
* Token discovery through `TokenRegistry`
* Token replacement
* Global replacement behavior
* Literal text preservation
* Delegation to token renderers
* Exception propagation
* Repeated rendering

## Out of Scope

**TokenRegistry:**

* Token registration
* Duplicate-token handling
* Token lookup semantics
* Registry construction

**Individual Token Renderers:**

* Timestamp formatting
* Log-level formatting
* Message extraction
* Source extraction
* Thread ID rendering
* Correlation ID rendering

These should be covered by their respective `*Token` unit-test suites.

# Coverage Summary

| Area                         | Scenarios |
| ---------------------------- | --------: |
| Constructor / Initialization |         3 |
| Basic Rendering              |         4 |
| Token Replacement            |         4 |
| Pattern Edge Cases           |         7 |
| Token Rendering Results      |         4 |
| Multiple Token Dependencies  |         3 |
| Null / Failure Behavior      |         3 |
| State & Reusability          |         3 |
| **Total**                    |    **31** |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>