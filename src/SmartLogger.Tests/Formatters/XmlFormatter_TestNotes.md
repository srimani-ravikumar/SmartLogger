# XmlFormatter Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                |
| ------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `XmlFormatter` class, validating raw message propagation, content preservation, and current null handling behavior. |

# Objective

Validate that **XmlFormatter** correctly implements the current placeholder XML formatting behavior by:

* Returning the `LogMessage.Message` value.
* Preserving the message exactly without modification.
* Supporting empty message content when a valid `LogMessage` can contain it.
* Preserving special characters and Unicode content.
* Handling null message input according to the current implementation.
* Ensuring the formatter does not perform unintended XML serialization or transformation.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a fresh `XmlFormatter` and `LogMessage`.
* Dependency Isolation:

  * No external dependencies or collaborators are required.
  * `XmlFormatter` directly reads `LogMessage.Message`.

# Basic Formatting Tests

## Formatter should return the log message content

Validated by:

* `Format_WithValidMessage_ShouldReturnMessageContent`

Verifies that `Format()` returns the exact value stored in `LogMessage.Message`.

## Formatter should return only the message content

Validated by:

* `Format_WithValidMessage_ShouldNotIncludeAdditionalFields`

Verifies that the current implementation does not include timestamp, level, source, thread, correlation ID, or any XML wrapper.

## Formatter should preserve the exact message value

Validated by:

* `Format_WithValidMessage_ShouldPreserveExactContent`

The formatter should not trim, normalize, prefix, suffix, or otherwise modify the message.

# Content Preservation Tests

## Formatter should preserve whitespace

Validated by:

* `Format_WithWhitespaceMessage_ShouldPreserveWhitespace`

Verifies that leading, trailing, and internal whitespace remains unchanged.

## Formatter should preserve newlines and tabs

Validated by:

* `Format_WithMultilineMessage_ShouldPreserveLineBreaks`

Verifies that newline and tab characters are returned unchanged.

## Formatter should preserve special characters

Validated by:

* `Format_WithSpecialCharacters_ShouldReturnCharactersUnchanged`

Verifies that characters such as:

```text
<
>
&
"
'
\
```

are returned exactly as supplied.

> **Note:** The current implementation does not XML-escape the message because it does not perform XML serialization.

## Formatter should preserve Unicode content

Validated by:

* `Format_WithUnicodeMessage_ShouldPreserveUnicodeContent`

Verifies that Unicode characters are returned without transformation or loss.

# Boundary Tests

## Formatter should return an empty message when Message is empty

Validated by:

* `Format_WithEmptyMessage_ShouldReturnEmptyString`

> **Note:** `LogMessage.Builder.Build()` currently rejects an empty or whitespace-only message. This test therefore requires constructing a valid `LogMessage` through whatever test-access mechanism is appropriate, or should be omitted if the production API intentionally guarantees non-empty messages.

## Formatter should preserve a whitespace-only message when supplied

Validated by:

* `Format_WithWhitespaceOnlyMessage_ShouldPreserveWhitespace`

> **Note:** The same `LogMessage.Builder` validation currently prevents creating such a message through the normal public builder API. This test is relevant to `XmlFormatter`'s direct contract but may not be executable without bypassing `Builder.Build()`.

# Null Input Tests

## Formatter should throw when the LogMessage is null

Validated by:

* `Format_WithNullMessage_ShouldThrowNullReferenceException`

The current implementation directly accesses:

```csharp
message.Message
```

Therefore, a null `LogMessage` results in `NullReferenceException`.

> **Note:** A future enhancement could explicitly validate the argument and throw `ArgumentNullException`.

## Formatter should not throw when Message content is valid

Validated by:

* `Format_WithValidMessage_ShouldNotThrow`

Confirms the normal formatter path completes successfully for a valid `LogMessage`.

# Current XML Behavior Tests

## Formatter should not perform XML serialization

Validated by:

* `Format_WithXmlSensitiveMessage_ShouldReturnRawMessage`

Verifies that a message such as:

```text
<message>Hello & welcome</message>
```

is returned exactly as supplied rather than being escaped or serialized.

## Formatter should not add an XML document or element wrapper

Validated by:

* `Format_ShouldNotAddXmlWrapper`

The current output should remain the raw message rather than becoming something similar to:

```xml
<log>...</log>
```

> **Note:** This test documents the current placeholder behavior and should be revised when proper XML serialization is implemented.

# Test Scope

These tests validate only the current behavior of **XmlFormatter**.

The following responsibilities are intentionally excluded because XML serialization is not currently implemented:

* XML document creation.
* XML element generation.
* XML attribute generation.
* XML field selection.
* XML field naming.
* XML escaping.
* XML schema validation.
* XML namespace handling.
* Structured `LogMessage` serialization.

The following responsibilities are intentionally tested separately within **LogMessage**:

* Message validation.
* Required log level validation.
* Default property values.
* Builder behavior.
* Log message construction.

The following responsibilities are intentionally tested separately within **FormatterFactory**:

* Selecting `XmlFormatter` for `LogOutputFormat.Xml`.
* Formatter construction based on configuration.

# Future XML Serialization Test Scope

When the TODO implementation is replaced with actual XML serialization, the test plan should be extended to cover:

* Valid XML document generation.
* Serialization of timestamp.
* Serialization of log level.
* Serialization of message.
* Serialization of source.
* Serialization of thread ID.
* Serialization of correlation ID.
* XML escaping.
* Null field representation.
* XML element naming.
* Invalid XML-sensitive content.
* XML structure validation.

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Basic message formatting       |    ✅    |
| Exact message preservation     |    ✅    |
| Raw message behavior           |    ✅    |
| Empty message behavior         |    ⚠️   |
| Whitespace preservation        |    ⚠️   |
| Newline/tab preservation       |    ✅    |
| Special-character preservation |    ✅    |
| Unicode preservation           |    ✅    |
| Null LogMessage handling       |    ✅    |
| No XML serialization           |    ✅    |
| No XML wrapper                 |    ✅    |
| Timestamp serialization        |    ❌    |
| Log level serialization        |    ❌    |
| Structured XML generation      |    ❌    |
| XML escaping                   |    ❌    |
| XML schema validation          |    ❌    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>