# LogConfigurationHolder Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                                                                                                   |
------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
1.1.0   | 2026-07-12 | Srimani | Updated Draft | Defined the unit test coverage for the SmartLogger configuration model classes, validating default configuration, computed properties, object graph initialization, configuration contracts, and the redesigned file configuration hierarchy. |
1.2.0   | 2026-08-12 | Srimani | Revised Draft | Revised against the updated configuration model. Added negative and boundary coverage for optional configuration, unset values, collection reassignment, and unused configuration branches. |

# Objective

Validate that the SmartLogger configuration model classes correctly represent the logging configuration by:

* Providing expected default values.
* Initializing nested configuration objects.
* Maintaining configuration contracts.
* Computing derived configuration values correctly.
* Supporting customization through configuration objects.
* Behaving predictably when configuration is missing, empty, or unset.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **None Required**
* Target Framework: **.NET**

---

# LogConfigurationHolder Tests

## Configuration should initialize with expected default values

Validated by:

* `Constructor_ShouldInitializeDefaultConfiguration`

Verifies:

* RootLogLevel = INFO
* LoggerOverrides initialized
* Appenders initialized
* Async logging disabled

---

## Configuration should enable the default console appender when no appenders exist

Validated by:

* `EnableDefaultConsoleAppender_WithEmptyAppenderCollection_ShouldReturnTrue`

---

## Configuration should enable the default console appender when the appender collection is null

Validated by:

* `EnableDefaultConsoleAppender_WithNullAppenderCollection_ShouldReturnTrue`

---

## Configuration should disable the default console appender when appenders are configured

Validated by:

* `EnableDefaultConsoleAppender_WithConfiguredAppender_ShouldReturnFalse`

---

## Configuration should support enabling asynchronous logging

Validated by:

* `EnableAsyncLoggingProcess_WhenEnabled_ShouldReturnTrue`

---

## Configuration should allow logger overrides to be configured

Validated by:

* `LoggerOverrides_WithConfiguredOverrides_ShouldContainConfiguredEntries`

---

## Configuration should allow multiple appenders

Validated by:

* `Appenders_WithMultipleConfiguredAppenders_ShouldContainAllConfiguredAppenders`

---

## Configuration should allow collections to be replaced

Validated by:

* `LoggerOverrides_WhenReassigned_ShouldReplaceExistingCollection`
* `Appenders_WhenReassigned_ShouldReplaceExistingCollection`

---

## Configuration should keep asynchronous logging disabled unless explicitly enabled

Validated by:

* `EnableAsyncLoggingProcess_WhenNotConfigured_ShouldReturnFalse`

---

## Configuration should allow logger overrides to remain empty

Validated by:

* `LoggerOverrides_WithNoConfiguredOverrides_ShouldBeEmpty`

---

# LoggerOverrideConfiguration Tests

## Logger override should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultLoggerOverride`

Verifies:

* LoggerName = Empty
* LogLevel = INFO

---

## Logger override should support custom logger names

Validated by:

* `LoggerOverride_WithCustomLoggerName_ShouldStoreValue`

---

## Logger override should support custom log levels

Validated by:

* `LoggerOverride_WithCustomLogLevel_ShouldStoreValue`

---

## Logger override should allow the logger name to remain unset

Validated by:

* `LoggerOverride_WithoutLoggerName_ShouldRemainEmpty`

---

# AppenderConfiguration Tests

## Appender configuration should initialize nested configuration objects

Validated by:

* `Constructor_ShouldInitializeNestedConfigurations`

Verifies:

* Destination initialized
* Formatter initialized
* Filter = null
* AppenderLogLevel = null

---

## Appender configuration should allow custom filters

Validated by:

* `Filter_WithCustomObject_ShouldStoreValue`

---

## Appender configuration should support appender-specific log levels

Validated by:

* `AppenderLogLevel_WithConfiguredLevel_ShouldStoreValue`

---

## Appender configuration should allow the appender log level to remain unspecified

Validated by:

* `AppenderLogLevel_WithNullValue_ShouldRemainNull`

---

# DestinationConfiguration Tests

## Destination configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultDestinationConfiguration`

Verifies:

* Type = Console
* File = null

---

## Destination configuration should support file configuration

Validated by:

* `Destination_WithFileConfiguration_ShouldStoreConfiguration`

---

## Destination configuration should leave file configuration unset for non-file destinations

Validated by:

* `Destination_WithConsoleType_ShouldLeaveFileConfigurationNull`

---

## Destination configuration should support an unknown destination

Validated by:

* `Destination_WithUnknownType_ShouldStoreValue`

---

# FileConfiguration Tests

## File configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultFileConfiguration`

Verifies:

* Directory = Logs
* FileName = Application
* Extension = log
* Naming initialized
* Rolling initialized
* Archive initialized
* Retention initialized

---

## File configuration should support custom directory

Validated by:

* `Directory_WithConfiguredValue_ShouldStoreValue`

---

## File configuration should support custom file name

Validated by:

* `FileName_WithConfiguredValue_ShouldStoreValue`

---

## File configuration should support custom extensions

Validated by:

* `Extension_WithConfiguredValue_ShouldStoreValue`

---

# FileNamingConfiguration Tests

## File naming configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultNamingConfiguration`

Verifies:

* Strategy = Date
* DateFormat = yyyy-MM-dd

---

## File naming configuration should support a custom naming strategy without a date format

Validated by:

* `FileNaming_WithCustomStrategy_ShouldIgnoreDateFormatRequirement`

---

## File naming configuration should support custom naming options

Validated by:

* `FileNaming_WithCustomizedConfiguration_ShouldStoreValues`

Verifies:

* Strategy
* DateFormat

---

# FileRollingConfiguration Tests

## File rolling configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultRollingConfiguration`

Verifies:

* Strategy = Daily
* MaxFileSizeMB = 10

---

## File rolling configuration should support size-based rolling

Validated by:

* `RollingConfiguration_WithSizeStrategy_ShouldStoreValues`

---

## File rolling configuration should retain the size threshold for daily rolling

Validated by:

* `RollingConfiguration_WithDailyStrategy_ShouldRetainDefaultMaxFileSize`

---

# ArchiveConfiguration Tests

## Archive configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultArchiveConfiguration`

Verifies:

* Enabled = true
* Directory = Archive
* Compress = true

---

## Archive configuration should support custom archive settings

Validated by:

* `ArchiveConfiguration_WithCustomizedValues_ShouldStoreValues`

---

## Archive configuration should support archival being disabled

Validated by:

* `ArchiveConfiguration_WhenDisabled_ShouldReturnFalse`

---

# RetentionConfiguration Tests

## Retention configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultRetentionConfiguration`

Verifies:

* RetentionDays = 30

---

## Retention configuration should support custom retention settings

Validated by:

* `RetentionConfiguration_WithConfiguredRetentionDays_ShouldStoreValue`

---

# FormatterConfiguration Tests

## Formatter configuration should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultFormatterConfiguration`

Verifies:

* PlainText output
* Simple layout
* Empty pattern
* Default JSON fields
* Empty field mappings

---

## Formatter configuration should initialize default JSON fields

Validated by:

* `IncludedJsonFields_ShouldContainExpectedDefaultFields`

Verifies the default observable schema:

* timestamp
* level
* thread
* correlation
* source
* message

---

## Formatter configuration should support custom output formats

Validated by:

* `OutputFormat_WithConfiguredValue_ShouldStoreValue`

---

## Formatter configuration should support custom layout types

Validated by:

* `LayoutType_WithConfiguredValue_ShouldStoreValue`

---

## Formatter configuration should support custom layout patterns

Validated by:

* `Pattern_WithCustomPattern_ShouldStoreValue`

---

## Formatter configuration should support JSON field mappings

Validated by:

* `JsonFieldMappings_WithConfiguredMappings_ShouldContainConfiguredEntries`

---

## Formatter configuration should allow the pattern to remain unset for non-custom layouts

Validated by:

* `Pattern_WithSimpleLayout_ShouldRemainEmpty`

---

## Formatter configuration should allow the JSON field list to be replaced or cleared

Validated by:

* `IncludedJsonFields_WhenReassigned_ShouldContainOnlyConfiguredFields`
* `IncludedJsonFields_WhenCleared_ShouldBeEmpty`

---

# JsonFieldMappingConfiguration Tests

## JSON field mapping should initialize with expected defaults

Validated by:

* `Constructor_ShouldInitializeDefaultJsonFieldMapping`

Verifies:

* SourceField = Empty
* TargetField = Empty

---

## JSON field mapping should support custom field names

Validated by:

* `JsonFieldMapping_WithCustomFields_ShouldStoreValues`

---

# Enumeration Tests

## Configuration enums should expose expected values

Validated by:

* `LogOutputDestination_ShouldContainExpectedValues`
* `LogOutputFormat_ShouldContainExpectedValues`
* `LogMessageLayoutType_ShouldContainExpectedValues`
* `FileNamingStrategyType_ShouldContainExpectedValues`
* `RollingStrategyType_ShouldContainExpectedValues`

Verifies the public configuration contract exposed by the framework.

---

# Test Scope

These tests validate only the public configuration contract of the SmartLogger configuration model classes.

The following responsibilities are intentionally tested separately within other framework components:

* Configuration validation
* Logger creation
* Configuration loading
* Log level resolution
* Formatter implementation
* Appender implementation
* File naming generation
* File rolling execution
* Archive processing
* Retention cleanup
* JSON serialization
* Configuration reload behavior

---

# Coverage Summary

| Area                         | Covered |
| ---------------------------- | :-----: |
| Default configuration        |    ✅    |
| Default property values      |    ✅    |
| Nested object initialization |    ✅    |
| Computed properties          |    ✅    |
| Configuration customization  |    ✅    |
| Object graph integrity       |    ✅    |
| Default JSON schema          |    ✅    |
| File configuration           |    ✅    |
| File naming configuration    |    ✅    |
| File rolling configuration   |    ✅    |
| Archive configuration        |    ✅    |
| Retention configuration      |    ✅    |
| Enumeration contract         |    ✅    |
| Optional / unset values      |    ✅    |
| Empty collections            |    ✅    |
| Collection reassignment      |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
