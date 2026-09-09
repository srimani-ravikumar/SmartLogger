# LayoutFactory Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                 |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `LayoutFactory` class, validating layout selection, predefined patterns, custom patterns, token composition, and unsupported configuration handling. |

# Objective

Validate that **LayoutFactory** correctly creates and composes `ILogLayoutStrategy` instances by:

* Creating the correct layout strategy for each supported layout type.
* Applying the predefined Simple layout pattern.
* Applying the predefined Detailed layout pattern.
* Applying user-defined Custom patterns.
* Registering all supported token renderers.
* Resolving configured tokens through the resulting layout.
* Handling empty and complex custom patterns.
* Rejecting unsupported layout types.
* Handling invalid configuration appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a fresh `AppenderConfiguration`.
* Dependency Composition:

  * `LayoutFactory` directly creates `TokenRegistry`, token renderer implementations, and `PatternLayout`.
  * These dependencies are treated as real collaborators because they are statically/compositionally created inside the factory.
* Test Strategy:

  * Factory selection is validated through the returned strategy type.
  * Pattern composition is validated through `Render()` output.
  * Token-specific behavior remains the responsibility of individual token strategy tests.

# Layout Selection Tests

## Factory should create a PatternLayout for Simple layout

Validated by:

* `Create_WithSimpleLayout_ShouldReturnPatternLayout`

Verifies that `LogMessageLayoutType.Simple` produces a `PatternLayout` instance.

## Factory should create a PatternLayout for Detailed layout

Validated by:

* `Create_WithDetailedLayout_ShouldReturnPatternLayout`

Verifies that `LogMessageLayoutType.Detailed` produces a `PatternLayout` instance.

## Factory should create a PatternLayout for Custom layout

Validated by:

* `Create_WithCustomLayout_ShouldReturnPatternLayout`

Verifies that `LogMessageLayoutType.Custom` produces a `PatternLayout` instance.

# Simple Layout Tests

## Factory should apply the predefined Simple layout pattern

Validated by:

* `Create_WithSimpleLayout_ShouldUseExpectedPattern`

Expected pattern:

```text
[%TIMESTAMP] [%LEVEL] %SOURCE - %MESSAGE
```

## Simple layout should render timestamp, level, source, and message

Validated by:

* `Create_WithSimpleLayout_ShouldRenderConfiguredTokens`

Verifies that the resulting layout resolves the expected tokens from a `LogMessage`.

## Simple layout should not include Detailed-only tokens

Validated by:

* `Create_WithSimpleLayout_ShouldNotRenderThreadOrCorrelationTokens`

The Simple pattern intentionally contains no:

```text
%THREAD
%CORRELATION
```

tokens.

# Detailed Layout Tests

## Factory should apply the predefined Detailed layout pattern

Validated by:

* `Create_WithDetailedLayout_ShouldUseExpectedPattern`

Expected pattern:

```text
[%TIMESTAMP] [%LEVEL] [T#%THREAD] [%CORRELATION] %SOURCE - %MESSAGE
```

## Detailed layout should render all configured context tokens

Validated by:

* `Create_WithDetailedLayout_ShouldRenderAllConfiguredTokens`

Verifies rendering of:

* Timestamp
* Log level
* Thread ID
* Correlation ID
* Source
* Message

## Detailed layout should include thread information

Validated by:

* `Create_WithDetailedLayout_ShouldRenderThreadToken`

Verifies that `%THREAD` is resolved using the `LogMessage.ThreadId`.

## Detailed layout should include correlation information

Validated by:

* `Create_WithDetailedLayout_ShouldRenderCorrelationToken`

Verifies that `%CORRELATION` is resolved using the message's correlation ID.

# Custom Layout Tests

## Factory should use the configured custom pattern

Validated by:

* `Create_WithCustomLayout_ShouldUseConfiguredPattern`

Verifies that `FormatterConfiguration.Pattern` is passed to the resulting layout.

## Custom layout should resolve configured tokens

Validated by:

* `Create_WithCustomLayout_ShouldRenderConfiguredTokens`

For example:

```text
[%LEVEL] %SOURCE: %MESSAGE
```

should render using the corresponding message properties.

## Custom layout should support a single token

Validated by:

* `Create_WithCustomLayout_WithSingleToken_ShouldRenderToken`

## Custom layout should support multiple occurrences of the same token

Validated by:

* `Create_WithCustomLayout_WithRepeatedToken_ShouldRenderEachOccurrence`

For example:

```text
%LEVEL - %LEVEL - %MESSAGE
```

should resolve both `%LEVEL` occurrences.

## Custom layout should support literal text

Validated by:

* `Create_WithCustomLayout_WithLiteralText_ShouldPreserveLiteralText`

## Custom layout should support an empty pattern

Validated by:

* `Create_WithCustomLayout_WithEmptyPattern_ShouldReturnEmptyOutput`

The current implementation passes the configured pattern directly to `PatternLayout`.

# Token Registration Tests

## Factory should register all supported token renderers

Validated by:

* `Create_ShouldRegisterAllSupportedTokens`

The factory currently registers:

```text
TIMESTAMP
LEVEL
MESSAGE
SOURCE
THREAD
CORRELATION
```

The resulting layout should be capable of resolving all supported tokens.

## Factory should make token renderers available to Simple layout

Validated by:

* `Create_WithSimpleLayout_ShouldResolveRegisteredTokens`

## Factory should make token renderers available to Detailed layout

Validated by:

* `Create_WithDetailedLayout_ShouldResolveRegisteredTokens`

## Factory should make token renderers available to Custom layout

Validated by:

* `Create_WithCustomLayout_ShouldResolveRegisteredTokens`

> **Note:** These tests validate that the factory composes the token registry correctly. The individual rendering rules of each token belong to the corresponding token strategy tests.

# Layout Instance Tests

## Factory should create a new layout instance for each request

Validated by:

* `Create_CalledMultipleTimes_ShouldReturnDifferentLayoutInstances`

Verifies that `LayoutFactory` does not cache layout instances.

## Factory should create layouts independently for different configurations

Validated by:

* `Create_WithDifferentLayoutTypes_ShouldCreateIndependentLayouts`

A layout created with one configuration must not affect a layout created from another configuration.

# Unsupported Layout Tests

## Factory should reject an unsupported layout type

Validated by:

* `Create_WithUnsupportedLayoutType_ShouldThrowNotSupportedException`

Uses an undefined `LogMessageLayoutType` enum value.

## Factory should include the unsupported layout type in the exception message

Validated by:

* `Create_WithUnsupportedLayoutType_ShouldIncludeLayoutTypeInExceptionMessage`

Verifies that the exception identifies the configured unsupported layout type.

# Null Configuration Tests

## Factory should reject a null appender configuration according to current behavior

Validated by:

* `Create_WithNullAppenderConfiguration_ShouldThrowNullReferenceException`

The current implementation accesses:

```csharp
appenderConfig.Formatter.LayoutType
```

without explicit null validation.

> **Note:** This reflects current implementation behavior. A future enhancement could explicitly throw `ArgumentNullException`.

## Factory should reject a null Formatter configuration according to current behavior

Validated by:

* `Create_WithNullFormatterConfiguration_ShouldThrowNullReferenceException`

If `AppenderConfiguration.Formatter` is explicitly assigned `null`, the factory cannot resolve the layout configuration.

# Custom Pattern Boundary Tests

## Factory should preserve whitespace in a custom pattern

Validated by:

* `Create_WithCustomLayout_WithWhitespacePattern_ShouldPreserveWhitespace`

## Factory should support special characters in a custom pattern

Validated by:

* `Create_WithCustomLayout_WithSpecialCharacters_ShouldPreserveLiteralCharacters`

## Factory should support Unicode characters in a custom pattern

Validated by:

* `Create_WithCustomLayout_WithUnicodePattern_ShouldPreserveUnicodeCharacters`

> **Note:** These tests verify that the configured pattern reaches `PatternLayout`. Pattern parsing and token interpretation remain `PatternLayout` responsibilities.

# Test Scope

These tests validate only the composition responsibility of **LayoutFactory**.

The following responsibilities are intentionally tested separately within **PatternLayout**:

* Pattern parsing.
* Token detection.
* Token replacement.
* Unknown token behavior.
* Literal text handling.
* Multiple token occurrences.
* Final string construction.

The following responsibilities are intentionally tested separately within individual token strategies:

* Timestamp rendering.
* Log level rendering.
* Message rendering.
* Source rendering.
* Thread ID rendering.
* Correlation ID rendering.

The following responsibilities are intentionally excluded from **LayoutFactory**:

* Log message creation.
* Token implementation details.
* Token value generation.
* Formatter selection.
* JSON serialization.
* XML serialization.
* Appender configuration loading.
* Layout caching.
* Thread synchronization.

# Coverage Summary

| Area                          | Covered |
| ----------------------------- | :-----: |
| Simple layout selection       |    ✅    |
| Detailed layout selection     |    ✅    |
| Custom layout selection       |    ✅    |
| Simple layout pattern         |    ✅    |
| Detailed layout pattern       |    ✅    |
| Custom pattern propagation    |    ✅    |
| Token registry composition    |    ✅    |
| Timestamp token composition   |    ✅    |
| Level token composition       |    ✅    |
| Message token composition     |    ✅    |
| Source token composition      |    ✅    |
| Thread token composition      |    ✅    |
| Correlation token composition |    ✅    |
| Repeated custom tokens        |    ✅    |
| Empty custom pattern          |    ✅    |
| Custom pattern boundaries     |    ✅    |
| Layout instance independence  |    ✅    |
| Unsupported layout handling   |    ✅    |
| Null configuration handling   |    ✅    |
| Pattern parsing               |    ❌    |
| Token implementation details  |    ❌    |
| JSON serialization            |    ❌    |
| XML serialization             |    ❌    |
| Thread safety                 |    ❌    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>