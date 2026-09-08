# ConfigurationValidator Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                           |
| ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-08 | Srimani | Initial Draft | Defined concise unit test coverage for SmartLogger configuration validation rules and invalid configuration handling. |

# Objective

Validate that **ConfigurationValidator** correctly enforces SmartLogger configuration rules by:

* Rejecting null configuration.
* Accepting valid configurations.
* Preventing duplicate output destinations.
* Requiring a configured appender destination.
* Requiring a file name for FileSystem appenders.
* Requiring a pattern for Custom formatters.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates its own `LogConfigurationHolder`.
  * No shared state or external resources are required.

# Configuration Validation Tests

## Framework should accept valid configurations

Validated by:

* `Validate_WithValidConfiguration_ShouldNotThrow`

Covers representative valid configurations including:

* Empty appender collection.
* Console appender.
* FileSystem appender with a valid file configuration.
* Custom formatter with a non-empty pattern.
* Multiple different destinations.

## Framework should reject null configuration

Validated by:

* `Validate_WithNullConfiguration_ShouldThrowArgumentNullException`

Verifies that null configuration input is rejected immediately.

# Destination Validation Tests

## Framework should reject duplicate configured destinations

Validated by:

* `Validate_WithDuplicateDestination_ShouldThrowInvalidOperationException`

Verifies that the same non-`Unknown` destination cannot be configured more than once.

Covers duplicate:

* Console destinations.
* FileSystem destinations.

## Framework should allow different configured destinations

Validated by:

* `Validate_WithDifferentDestinations_ShouldNotThrow`

Verifies that valid combinations such as Console + FileSystem are accepted.

## Framework should reject an unknown appender destination

Validated by:

* `Validate_WithUnknownDestination_ShouldThrowInvalidOperationException`

Verifies that every appender must specify a valid `LogOutputDestination`.

## Framework should allow multiple Unknown destinations without triggering duplicate-destination validation

Validated by:

* `Validate_WithMultipleUnknownDestinations_ShouldValidateEachAppenderIndividually`

Verifies that `Unknown` is excluded from the duplicate-destination check, while each individual appender is still validated and therefore rejected for having no destination.

# File Configuration Tests

## Framework should require a file name for FileSystem appenders

Validated by:

* `Validate_WithFileSystemAppenderWithoutFileName_ShouldThrowInvalidOperationException`

Covers:

* Missing `File` configuration.
* Null/empty file name.
* Whitespace-only file name.

## Framework should accept a FileSystem appender with a valid file name

Validated by:

* `Validate_WithValidFileSystemConfiguration_ShouldNotThrow`

Verifies that a FileSystem destination with a non-empty file name is accepted.

# Custom Layout Tests

## Framework should require a pattern for Custom layouts

Validated by:

* `Validate_WithCustomLayoutWithoutPattern_ShouldThrowInvalidOperationException`

Covers:

* Empty pattern.
* Null pattern.
* Whitespace-only pattern.

## Framework should accept a Custom layout with a valid pattern

Validated by:

* `Validate_WithCustomLayoutWithPattern_ShouldNotThrow`

## Framework should not require a pattern for non-Custom layouts

Validated by:

* `Validate_WithNonCustomLayoutWithoutPattern_ShouldNotThrow`

Verifies that Simple and Detailed layouts do not require a custom pattern.

# Test Scope

These tests validate only the rules implemented by **ConfigurationValidator**.

The following are intentionally outside the scope:

* JSON/XML deserialization.
* File path resolution.
* Configuration file existence.
* Automatic configuration reload.
* Logger creation.
* Logger configuration application.
* Appender execution.
* Log formatting.
* File writing.

Those responsibilities belong to their respective components.

# Coverage Summary

| Area                              | Covered |
| --------------------------------- | :-----: |
| Null configuration                |    ✅    |
| Valid configuration               |    ✅    |
| Duplicate destinations            |    ✅    |
| Different destinations            |    ✅    |
| Unknown destination               |    ✅    |
| FileSystem file name requirement  |    ✅    |
| Custom layout pattern requirement |    ✅    |
| Non-Custom layout without pattern |    ✅    |
| JSON/XML deserialization          |    ⬜    |
| File handling                     |    ⬜    |
| Automatic reload                  |    ⬜    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
