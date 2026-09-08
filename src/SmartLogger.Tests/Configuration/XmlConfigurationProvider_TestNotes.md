# XmlConfigurationProvider Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                        |
| ------- | ---------- | ------- | ------------- | -------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-08 | Srimani | Initial Draft | Defined concise unit test coverage for XML configuration deserialization and invalid XML handling. |

# Objective

Validate that **XmlConfigurationProvider** correctly loads SmartLogger configuration from XML by:

* Deserializing valid XML into `LogConfigurationHolder`.
* Preserving configured values during deserialization.
* Handling valid collections and nested configuration objects.
* Rejecting malformed XML.
* Rejecting XML that cannot be deserialized into the expected configuration type.
* Handling an empty or invalid deserialization result appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * Each test uses a unique temporary XML configuration file.
  * Temporary files are removed after each test.
  * Auto-reload and file-path behavior are intentionally excluded because they belong to `FileConfigurationProviderBase`.

# XML Deserialization Tests

## Framework should deserialize a valid XML configuration

Validated by:

* `Load_WithValidXml_ShouldReturnConfiguration`

Verifies that a valid XML document is successfully converted into `LogConfigurationHolder`.

## Framework should preserve configured scalar values

Validated by:

* `Load_WithConfiguredValues_ShouldPreserveConfiguration`

Verifies representative configuration values such as:

* `RootLogLevel`
* `EnableAsyncLoggingProcess`

are correctly populated after deserialization.

## Framework should deserialize nested and collection configuration

Validated by:

* `Load_WithAppendersAndOverrides_ShouldDeserializeConfiguration`

Verifies that XML deserialization correctly populates:

* Logger overrides.
* Appender configurations.
* Destination configuration.
* File configuration.
* Formatter configuration.

# Invalid XML Tests

## Framework should reject malformed XML

Validated by:

* `Load_WithMalformedXml_ShouldThrowInvalidOperationException`

Verifies that syntactically invalid XML is rejected by `XmlSerializer`.

## Framework should reject XML with incompatible configuration values

Validated by:

* `Load_WithInvalidXmlValue_ShouldThrowInvalidOperationException`

Verifies that XML values that cannot be converted to the target configuration property types are rejected.

# Test Scope

These tests validate only XML-specific behavior implemented by **XmlConfigurationProvider**.

The following are intentionally outside the scope:

* File path validation.
* Relative/absolute path resolution.
* Missing configuration files.
* Configuration validation rules.
* Automatic configuration reload.
* `FileSystemWatcher` behavior.
* JSON configuration behavior.
* Logger creation.
* Logger caching.
* Appender execution.
* XML serialization.

Those responsibilities are covered by `FileConfigurationProviderBase`, `ConfigurationValidator`, and their respective components.

# Coverage Summary

| Area                        | Covered |
| --------------------------- | :-----: |
| Valid XML deserialization   |    ✅    |
| Scalar configuration values |    ✅    |
| Nested configuration        |    ✅    |
| Collection configuration    |    ✅    |
| Malformed XML               |    ✅    |
| Invalid XML values          |    ✅    |
| File path handling          |    ⬜    |
| Missing file handling       |    ⬜    |
| Configuration validation    |    ⬜    |
| Automatic reload            |    ⬜    |
| FileSystemWatcher behavior  |    ⬜    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
