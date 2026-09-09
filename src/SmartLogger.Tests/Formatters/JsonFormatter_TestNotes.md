# JsonFormatter Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                               |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-10 | Srimani | Initial Draft | Defined the unit test coverage for the `JsonFormatter` class, validating JSON serialization, field filtering, field mapping, null handling, and formatting configuration. |

# Objective

Validate that **JsonFormatter** correctly converts `LogMessage` instances into JSON output by:

* Serializing all supported log message fields.
* Filtering fields according to configuration.
* Mapping configured JSON field names.
* Preserving unmapped field names.
* Handling null and empty formatting configuration.
* Handling null message values.
* Producing valid JSON output.
* Supporting compact and pretty-printed JSON output.
* Rejecting invalid or conflicting field mappings appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a new `JsonFormatter` instance with independent field and mapping configuration.
* JSON Validation:

  * Serialized output should be parsed as JSON rather than validated through fragile string comparisons wherever possible.

# Basic Serialization Tests

## Formatter should serialize all supported log message fields by default

Validated by:

* `Format_WithDefaultConfiguration_ShouldSerializeAllFields`

Verifies that the JSON output contains:

* `timestamp`
* `level`
* `message`
* `source`
* `thread`
* `correlation`

## Formatter should serialize the log level as a string

Validated by:

* `Format_WithLogLevel_ShouldSerializeLevelAsString`

Verifies that `LogMessage.LogLevel` is represented using `ToString()` rather than its underlying numeric value.

## Formatter should preserve the log message content

Validated by:

* `Format_WithMessage_ShouldSerializeMessageCorrectly`

## Formatter should preserve the source information

Validated by:

* `Format_WithSource_ShouldSerializeSourceCorrectly`

## Formatter should serialize thread information

Validated by:

* `Format_WithThreadId_ShouldSerializeThreadCorrectly`

## Formatter should serialize correlation information

Validated by:

* `Format_WithCorrelationId_ShouldSerializeCorrelationCorrectly`

## Formatter should serialize timestamp correctly

Validated by:

* `Format_WithTimestamp_ShouldSerializeTimestampCorrectly`

# JSON Output Validity Tests

## Formatter should always produce valid JSON

Validated by:

* `Format_WithValidMessage_ShouldReturnValidJson`

The returned string should be successfully parsed as a JSON object.

## Formatter should return a JSON object

Validated by:

* `Format_WithValidMessage_ShouldReturnJsonObject`

Verifies that the root JSON representation is an object containing the configured fields.

## Formatter should correctly escape special characters

Validated by:

* `Format_WithSpecialCharacters_ShouldProduceValidJson`

Verifies correct JSON serialization when message or other string fields contain characters such as:

```text
"
\
\n
\t
```

## Formatter should preserve Unicode content

Validated by:

* `Format_WithUnicodeContent_ShouldPreserveUnicodeCharacters`

Verifies that Unicode characters in log fields remain correctly represented after serialization.

# Field Filtering Tests

## Formatter should include only configured fields

Validated by:

* `Format_WithIncludedFields_ShouldSerializeOnlyConfiguredFields`

Verifies that fields not present in `_fields` are excluded from the JSON output.

## Formatter should support filtering to a single field

Validated by:

* `Format_WithSingleIncludedField_ShouldSerializeOnlyThatField`

## Formatter should support filtering to multiple fields

Validated by:

* `Format_WithMultipleIncludedFields_ShouldSerializeConfiguredFields`

## Formatter should exclude all fields when configured with an unknown field

Validated by:

* `Format_WithUnknownIncludedField_ShouldReturnEmptyJsonObject`

An unknown field does not match any supported field and therefore produces:

```json
{}
```

## Formatter should treat an empty field list as no filtering

Validated by:

* `Format_WithEmptyIncludedFields_ShouldIncludeAllFields`

The constructor converts an empty list to `null`, disabling field filtering.

## Formatter should treat null field configuration as no filtering

Validated by:

* `Format_WithNullIncludedFields_ShouldIncludeAllFields`

## Formatter should handle duplicate included fields

Validated by:

* `Format_WithDuplicateIncludedFields_ShouldSerializeEachFieldOnce`

Because the configured fields are converted to a `HashSet`, duplicate field names must not result in duplicate JSON properties.

# Field Mapping Tests

## Formatter should map configured field names

Validated by:

* `Format_WithFieldMapping_ShouldUseMappedFieldName`

For example:

```text
timestamp → ts
```

should produce:

```json
{
    "ts": "..."
}
```

rather than:

```json
{
    "timestamp": "..."
}
```

## Formatter should support multiple field mappings

Validated by:

* `Format_WithMultipleFieldMappings_ShouldMapAllConfiguredFields`

## Formatter should preserve fields without mappings

Validated by:

* `Format_WithPartialFieldMapping_ShouldPreserveUnmappedFields`

Only configured source fields should be renamed.

## Formatter should support mapping together with field filtering

Validated by:

* `Format_WithFilteringAndMapping_ShouldFilterThenMapFields`

Verifies that filtering is performed against the original field name before the mapped output name is applied.

## Formatter should treat empty mapping configuration as no mapping

Validated by:

* `Format_WithEmptyMappings_ShouldUseOriginalFieldNames`

## Formatter should treat null mapping configuration as no mapping

Validated by:

* `Format_WithNullMappings_ShouldUseOriginalFieldNames`

# Mapping Validation Tests

## Formatter should reject duplicate source field mappings

Validated by:

* `Constructor_WithDuplicateSourceMappings_ShouldThrowArgumentException`

`ToDictionary()` does not permit multiple mappings with the same `SourceField`.

## Formatter should support mapping to the same target field

Validated by:

* `Constructor_WithMultipleSourcesMappedToSameTarget_ShouldCreateFormatter`

The constructor only requires source keys to be unique.

> **Note:** If multiple source fields map to the same target field, the later field written during `Format()` replaces the earlier dictionary value. This is current implementation behavior.

## Formatter should preserve explicitly mapped empty target names

Validated by:

* `Format_WithEmptyTargetField_ShouldUseEmptyJsonPropertyName`

The current implementation does not validate `TargetField`. An empty target therefore becomes an empty JSON property name.

> **Note:** This reflects current implementation behavior. Explicit validation may be considered as a future enhancement.

# Null Value Tests

## Formatter should serialize null message fields as JSON null

Validated by:

* `Format_WithNullOptionalFields_ShouldSerializeNullValues`

Because the dictionary stores `object?` values, null fields should be represented as:

```json
"field": null
```

rather than causing serialization failure.

## Formatter should handle a null Message value

Validated by:

* `Format_WithNullMessage_ShouldThrowNullReferenceException`

The current implementation accesses:

```csharp
message.Timestamp
message.LogLevel
message.Message
```

without validating `message`.

Therefore, a null `LogMessage` currently results in `NullReferenceException`.

> **Note:** A future enhancement could explicitly validate the argument and throw `ArgumentNullException`.

# Pretty Print Tests

## Formatter should produce compact JSON by default

Validated by:

* `Format_WithDefaultPrettyPrint_ShouldReturnCompactJson`

The constructor defaults:

```text
prettyPrint = false
```

Therefore, the output should not contain indentation/newline formatting introduced by `JsonSerializerOptions`.

## Formatter should produce indented JSON when pretty printing is enabled

Validated by:

* `Format_WithPrettyPrintEnabled_ShouldReturnIndentedJson`

Verifies that `WriteIndented` is enabled when `prettyPrint` is `true`.

## Pretty printing should not change serialized data

Validated by:

* `Format_WithPrettyPrintEnabled_ShouldPreserveJsonContent`

The semantic JSON object should remain identical regardless of indentation.

# Case Sensitivity Tests

## Formatter should treat field names as case-sensitive

Validated by:

* `Format_WithIncorrectFieldNameCasing_ShouldExcludeField`

`HashSet<string>` uses its default case-sensitive comparer.

Therefore:

```text
"Message"
```

does not match:

```text
"message"
```

## Formatter should treat mapping source fields as case-sensitive

Validated by:

* `Format_WithIncorrectMappingSourceCasing_ShouldNotApplyMapping`

# Configuration Independence Tests

## Formatter should not modify the supplied field configuration

Validated by:

* `Constructor_ShouldNotModifyIncludedFields`

The formatter creates a new `HashSet<string>` from the supplied list.

## Formatter should not modify the supplied mapping configuration

Validated by:

* `Constructor_ShouldNotModifyMappings`

The formatter creates a new `Dictionary<string, string>` through `ToDictionary()`.

## Formatter instances should maintain independent configuration

Validated by:

* `MultipleFormatterInstances_ShouldMaintainIndependentConfiguration`

Changing the configuration used to construct one formatter must not affect another formatter instance.

# Test Scope

These tests validate the behavior of **JsonFormatter**.

The following responsibilities are intentionally tested separately within **FormatterFactory**:

* Selecting `JsonFormatter` for `LogOutputFormat.Json`.
* Passing configured JSON fields to the formatter.
* Passing configured JSON field mappings to the formatter.
* Selecting the correct formatter implementation.

The following responsibilities are intentionally tested separately within **LogMessage**:

* Message construction.
* Default property values.
* Log level semantics.
* Timestamp generation.
* Thread and correlation context creation.

The following responsibilities are intentionally excluded from **JsonFormatter**:

* Layout/token rendering.
* Plain-text formatting.
* XML formatting.
* Appender selection.
* Log destination management.
* Log level filtering.
* Configuration loading.
* Formatter caching.
* Thread synchronization.

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Basic JSON serialization       |    ✅    |
| All supported fields           |    ✅    |
| Log level serialization        |    ✅    |
| Timestamp serialization        |    ✅    |
| Field filtering                |    ✅    |
| Single-field filtering         |    ✅    |
| Multi-field filtering          |    ✅    |
| Empty/null field configuration |    ✅    |
| Duplicate included fields      |    ✅    |
| Field name mapping             |    ✅    |
| Multiple field mappings        |    ✅    |
| Partial mapping                |    ✅    |
| Filtering + mapping            |    ✅    |
| Duplicate source mappings      |    ✅    |
| Null values                    |    ✅    |
| Null LogMessage handling       |    ✅    |
| JSON validity                  |    ✅    |
| Special-character escaping     |    ✅    |
| Unicode content                |    ✅    |
| Compact JSON                   |    ✅    |
| Pretty-printed JSON            |    ✅    |
| Case sensitivity               |    ✅    |
| Configuration independence     |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>