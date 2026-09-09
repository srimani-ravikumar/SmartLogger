# PlainTextFormatter Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                            |
| ------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `PlainTextFormatter` class, validating layout delegation, formatted output propagation, input handling, and formatter behavior. |

# Objective

Validate that **PlainTextFormatter** correctly acts as a thin output-formatting adapter by:

* Delegating rendering to the configured `ILogLayoutStrategy`.
* Passing the exact `LogMessage` instance to the layout.
* Returning the exact result produced by the layout.
* Supporting empty and complex layout results.
* Propagating exceptions from the layout strategy.
* Guarding against invalid layout configuration appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * A mocked `ILogLayoutStrategy` is supplied to each formatter instance.
  * Each test creates an independent `LogMessage` instance.
* Dependency Isolation:

  * `ILogLayoutStrategy` is mocked because layout rendering is outside the responsibility of `PlainTextFormatter`.

# Constructor Tests

## Formatter should accept a valid layout strategy

Validated by:

* `Constructor_WithValidLayout_ShouldCreateFormatter`

Verifies that the formatter can be constructed with a valid `ILogLayoutStrategy`.

## Formatter should reject a null layout strategy

Validated by:

* `Constructor_WithNullLayout_ShouldThrowArgumentNullException`

> **Note:** The current implementation does not explicitly validate `_layout`. If a null layout is supplied, construction currently succeeds and the failure occurs when `Format()` is called.

Therefore, this test should only be introduced if the constructor is updated to explicitly guard against null dependencies.

# Layout Delegation Tests

## Formatter should delegate formatting to the configured layout

Validated by:

* `Format_WithValidMessage_ShouldCallLayoutRender`

Verifies that `Format()` invokes:

```text
ILogLayoutStrategy.Render(message)
```

exactly once.

## Formatter should pass the same LogMessage instance to the layout

Validated by:

* `Format_WithValidMessage_ShouldPassSameMessageToLayout`

Ensures that the formatter does not create, clone, modify, or replace the supplied `LogMessage`.

## Formatter should invoke the layout exactly once per format request

Validated by:

* `Format_CalledOnce_ShouldCallLayoutRenderExactlyOnce`

## Formatter should invoke the layout independently for each format request

Validated by:

* `Format_CalledMultipleTimes_ShouldCallLayoutForEachRequest`

Verifies that the formatter does not cache the rendered output.

# Output Propagation Tests

## Formatter should return the exact output produced by the layout

Validated by:

* `Format_WhenLayoutReturnsFormattedText_ShouldReturnSameText`

Verifies that the formatter does not modify the layout result.

## Formatter should preserve an empty layout result

Validated by:

* `Format_WhenLayoutReturnsEmptyString_ShouldReturnEmptyString`

## Formatter should preserve whitespace in the layout result

Validated by:

* `Format_WhenLayoutReturnsWhitespace_ShouldPreserveWhitespace`

Verifies that the formatter does not trim or normalize the output.

## Formatter should preserve complex formatted output

Validated by:

* `Format_WhenLayoutReturnsComplexText_ShouldReturnExactOutput`

The formatter should return the layout output exactly, including:

* Tokens
* Special characters
* Newlines
* Tabs
* Unicode characters
* Leading/trailing whitespace

# Message Handling Tests

## Formatter should support a valid LogMessage

Validated by:

* `Format_WithValidMessage_ShouldReturnFormattedOutput`

Verifies the normal formatting pipeline:

```text
LogMessage
    ↓
PlainTextFormatter
    ↓
ILogLayoutStrategy.Render()
    ↓
Formatted string
```

## Formatter should pass message content without interpreting it

Validated by:

* `Format_WithSpecialCharactersInMessage_ShouldDelegateWithoutModification`

Special characters in the message are the responsibility of the layout strategy and should not be processed by `PlainTextFormatter`.

## Formatter should pass Unicode message content without modification

Validated by:

* `Format_WithUnicodeMessage_ShouldDelegateWithoutModification`

# Exception Propagation Tests

## Formatter should propagate exceptions thrown by the layout

Validated by:

* `Format_WhenLayoutThrowsException_ShouldPropagateException`

The formatter should not catch, replace, or suppress exceptions originating from `ILogLayoutStrategy.Render()`.

## Formatter should preserve the original layout exception

Validated by:

* `Format_WhenLayoutThrowsException_ShouldPreserveOriginalException`

Verifies that the same exception type and error information are propagated to the caller.

## Formatter should propagate different layout failure types

Validated by:

* `Format_WhenLayoutThrowsInvalidOperationException_ShouldPropagateException`
* `Format_WhenLayoutThrowsArgumentException_ShouldPropagateException`

The formatter should remain transparent to the type of exception generated by the layout.

# Null Input Tests

## Formatter should propagate the null message to the layout

Validated by:

* `Format_WithNullMessage_ShouldPassNullToLayout`

Because `PlainTextFormatter` performs no explicit null validation, it should pass the supplied value directly to the layout.

> **Note:** The expected behavior depends on the mocked layout. If the layout accepts the null value, `PlainTextFormatter` itself should not fail before invoking it.

## Formatter should fail when the configured layout is null

Validated by:

* `Format_WithNullLayout_ShouldThrowNullReferenceException`

Reflects the current implementation:

```csharp
_layout.Render(message)
```

If `_layout` is null, the call results in `NullReferenceException`.

> **Note:** This is current implementation behavior. Explicit constructor validation with `ArgumentNullException` would provide a clearer failure contract.

# Test Scope

These tests validate only the behavior of **PlainTextFormatter**.

The following responsibilities are intentionally tested separately within **LayoutFactory**:

* Layout selection.
* Simple layout creation.
* Detailed layout creation.
* Custom layout creation.
* Token registration.
* Unsupported layout handling.

The following responsibilities are intentionally tested separately within **PatternLayout**:

* Pattern parsing.
* Token resolution.
* Token replacement.
* Final string construction.
* Unknown token handling.

The following responsibilities are intentionally tested separately within individual token strategies:

* Timestamp rendering.
* Log level rendering.
* Message rendering.
* Source rendering.
* Thread rendering.
* Correlation rendering.

The following responsibilities are intentionally excluded from **PlainTextFormatter**:

* JSON serialization.
* XML serialization.
* Log level filtering.
* Appender selection.
* Log destination management.
* Configuration loading.
* Layout creation.
* Token resolution.
* Output caching.
* Thread synchronization.

# Coverage Summary

| Area                          | Covered |
| ----------------------------- | :-----: |
| Constructor with valid layout |    ✅    |
| Null layout handling          |    ✅    |
| Layout delegation             |    ✅    |
| Message propagation           |    ✅    |
| Single Render invocation      |    ✅    |
| Multiple format requests      |    ✅    |
| Exact output propagation      |    ✅    |
| Empty output                  |    ✅    |
| Whitespace preservation       |    ✅    |
| Complex output preservation   |    ✅    |
| Special-character handling    |    ✅    |
| Unicode handling              |    ✅    |
| Null message handling         |    ✅    |
| Layout exception propagation  |    ✅    |
| Exception preservation        |    ✅    |
| Layout creation               |    ❌    |
| Token rendering               |    ❌    |
| Pattern parsing               |    ❌    |
| JSON serialization            |    ❌    |
| XML serialization             |    ❌    |
| Output caching                |    ❌    |
| Thread synchronization        |    ❌    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>