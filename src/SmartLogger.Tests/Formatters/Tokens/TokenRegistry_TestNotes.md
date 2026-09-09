# TokenRegistry Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                    |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `TokenRegistry` class, validating token registration, renderer resolution, duplicate handling, collection exposure, and input behavior. |

# Objective

Validate that **TokenRegistry** correctly manages and resolves token renderer strategies by:

* Registering supplied token renderers.
* Mapping each token identifier to its corresponding renderer.
* Preserving the original renderer instances.
* Supporting multiple token renderers.
* Rejecting duplicate token identifiers.
* Exposing the registered tokens through a read-only dictionary interface.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `TokenRegistry` contains instance-level state only.
  * A fresh `TokenRegistry` instance should be created for each test.

# Registration Tests

## Registry should register a single token renderer

Validated by:

* `Constructor_WithSingleToken_ShouldRegisterToken`

Verifies that a supplied token strategy is registered and accessible using its token identifier.

## Registry should register multiple token renderers

Validated by:

* `Constructor_WithMultipleTokens_ShouldRegisterAllTokens`

Verifies that all supplied token strategies are registered.

## Registry should preserve the original renderer instance

Validated by:

* `Tokens_WithRegisteredToken_ShouldReturnSameRendererInstance`

Ensures the registry stores and returns the exact renderer instance supplied during construction rather than creating or replacing it.

## Registry should map each token to its corresponding renderer

Validated by:

* `Tokens_WithMultipleTokens_ShouldMapEachTokenToCorrectRenderer`

Verifies that each token identifier resolves to its correct strategy.

# Lookup and Collection Tests

## Registry should expose the correct number of registered tokens

Validated by:

* `Tokens_WithMultipleTokens_ShouldContainExpectedCount`

Verifies that the registry contains exactly the supplied unique token strategies.

## Registry should allow token lookup through the exposed dictionary

Validated by:

* `Tokens_WithRegisteredToken_ShouldSupportDictionaryLookup`

Verifies that consumers can resolve a renderer using its token identifier.

## Registry should expose an empty collection when initialized with no tokens

Validated by:

* `Constructor_WithEmptyTokenCollection_ShouldCreateEmptyRegistry`

Verifies that an empty token collection results in a valid registry containing no entries.

# Duplicate Token Tests

## Registry should reject duplicate token identifiers

Validated by:

* `Constructor_WithDuplicateTokens_ShouldThrowArgumentException`

Verifies that duplicate token identifiers are rejected during dictionary construction.

The current implementation relies on `Enumerable.ToDictionary()` for duplicate detection.

## Registry should reject duplicate tokens even when renderer instances differ

Validated by:

* `Constructor_WithDifferentRenderersUsingSameToken_ShouldThrowArgumentException`

Ensures uniqueness is based on the **token identifier**, not renderer object identity.

# Input Handling Tests

## Registry should reject a null token collection

Validated by:

* `Constructor_WithNullTokens_ShouldThrowArgumentNullException`

Verifies the current behavior when the `tokens` argument itself is null.

> **Note:** This exception is currently propagated from LINQ's `ToDictionary()` implementation. If explicit argument validation is introduced later, this test should continue validating the intended contract rather than the framework-generated exception message.

## Registry should support a collection containing a single valid strategy

Validated by:

* `Constructor_WithValidTokenStrategy_ShouldCreateRegistry`

Verifies the basic construction path using a valid strategy.

# Token Identifier Tests

## Registry should preserve the exact token identifier

Validated by:

* `Tokens_ShouldPreserveTokenIdentifierExactly`

Verifies that token identifiers are not normalized, trimmed, upper-cased, or otherwise modified during registration.

## Registry should treat token identifiers according to dictionary key semantics

Validated by:

* `Constructor_WithDifferentCaseTokenIdentifiers_ShouldRegisterAccordingToDictionarySemantics`

Verifies the current case-sensitive behavior of `Dictionary<string, ...>`.

For example:

```text
%LEVEL
%level
```

are treated as different keys by the default comparer.

# Test Scope

These tests validate only the public behavior of **TokenRegistry**.

The following responsibilities are intentionally tested separately within other components:

* Token renderer implementation behavior.
* Token rendering logic.
* Token parsing from pattern layouts.
* Pattern matching and token discovery.
* Formatting of complete log messages.
* Token replacement/order during rendering.
* Thread safety and concurrent registry access.
* Dynamic registration after construction.
* Token strategy lifecycle management.

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Single token registration      |    ✅    |
| Multiple token registration    |    ✅    |
| Renderer instance preservation |    ✅    |
| Token-to-renderer mapping      |    ✅    |
| Token count                    |    ✅    |
| Dictionary lookup              |    ✅    |
| Empty collection               |    ✅    |
| Duplicate token handling       |    ✅    |
| Duplicate renderer instances   |    ✅    |
| Null input handling            |    ✅    |
| Token identifier preservation  |    ✅    |
| Case-sensitive key behavior    |    ✅    |
| Exception handling             |    ✅    |
| Collection exposure            |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>