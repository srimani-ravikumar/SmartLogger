# InMemoryConfigurationProvider Unit Tests

## Document Information

| Project     | Version | Date       | Author  | Status        | Description                                                                                                                                                                                         |
| ----------- | ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SmartLogger | 1.0.0   | 2026-07-11 | Srimani | Initial Draft | Defined the unit test coverage for the `InMemoryConfigurationProvider` class, validating configuration loading, default configuration creation, configuration validation, and constructor behavior. |

# Objective

Validate that **InMemoryConfigurationProvider** correctly provides logging configuration from an in-memory object by:

* Accepting valid configuration instances.
* Rejecting invalid constructor arguments.
* Returning validated configuration objects.
* Creating a valid default configuration.
* Detecting structurally invalid configurations.
* Preserving configuration object identity.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * Every test creates a fresh `LogConfigurationHolder`.
  * No shared or static state exists.

# Constructor Tests

## Provider should initialize successfully with a valid configuration

Validated by:

* `Constructor_WithValidConfiguration_ShouldCreateProvider`

## Provider should reject a null configuration

Validated by:

* `Constructor_WithNullConfiguration_ShouldThrowArgumentNullException`

# Configuration Loading Tests

## Provider should return the supplied configuration

Validated by:

* `Load_WithValidConfiguration_ShouldReturnConfiguration`

Verifies that the provider exposes the supplied configuration after successful validation.

## Provider should return the same configuration instance

Validated by:

* `Load_CalledMultipleTimes_ShouldReturnSameConfigurationInstance`

Ensures the provider returns the original configuration object without cloning or creating new instances.

## Provider should allow multiple consecutive loads

Validated by:

* `Load_CalledMultipleTimes_ShouldAlwaysReturnConfiguration`

Verifies that repeated calls consistently return the validated configuration.

# Default Configuration Tests

## Provider should create a valid default configuration

Validated by:

* `CreateDefault_ShouldReturnValidProvider`

Ensures the generated provider successfully loads its default configuration.

## Default configuration should use DEBUG as the root log level

Validated by:

* `CreateDefault_ShouldConfigureRootLogLevelAsDebug`

## Default configuration should contain exactly one appender

Validated by:

* `CreateDefault_ShouldCreateSingleAppender`

## Default configuration should configure a Console destination

Validated by:

* `CreateDefault_ShouldConfigureConsoleAppender`

## Default configuration should configure DEBUG as the appender log level

Validated by:

* `CreateDefault_ShouldConfigureAppenderLogLevelAsDebug`

# Configuration Validation Tests

## Provider should reject configurations without appenders

Validated by:

* `Load_WithNullAppenderCollection_ShouldThrowInvalidOperationException`
* `Load_WithEmptyAppenderCollection_ShouldThrowInvalidOperationException`

Ensures every configuration contains at least one configured appender.

## Provider should reject appenders with an unknown destination

Validated by:

* `Load_WithUnknownAppenderDestination_ShouldThrowInvalidOperationException`

Verifies that every configured appender specifies a valid output destination.

## Provider should accept multiple valid appenders

Validated by:

* `Load_WithMultipleValidAppenders_ShouldReturnConfiguration`

Ensures configurations containing multiple valid appenders successfully pass validation.

## Provider should support all supported destination types

Validated by:

* `Load_WithConsoleAppender_ShouldSucceed`
* `Load_WithFileSystemAppender_ShouldSucceed`
* `Load_WithDatabaseAppender_ShouldSucceed`

Verifies that every supported destination passes structural validation.

# Input Validation Tests

## Provider should support all log levels

Validated by:

* `Load_WithRootLogLevelDebug_ShouldSucceed`
* `Load_WithRootLogLevelInfo_ShouldSucceed`
* `Load_WithRootLogLevelWarning_ShouldSucceed`
* `Load_WithRootLogLevelError_ShouldSucceed`
* `Load_WithRootLogLevelFatal_ShouldSucceed`

Ensures validation is independent of the configured root log level.

## Provider should preserve logger overrides

Validated by:

* `Load_WithLoggerOverrides_ShouldReturnConfiguration`

Verifies that logger override configuration remains unchanged after loading.

## Provider should preserve formatter configuration

Validated by:

* `Load_WithFormatterConfiguration_ShouldReturnConfiguration`

Ensures formatter configuration is not modified during validation.

## Provider should preserve file destination configuration

Validated by:

* `Load_WithFileDestinationConfiguration_ShouldReturnConfiguration`

Ensures file-specific configuration remains intact after loading.

## Provider should preserve asynchronous logging configuration

Validated by:

* `Load_WithAsyncLoggingEnabled_ShouldReturnConfiguration`

# Boundary Tests

## Provider should support a large number of appenders

Validated by:

* `Load_WithManyAppenders_ShouldReturnConfiguration`

Ensures validation scales correctly for larger configurations.

## Provider should support a large number of logger overrides

Validated by:

* `Load_WithManyLoggerOverrides_ShouldReturnConfiguration`

# Exception Handling Tests

## Provider should throw InvalidOperationException for structurally invalid configurations

Validated by:

* `Load_WithNullAppenderCollection_ShouldThrowInvalidOperationException`
* `Load_WithEmptyAppenderCollection_ShouldThrowInvalidOperationException`
* `Load_WithUnknownAppenderDestination_ShouldThrowInvalidOperationException`

Verifies that invalid configuration structures are rejected before being returned to consumers.

# Current Validation Limitations

The current implementation intentionally validates only the structural requirements necessary for configuration loading.

The following scenarios are **not validated** by `InMemoryConfigurationProvider` and are expected to be handled elsewhere within the framework:

* Null `Destination` objects
* File destination requiring missing `FileConfiguration`
* Database destination requiring database configuration
* Formatter configuration correctness
* Custom formatter pattern validation
* Rolling policy validation
* Logger override validation
* Duplicate logger overrides
* Duplicate appenders
* Invalid file paths
* Invalid rolling policy combinations

> **Note:** These validations belong to higher-level configuration validation and are outside the responsibility of `InMemoryConfigurationProvider`.

# Test Scope

These tests validate only the public behavior of **InMemoryConfigurationProvider**.

The following responsibilities are intentionally tested separately within other framework components:

* Configuration parsing
* JSON configuration loading
* Configuration file watching
* Runtime configuration reload
* Logger creation
* Appender creation
* Formatter creation
* Rolling policy behavior
* File system interactions
* Database connectivity

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Constructor validation         |    ✅    |
| Configuration loading          |    ✅    |
| Configuration validation       |    ✅    |
| Default configuration creation |    ✅    |
| Object identity preservation   |    ✅    |
| Input validation               |    ✅    |
| Exception handling             |    ✅    |
| Multiple appender support      |    ✅    |
| Boundary scenarios             |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>