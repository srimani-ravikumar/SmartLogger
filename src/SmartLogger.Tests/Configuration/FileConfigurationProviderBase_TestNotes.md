# FileConfigurationProviderBase Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                  |
| ------- | ---------- | ------- | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-08 | Srimani | Initial Draft | Defined concise unit test coverage for file path resolution, configuration loading, validation, and automatic configuration reload behavior. |

# Objective

Validate that **FileConfigurationProviderBase** correctly provides common behavior for file-based configuration providers by:

* Resolving relative and absolute configuration paths.
* Rejecting invalid file paths.
* Loading and deserializing existing configuration files.
* Handling missing or invalid configuration data.
* Validating loaded configuration.
* Automatically reloading configuration when the watched file changes.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq** where required for `LoggerManager` integration.
* Target Framework: **.NET**
* Test Isolation:

  * Each test uses a unique temporary directory/file.
  * Temporary files and directories are removed during teardown.
  * Auto-reload tests must reset `LoggerManager` state before execution.

# Constructor and Path Resolution Tests

## Framework should reject null or whitespace configuration paths

Validated by:

* `Constructor_WithInvalidFilePath_ShouldThrowArgumentNullException`

Covers `null`, empty, and whitespace-only paths.

## Framework should preserve an absolute configuration path

Validated by:

* `Constructor_WithAbsolutePath_ShouldUseProvidedPath`

Verifies that an already rooted path is used without modification.

## Framework should resolve a relative configuration path against the application base directory

Validated by:

* `Constructor_WithRelativePath_ShouldResolveToAbsolutePath`

Verifies that relative paths are converted to an absolute path using `AppContext.BaseDirectory`.

# Configuration Loading Tests

## Framework should load and return a valid configuration

Validated by:

* `Load_WithValidConfiguration_ShouldReturnConfiguration`

Verifies the complete loading flow:

```text
File.Exists
    ↓
Deserialize
    ↓
ConfigurationValidator.Validate
    ↓
Return configuration
```

## Framework should throw when the configuration file does not exist

Validated by:

* `Load_WithMissingFile_ShouldThrowFileNotFoundException`

## Framework should throw when deserialization returns null

Validated by:

* `Load_WhenDeserializeReturnsNull_ShouldThrowInvalidOperationException`

## Framework should reject invalid configuration

Validated by:

* `Load_WithInvalidConfiguration_ShouldThrowInvalidOperationException`

Uses the validator's supported invalid configurations, such as:

* Unknown appender destination.
* Duplicate configured destinations.
* FileSystem appender without a file name.
* Custom formatter without a pattern.

Verifies that invalid configuration does not escape the provider as a valid configuration.

# Automatic Reload Tests

## Framework should enable automatic reload when requested

Validated by:

* `Constructor_WithAutoReloadEnabled_ShouldWatchConfigurationFile`

Verifies that changing the watched configuration file triggers the provider's reload path.

## Framework should not watch the configuration file when automatic reload is disabled

Validated by:

* `Constructor_WithAutoReloadDisabled_ShouldNotTriggerReload`

Verifies that file changes do not initiate automatic configuration reload when the feature is disabled.

# Test Scope

These tests validate only the common behavior implemented by **FileConfigurationProviderBase**.

The following responsibilities are intentionally outside the scope:

* JSON/XML/YAML deserialization implementation.
* Format-specific parsing rules.
* Logger creation.
* Logger caching.
* Log filtering.
* Appender creation.
* Log formatting.
* File writing/rolling.
* Configuration validation rules themselves.
* `FileSystemWatcher` framework internals.
* Concurrent reload implementation details.

`ConfigurationValidator` should have its own dedicated unit test suite.

Derived configuration providers should separately test their specific `Deserialize()` implementations.

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Invalid file path              |    ✅    |
| Absolute path resolution       |    ✅    |
| Relative path resolution       |    ✅    |
| Valid configuration loading    |    ✅    |
| Missing configuration file     |    ✅    |
| Null deserialization result    |    ✅    |
| Configuration validation       |    ✅    |
| Automatic reload enabled       |    ✅    |
| Automatic reload disabled      |    ✅    |
| Serialization/deserialization  |    ⬜    |
| Configuration validation rules |    ⬜    |
| Logger creation                |    ⬜    |
| FileSystemWatcher internals    |    ⬜    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
