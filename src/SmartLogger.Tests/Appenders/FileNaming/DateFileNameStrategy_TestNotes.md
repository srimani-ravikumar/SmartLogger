# DateFileNamingStrategy Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                  |
------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `DateFileNamingStrategy` class, validating configuration, active file naming, date-based rolled file naming, indexes, and input handling. |

# Objective

Validate that **DateFileNamingStrategy** correctly generates log file names based on the supplied file configuration by:

* Rejecting a null configuration.
* Generating the active file name using the configured file name and extension.
* Generating date-based rolled file names.
* Applying the configured date format.
* Handling the default rolling index.
* Appending a rolling index when supplied.
* Supporting zero and positive rolling indexes.
* Preserving configured file names and extensions.
* Handling custom date formats correctly.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:
  * Tests use an isolated `FileConfiguration` instance.
  * Date expectations are calculated using `DateTime.Now` at test execution time because the implementation directly uses the system clock.
* Dependencies:
  * `FileConfiguration`
  * `FileNamingConfiguration`
  * `IFileNamingStrategy`

# Initialization Tests

## Strategy should initialize successfully with a valid configuration

Validated by:

* `Constructor_WithValidConfiguration_ShouldCreateStrategy`

Verifies that a valid `FileConfiguration` can be supplied without throwing an exception.

## Strategy should reject a null configuration

Validated by:

* `Constructor_WithNullConfiguration_ShouldThrowArgumentNullException`

Verifies explicit null validation in the constructor.

# Active File Name Tests

## Strategy should generate an active file name using the configured file name and extension

Validated by:

* `CreateActiveFileName_WithValidConfiguration_ShouldReturnExpectedFileName`

For example:

```text
FileName = Application
Extension = log

Application.log
````

## Strategy should use a custom file name

Validated by:

* `CreateActiveFileName_WithCustomFileName_ShouldUseConfiguredFileName`

## Strategy should use a custom file extension

Validated by:

* `CreateActiveFileName_WithCustomExtension_ShouldUseConfiguredExtension`

## Strategy should preserve the configured extension without adding an additional dot

Validated by:

* `CreateActiveFileName_WithExtensionWithoutDot_ShouldReturnExpectedFileName`

The configuration contract specifies that the extension does not contain the leading dot.

## Strategy should generate an active file name using default configuration values

Validated by:

* `CreateActiveFileName_WithDefaultConfiguration_ShouldReturnDefaultFileName`

Verifies the default:

```text
Application.log
```

# Rolled File Name Tests

## Strategy should generate a rolled file name using the current date

Validated by:

* `CreateRolledFileName_WithDefaultIndex_ShouldIncludeCurrentDate`

For example:

```text
Application_2026-09-09.log
```

The expected date is calculated using the configured `DateFormat`.

## Strategy should generate a rolled file name without an index when index is zero

Validated by:

* `CreateRolledFileName_WithZeroIndex_ShouldNotIncludeIndex`

Verifies:

```text
Application_2026-09-09.log
```

rather than:

```text
Application_2026-09-09_0.log
```

## Strategy should generate a rolled file name with a positive index

Validated by:

* `CreateRolledFileName_WithPositiveIndex_ShouldIncludeIndex`

For example:

```text
Application_2026-09-09_1.log
```

## Strategy should preserve the supplied rolling index

Validated by:

* `CreateRolledFileName_WithMultipleIndexes_ShouldIncludeExactIndex`

Verifies that values such as `1`, `2`, `10`, and `100` are included exactly as supplied.

## Strategy should generate consistent file names for repeated calls within the same date

Validated by:

* `CreateRolledFileName_CalledMultipleTimesWithinSameDate_ShouldReturnSameDateComponent`

Verifies that the strategy derives the filename from the current date rather than maintaining internal rolling state.

# Date Format Tests

## Strategy should use the configured date format

Validated by:

* `CreateRolledFileName_WithCustomDateFormat_ShouldUseConfiguredFormat`

For example:

```text
DateFormat = yyyyMMdd

Application_20260909.log
```

## Strategy should support date formats containing separators

Validated by:

* `CreateRolledFileName_WithDateFormatContainingSeparators_ShouldPreserveFormat`

For example:

```text
DateFormat = yyyy-MM-dd

Application_2026-09-09.log
```

## Strategy should support a date format containing time components

Validated by:

* `CreateRolledFileName_WithDateAndTimeFormat_ShouldUseConfiguredFormat`

Verifies that the strategy delegates date rendering to `DateTime.ToString()` using the supplied format.

## Strategy should support literal text within a valid date format

Validated by:

* `CreateRolledFileName_WithLiteralDateFormatText_ShouldPreserveLiteralText`

Verifies that valid .NET date-format patterns are passed through without modification.

# Configuration Tests

## Strategy should reflect changes made to the supplied configuration

Validated by:

* `CreateActiveFileName_AfterConfigurationChange_ShouldUseUpdatedValues`
* `CreateRolledFileName_AfterConfigurationChange_ShouldUseUpdatedValues`

The strategy retains the supplied `FileConfiguration` reference rather than creating a defensive copy.

## Strategy should use the same configuration for active and rolled file naming

Validated by:

* `CreateActiveFileName_AndCreateRolledFileName_ShouldUseConfiguredFileNameAndExtension`

Verifies that both naming operations consistently use `FileName` and `Extension`.

# Rolling Index Boundary Tests

## Strategy should treat zero as an unindexed rolled file

Validated by:

* `CreateRolledFileName_WithZeroIndex_ShouldNotIncludeIndex`

## Strategy should treat positive one as the first indexed rolled file

Validated by:

* `CreateRolledFileName_WithIndexOne_ShouldAppendOne`

## Strategy should support large positive indexes

Validated by:

* `CreateRolledFileName_WithLargePositiveIndex_ShouldReturnExpectedFileName`

## Strategy should treat negative indexes as unindexed files

Validated by:

* `CreateRolledFileName_WithNegativeIndex_ShouldNotIncludeIndex`

The current implementation checks:

```text
index > 0
```

Therefore, negative values follow the same branch as zero.

> **Note:** This reflects the current implementation. If negative indexes are considered invalid by the domain contract, the implementation should explicitly reject them with `ArgumentOutOfRangeException` and the test should be updated accordingly.

# Special Configuration Tests

## Strategy should handle a file name containing spaces

Validated by:

* `CreateActiveFileName_WithFileNameContainingSpaces_ShouldPreserveFileName`

## Strategy should handle a file name containing special characters

Validated by:

* `CreateActiveFileName_WithSpecialCharacters_ShouldPreserveFileName`

## Strategy should handle Unicode file names

Validated by:

* `CreateActiveFileName_WithUnicodeFileName_ShouldPreserveFileName`

## Strategy should handle custom extensions

Validated by:

* `CreateActiveFileName_WithJsonExtension_ShouldReturnJsonFileName`

Verifies that the naming strategy is not restricted to `.log`.

# Invalid Configuration Tests

## Strategy should propagate invalid date format errors

Validated by:

* `CreateRolledFileName_WithInvalidDateFormat_ShouldPropagateFormatException`

The strategy directly delegates to:

```text
DateTime.Now.ToString(configuration.Naming.DateFormat)
```

Therefore, invalid format strings are expected to propagate the corresponding .NET exception.

## Strategy should handle a null Naming configuration according to current implementation behavior

Validated by:

* `CreateRolledFileName_WithNullNamingConfiguration_ShouldThrowNullReferenceException`

> **Note:** `FileConfiguration.Naming` normally defaults to a valid `FileNamingConfiguration`, but callers can explicitly assign `null`. The current implementation does not validate this dependency and therefore a `NullReferenceException` is expected.

# Test Scope

These tests validate only the public behavior of **DateFileNamingStrategy**.

The following responsibilities are intentionally tested separately:

* `FileConfiguration`:

  * Default configuration values.
  * Configuration object construction.
  * Configuration validation, if introduced.
* `FileNamingConfiguration`:

  * Naming strategy selection.
  * Default date format.
* `FileNamingStrategyType`:

  * Strategy selection and resolution.
* `TimestampFileNamingStrategy`:

  * Timestamp-based filename generation.
* Custom naming strategies:

  * Application-specific filename generation.
* Rolling policies:

  * Determining when a file should be rolled.
  * Daily rolling behavior.
  * Size-based rolling behavior.
* File appenders:

  * Physical file creation.
  * Directory creation.
  * File writing.
  * File rotation.
  * Archive handling.
  * Retention cleanup.

DateFileNamingStrategy tests should verify only filename construction and should not perform actual file-system operations.

# Coverage Summary

| Area                            | Covered |
| ------------------------------- | :-----: |
| Constructor initialization      |    ✅    |
| Null configuration validation   |    ✅    |
| Active file naming              |    ✅    |
| Default file naming             |    ✅    |
| Custom file name                |    ✅    |
| Custom extension                |    ✅    |
| Rolled file naming              |    ✅    |
| Current date generation         |    ✅    |
| Custom date formats             |    ✅    |
| Default rolling index           |    ✅    |
| Zero rolling index              |    ✅    |
| Positive rolling indexes        |    ✅    |
| Large rolling indexes           |    ✅    |
| Negative rolling index behavior |    ✅    |
| Configuration changes           |    ✅    |
| Unicode/special file names      |    ✅    |
| Invalid date format             |    ✅    |
| Null naming configuration       |    ✅    |
| Responsibility isolation        |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>