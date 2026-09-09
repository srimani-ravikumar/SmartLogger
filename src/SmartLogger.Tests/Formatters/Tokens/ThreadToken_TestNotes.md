# ThreadToken Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                   |
| ------- | ---------- | ------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `ThreadToken` class, validating token identification, thread ID rendering, value conversion, boundary values, and null input handling. |

# Objective

Validate that **ThreadToken** correctly renders the thread identifier of a `LogMessage` by:

* Exposing the correct token identifier.
* Returning the thread ID as a string.
* Correctly handling zero and positive thread IDs.
* Correctly handling large thread IDs.
* Rejecting a null `LogMessage`.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * `ThreadToken` is stateless and does not require shared/static state reset.
  * A fresh `ThreadToken` instance may be created for each test.

# Token Identification Tests

## Token should expose the expected token identifier

Validated by:

* `Token_ShouldReturnThreadTokenIdentifier`

Verifies that the renderer exposes `%THREAD` as its token identifier.

# Thread ID Rendering Tests

## Renderer should return the thread ID as a string

Validated by:

* `Render_WithValidThreadId_ShouldReturnThreadIdAsString`

Verifies that the numeric `ThreadId` is converted to its string representation.

## Renderer should correctly render a zero thread ID

Validated by:

* `Render_WithZeroThreadId_ShouldReturnZeroAsString`

Verifies that `0` is rendered as `"0"`.

## Renderer should correctly render a positive thread ID

Validated by:

* `Render_WithPositiveThreadId_ShouldReturnThreadIdAsString`

Verifies that a normal positive thread identifier is rendered without modification.

## Renderer should correctly render a large thread ID

Validated by:

* `Render_WithLargeThreadId_ShouldReturnThreadIdAsString`

Verifies that large valid thread identifiers are converted correctly without truncation or formatting changes.

# Rendering Consistency Tests

## Renderer should produce deterministic output for the same thread ID

Validated by:

* `Render_WithSameThreadId_ShouldReturnSameResult`

Ensures repeated rendering of messages with the same thread ID produces identical output.

## Renderer should render different thread IDs independently

Validated by:

* `Render_WithDifferentThreadIds_ShouldReturnCorrespondingValues`

Verifies that the renderer does not retain or reuse a previously rendered thread ID.

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

These tests validate only the public behavior of **ThreadToken**.

The following responsibilities are intentionally tested separately within other components:

* `LogMessage` construction and validation.
* Thread creation and lifecycle.
* Assignment/capture of the thread ID.
* Thread-local or asynchronous execution behavior.
* Token discovery and token registration.
* Token parsing from format strings.
* Combination of multiple token renderers.
* Overall formatter output composition.
* Concurrent/thread-safety behavior of the logging framework.

# Coverage Summary

| Area                  | Covered |
| --------------------- | :-----: |
| Token identification  |    ✅    |
| Thread ID rendering   |    ✅    |
| String conversion     |    ✅    |
| Zero thread ID        |    ✅    |
| Positive thread ID    |    ✅    |
| Large thread ID       |    ✅    |
| Rendering consistency |    ✅    |
| Null input validation |    ✅    |
| Exception handling    |    ✅    |
| Parameter validation  |    ✅    |
| Stateless behavior    |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>