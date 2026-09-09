# FormatterFactory Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                             |
| ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `FormatterFactory` class, validating formatter selection, layout composition, configuration propagation, and unsupported configuration handling. |

# Objective

Validate that **FormatterFactory** correctly acts as the composition point between appender configuration, layout strategies, and output formatter strategies by:

* Creating the correct formatter implementation for each supported output format.
* Creating and supplying the appropriate layout for plain-text formatting.
* Propagating JSON field configuration to the JSON formatter.
* Creating XML formatting without requiring a layout.
* Rejecting unsupported output formats.
* Handling invalid configuration appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Accessibility:

  * `FormatterFactory` is `internal`; the test project must have access through `InternalsVisibleTo`.
* Dependency Composition:

  * `FormatterFactory` directly invokes the static `LayoutFactory.Create()`.
  * `LayoutFactory` is therefore treated as a real collaborator during these tests rather than a mockable dependency.
* Test Isolation:

  * Each test creates a fresh `AppenderConfiguration` to avoid configuration state leaking between tests.

# Formatter Selection Tests

## Factory should create a PlainText formatter for PlainText output

Validated by:

* `Create_WithPlainTextFormat_ShouldReturnPlainTextFormatter`

Verifies that configuring `LogOutputFormat.PlainText` results in a `PlainTextFormatter` instance.

## Factory should create a JSON formatter for Json output

Validated by:

* `Create_WithJsonFormat_ShouldReturnJsonFormatter`

Verifies that configuring `LogOutputFormat.Json` results in a `JsonFormatter` instance.

## Factory should create an XML formatter for Xml output

Validated by:

* `Create_WithXmlFormat_ShouldReturnXmlFormatter`

Verifies that configuring `LogOutputFormat.Xml` results in an `XmlFormatter` instance.

# Layout Composition Tests

## Factory should create a layout when creating a PlainText formatter

Validated by:

* `Create_WithPlainTextFormat_ShouldCreateFormatterWithLayout`

Verifies that the PlainText formatter is composed with a layout created from the supplied appender configuration.

## Factory should support Simple layout configuration for PlainText output

Validated by:

* `Create_WithPlainTextAndSimpleLayout_ShouldCreateFormatter`

The resulting formatter should be successfully created using the `Simple` layout configuration.

## Factory should support Detailed layout configuration for PlainText output

Validated by:

* `Create_WithPlainTextAndDetailedLayout_ShouldCreateFormatter`

The resulting formatter should be successfully created using the `Detailed` layout configuration.

## Factory should support Custom layout configuration for PlainText output

Validated by:

* `Create_WithPlainTextAndCustomLayout_ShouldCreateFormatter`

The resulting formatter should be successfully created using the configured custom pattern.

> **Note:** Token resolution and actual layout rendering are responsibilities of `LayoutFactory` and `PatternLayout` and are tested separately.

# JSON Configuration Composition Tests

## Factory should create a JSON formatter using configured JSON fields

Validated by:

* `Create_WithJsonFormatAndIncludedFields_ShouldCreateFormatter`

Verifies that the JSON formatter receives the configured `IncludedJsonFields`.

## Factory should create a JSON formatter using configured field mappings

Validated by:

* `Create_WithJsonFormatAndFieldMappings_ShouldCreateFormatter`

Verifies that the JSON formatter receives the configured `JsonFieldMappings`.

## Factory should support JSON formatting with default configuration

Validated by:

* `Create_WithJsonFormatAndDefaultConfiguration_ShouldReturnJsonFormatter`

Verifies that the default `FormatterConfiguration` can be used to create a JSON formatter successfully.

> **Note:** Actual field filtering, mapping behavior, JSON serialization, and duplicate mapping validation are responsibilities of `JsonFormatter` and are intentionally excluded from this factory test plan.

# XML Composition Tests

## Factory should create an XML formatter without requiring layout configuration

Validated by:

* `Create_WithXmlFormat_ShouldReturnXmlFormatterWithoutLayoutDependency`

Verifies that XML formatter creation follows the XML branch and does not depend on the layout strategy for its formatting behavior.

> **Note:** `XmlFormatter` currently acts as a placeholder implementation. Its actual XML serialization behavior belongs to the `XmlFormatter` test plan.

# Input Validation Tests

## Factory should reject a null appender configuration

Validated by:

* `Create_WithNullAppenderConfiguration_ShouldThrowNullReferenceException`

The current implementation passes the configuration directly to `LayoutFactory.Create()`, which accesses the configuration before the formatter selection can occur.

> **Note:** This reflects the current implementation. A future enhancement could explicitly validate `appenderConfig` and throw `ArgumentNullException` for clearer API behavior.

## Factory should handle an explicitly null Formatter configuration according to current implementation

Validated by:

* `Create_WithNullFormatterConfiguration_ShouldThrowNullReferenceException`

If `AppenderConfiguration.Formatter` is explicitly assigned `null`, the current implementation cannot resolve either layout or output format.

> **Note:** `FormatterConfiguration` is initialized by default, so this scenario requires explicitly assigning `null`.

# Unsupported Configuration Tests

## Factory should reject an unsupported output format

Validated by:

* `Create_WithUnsupportedOutputFormat_ShouldThrowNotSupportedException`

Uses an undefined `LogOutputFormat` enum value and verifies that the factory throws `NotSupportedException`.

## Factory should report the unsupported output format in the exception

Validated by:

* `Create_WithUnsupportedOutputFormat_ShouldIncludeFormatInExceptionMessage`

Verifies that the exception message identifies the configured unsupported output format.

> **Important:** A valid layout configuration must be supplied for this test because `LayoutFactory.Create()` executes before `FormatterFactory` evaluates `OutputFormat`.

# Layout Validation Propagation Tests

## Factory should propagate unsupported layout configuration

Validated by:

* `Create_WithUnsupportedLayoutType_ShouldThrowNotSupportedException`

Uses an undefined `LogMessageLayoutType` value and verifies that the exception produced by `LayoutFactory` propagates through `FormatterFactory`.

## Factory should not create a formatter when layout creation fails

Validated by:

* `Create_WithUnsupportedLayoutType_ShouldNotReturnFormatter`

Verifies that formatter construction does not proceed when the required layout cannot be created.

> **Note:** This behavior is a consequence of the current composition order:
>
> ```text
> LayoutFactory.Create()
>        ↓
> OutputFormat switch
>        ↓
> Formatter creation
> ```
>
> Layout validation therefore occurs before output-format validation.

# Configuration Independence Tests

## Factory should create separate formatter instances for separate Create calls

Validated by:

* `Create_CalledMultipleTimes_ShouldReturnDifferentFormatterInstances`

Verifies that `FormatterFactory` does not maintain or cache formatter instances.

## Factory should create formatters according to the current configuration

Validated by:

* `Create_WithDifferentOutputFormats_ShouldCreateCorrespondingFormatterTypes`

Verifies that changing the configured `OutputFormat` results in the corresponding formatter strategy rather than reusing a previous formatter.

# Test Scope

These tests validate only the public behavior and composition responsibility of **FormatterFactory**.

The following responsibilities are intentionally tested separately within **LayoutFactory**:

* Layout selection
* Simple layout pattern
* Detailed layout pattern
* Custom pattern handling
* Token registry creation
* Token renderer registration
* Unsupported layout handling
* Token rendering

The following responsibilities are intentionally tested separately within **PlainTextFormatter**:

* Delegation to `ILogLayoutStrategy`
* Formatted output returned from the layout
* Layout interaction

The following responsibilities are intentionally tested separately within **JsonFormatter**:

* JSON serialization
* Field filtering
* JSON field mappings
* Default fields
* Empty/null field configuration
* Duplicate mappings
* JSON serialization options

The following responsibilities are intentionally tested separately within **XmlFormatter**:

* Current raw-message behavior
* Null message handling
* Future XML serialization behavior

The following responsibilities are intentionally excluded from **FormatterFactory**:

* Log message token resolution
* Actual formatted output correctness
* JSON serialization correctness
* XML serialization correctness
* Formatter caching
* Thread safety
* Formatter lifecycle management

# Coverage Summary

| Area                            | Covered |
| ------------------------------- | :-----: |
| PlainText formatter selection   |    ✅    |
| JSON formatter selection        |    ✅    |
| XML formatter selection         |    ✅    |
| Layout composition              |    ✅    |
| Simple layout configuration     |    ✅    |
| Detailed layout configuration   |    ✅    |
| Custom layout configuration     |    ✅    |
| JSON configuration propagation  |    ✅    |
| XML composition                 |    ✅    |
| Null configuration handling     |    ✅    |
| Unsupported output format       |    ✅    |
| Unsupported layout propagation  |    ✅    |
| Formatter instance independence |    ✅    |
| JSON serialization behavior     |    ❌    |
| XML serialization behavior      |    ❌    |
| Token rendering                 |    ❌    |
| Layout implementation details   |    ❌    |
| Formatter caching               |    ❌    |
| Thread safety                   |    ❌    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>