# JsonConfigurationProvider Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                   |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-08 | Srimani | Initial Draft | Defined concise unit test coverage for JSON configuration deserialization and JSON-specific parsing behavior. |

# Objective

Validate that **JsonConfigurationProvider** correctly deserializes SmartLogger JSON configuration by:

* Loading valid JSON configuration.
* Supporting case-insensitive property names.
* Supporting JSON comments.
* Supporting trailing commas.
* Deserializing string-based enum values.
* Rejecting integer enum values.
* Handling malformed JSON.
* Handling a JSON document that produces a null configuration.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * Each test uses a unique temporary JSON configuration file.
  * Temporary files are removed after each test.
  * Auto-reload behavior is intentionally excluded because it belongs to `FileConfigurationProviderBase`.

# JSON Deserialization Tests

## Framework should deserialize a valid JSON configuration

Validated by:

* `Load_WithValidJson_ShouldReturnConfiguration`

Verifies that a valid JSON document is deserialized into `LogConfigurationHolder` with the expected configuration values.

## Framework should deserialize JSON property names case-insensitively

Validated by:

* `Load_WithCaseInsensitiveProperties_ShouldDeserializeConfiguration`

Verifies that property names with different casing are accepted.

For example:

```text
"rootloglevel"
"ROOTLOGLEVEL"
"RootLogLevel"
```

should all map to `RootLogLevel`.

## Framework should ignore JSON comments

Validated by:

* `Load_WithJsonComments_ShouldDeserializeConfiguration`

Verifies support for both supported JSON comment forms:

```text
// comment
```

and

```text
/* comment */
```

## Framework should allow trailing commas

Validated by:

* `Load_WithTrailingCommas_ShouldDeserializeConfiguration`

Verifies that trailing commas in JSON objects/arrays are accepted.

# Enum Deserialization Tests

## Framework should deserialize valid string enum values

Validated by:

* `Load_WithStringEnumValues_ShouldDeserializeConfiguration`

Verifies that supported enum values such as:

```text
"INFO"
"WARNING"
"ERROR"
"FATAL"
```

are correctly converted to their corresponding enum values.

## Framework should reject integer enum values

Validated by:

* `Load_WithIntegerEnumValue_ShouldThrowJsonException`

The configured `JsonStringEnumConverter` explicitly sets:

```text
allowIntegerValues = false
```

Therefore numeric representations such as:

```json
{
    "RootLogLevel": 2
}
```

must be rejected.

# Invalid JSON Tests

## Framework should reject malformed JSON

Validated by:

* `Load_WithMalformedJson_ShouldThrowJsonException`

Verifies that syntactically invalid JSON does not produce a configuration.

## Framework should reject JSON that cannot be deserialized into the configuration model

Validated by:

* `Load_WithInvalidJsonStructure_ShouldThrowJsonException`

Verifies that incompatible JSON values/types are rejected rather than silently producing an incorrect configuration.

# Test Scope

These tests validate only JSON-specific behavior implemented by **JsonConfigurationProvider**.

The following are intentionally outside the scope:

* File path validation.
* Missing configuration files.
* Relative/absolute path resolution.
* Configuration validation rules.
* Automatic configuration reload.
* `FileSystemWatcher` behavior.
* Logger creation.
* Logger caching.
* Appender creation.
* Log formatting.
* JSON serialization.

Those responsibilities are covered by `FileConfigurationProviderBase`, `ConfigurationValidator`, and their respective components.

# Coverage Summary

| Area                        | Covered |
| --------------------------- | :-----: |
| Valid JSON deserialization  |    ✅    |
| Case-insensitive properties |    ✅    |
| JSON comments               |    ✅    |
| Trailing commas             |    ✅    |
| String enum deserialization |    ✅    |
| Integer enum rejection      |    ✅    |
| Malformed JSON              |    ✅    |
| Invalid JSON structure      |    ✅    |
| File path handling          |    ⬜    |
| Configuration validation    |    ⬜    |
| Automatic reload            |    ⬜    |
| FileSystemWatcher behavior  |    ⬜    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
