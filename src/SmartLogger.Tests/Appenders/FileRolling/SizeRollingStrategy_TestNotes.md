# SizeRollingStrategy Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                   |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `SizeRollingStrategy` class, validating file existence handling, configured size thresholds, boundary conditions, oversized files, and invalid configuration behavior. |

# Objective

Validate that **SizeRollingStrategy** correctly determines whether an active log file should be rolled based on its configured file-size threshold by:

* Converting the configured maximum file size from megabytes to bytes.
* Returning `false` when the active file does not exist.
* Returning `false` when the file size is below the configured threshold.
* Returning `true` when the file size reaches the configured threshold.
* Returning `true` when the file exceeds the configured threshold.
* Correctly handling small and large configured thresholds.
* Evaluating the current file size on every invocation.
* Operating independently for separate strategy instances.
* Handling invalid configuration values according to the current implementation.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Tests use uniquely created temporary files.
  * Temporary files are deleted during test cleanup.
  * Tests must not interact with application log files or production directories.
  * File sizes should be constructed deterministically rather than relying on existing files.

# Constructor Tests

## Strategy should initialize successfully with a valid configuration

Validated by:

* `Constructor_WithValidConfiguration_ShouldInitializeSuccessfully`

Verifies that a valid `FileConfiguration` can be used to create the strategy.

## Strategy should calculate the maximum file size in bytes from megabytes

Validated by:

* `ShouldRoll_WithConfiguredMaxFileSize_ShouldUseMegabyteThreshold`

For:

```text
MaxFileSizeMB = 1
```

the effective threshold should be:

```text
1 × 1024 × 1024 = 1,048,576 bytes
```

The strategy should use this calculated value when evaluating the active file.

## Strategy should reject a null configuration appropriately

Validated by:

* `Constructor_WithNullConfiguration_ShouldThrowNullReferenceException`

The current implementation directly accesses:

```csharp
configuration.Rolling.MaxFileSizeMB
```

Therefore a null configuration currently results in:

```text
NullReferenceException
```

> **Note:** This reflects the current implementation. Explicit `ArgumentNullException` validation could be introduced as a future enhancement.

# File Existence Tests

## Strategy should not roll when the active file does not exist

Validated by:

* `ShouldRoll_WhenFileDoesNotExist_ShouldReturnFalse`

The implementation explicitly checks:

```csharp
File.Exists(activeFilePath)
```

A missing active file must not trigger a rolling operation.

## Strategy should not require the file to be created before strategy initialization

Validated by:

* `ShouldRoll_WithMissingFile_ShouldReturnFalse`

The strategy itself should remain usable even when the active log file has not yet been created.

# File Size Threshold Tests

## Strategy should not roll when file size is below the configured threshold

Validated by:

* `ShouldRoll_WhenFileSizeIsBelowThreshold_ShouldReturnFalse`

For example, with:

```text
MaxFileSizeMB = 1
```

a file smaller than:

```text
1,048,576 bytes
```

should not trigger rolling.

## Strategy should roll when file size exactly reaches the configured threshold

Validated by:

* `ShouldRoll_WhenFileSizeEqualsThreshold_ShouldReturnTrue`

The implementation uses:

```csharp
FileInfo.Length >= _maxBytes
```

Therefore the boundary condition is inclusive.

For:

```text
MaxFileSizeMB = 1
```

a file of exactly:

```text
1,048,576 bytes
```

must trigger rolling.

## Strategy should roll when file size exceeds the configured threshold

Validated by:

* `ShouldRoll_WhenFileSizeExceedsThreshold_ShouldReturnTrue`

A file larger than the configured threshold must trigger rolling.

## Strategy should not roll when file size is just below the threshold

Validated by:

* `ShouldRoll_WhenFileSizeIsOneByteBelowThreshold_ShouldReturnFalse`

This explicitly validates the boundary immediately before the rolling threshold.

## Strategy should roll when file size is one byte above the threshold

Validated by:

* `ShouldRoll_WhenFileSizeIsOneByteAboveThreshold_ShouldReturnTrue`

This explicitly validates the boundary immediately after the rolling threshold.

# Configuration Tests

## Strategy should respect the configured maximum file size

Validated by:

* `ShouldRoll_WithDifferentMaxFileSize_ShouldUseConfiguredThreshold`

Changing:

```text
MaxFileSizeMB
```

should change the threshold used by the strategy.

## Strategy should support a one megabyte threshold

Validated by:

* `ShouldRoll_WithOneMBThreshold_ShouldUseCorrectThreshold`

Verifies correct conversion of:

```text
1 MB = 1,048,576 bytes
```

## Strategy should support larger file-size thresholds

Validated by:

* `ShouldRoll_WithLargeMaxFileSize_ShouldUseCorrectThreshold`

For example:

```text
MaxFileSizeMB = 100
```

should result in a threshold of:

```text
104,857,600 bytes
```

## Strategy should use the configured threshold captured during construction

Validated by:

* `ShouldRoll_WhenConfigurationChangesAfterConstruction_ShouldRetainOriginalThreshold`

The constructor calculates:

```csharp
_maxBytes =
    configuration.Rolling.MaxFileSizeMB * 1024 * 1024;
```

The resulting threshold is stored in a private readonly field.

Therefore changes to `configuration.Rolling.MaxFileSizeMB` after construction should not change the strategy's existing threshold.

# File State Tests

## Strategy should evaluate the current file size on every invocation

Validated by:

* `ShouldRoll_WhenFileGrowsBetweenChecks_ShouldReflectCurrentFileSize`

For example:

```text
Initial file size < threshold
        ↓
ShouldRoll() → false
        ↓
File grows >= threshold
        ↓
ShouldRoll() → true
```

The strategy should not cache the file size.

## Strategy should continue returning false when an existing file remains below the threshold

Validated by:

* `ShouldRoll_CalledMultipleTimesBelowThreshold_ShouldReturnFalse`

Repeated evaluations of an unchanged file below the threshold should consistently return `false`.

## Strategy should continue returning true while an existing file remains at or above the threshold

Validated by:

* `ShouldRoll_CalledMultipleTimesAtOrAboveThreshold_ShouldReturnTrue`

The strategy itself does not maintain a "roll already requested" state. Therefore an existing file that remains above the threshold should continue to satisfy the rolling condition.

# Active File Path Tests

## Strategy should evaluate the file specified by the active file path

Validated by:

* `ShouldRoll_WithValidActiveFilePath_ShouldEvaluateFile`

The file path supplied to `ShouldRoll()` determines which file's size is evaluated.

## Strategy should return false for an empty file path when no such file exists

Validated by:

* `ShouldRoll_WithEmptyFilePath_ShouldReturnFalse`

Because `File.Exists()` returns `false` for an invalid/nonexistent path, the strategy should not trigger rolling.

## Strategy should return false for a nonexistent file path

Validated by:

* `ShouldRoll_WithNonExistentFilePath_ShouldReturnFalse`

Ensures arbitrary nonexistent paths do not result in exceptions during normal evaluation.

# Independent Instance Tests

## Separate strategy instances should maintain independent thresholds

Validated by:

* `ShouldRoll_WithSeparateInstances_ShouldUseTheirOwnConfiguration`

For example:

```text
Strategy A → 1 MB
Strategy B → 10 MB
```

each instance should evaluate files using its own configured threshold.

# Invalid Configuration Tests

## Strategy should handle a zero maximum file size according to the current implementation

Validated by:

* `ShouldRoll_WithZeroMaxFileSize_ShouldEvaluateAccordingToZeroThreshold`

With:

```text
MaxFileSizeMB = 0
```

the calculated threshold is:

```text
0 bytes
```

Therefore any existing file with a length greater than or equal to zero should cause:

```text
ShouldRoll() → true
```

> **Note:** This is mathematically consistent with the current implementation but may not be a desirable production configuration. Configuration validation could reject zero in a higher-level configuration validator.

## Strategy should handle a negative maximum file size according to the current implementation

Validated by:

* `ShouldRoll_WithNegativeMaxFileSize_ShouldEvaluateAccordingToCalculatedThreshold`

The current constructor does not validate negative values.

Therefore the test should document current behavior rather than assume a particular validation policy.

> **Note:** A negative maximum file size is logically invalid and should ideally be rejected during configuration validation rather than by the rolling strategy itself.

# Test Scope

These tests validate only the responsibilities of **SizeRollingStrategy**.

The following responsibilities are intentionally tested separately within their respective components:

* File creation
* File writing
* File naming
* Daily rolling decisions
* Timestamp generation
* File renaming
* Archive processing
* Archive compression
* Retention cleanup
* Rolling orchestration
* Configuration validation
* Concurrent rolling coordination
* Thread synchronization

`SizeRollingStrategy` is responsible only for answering:

```text
"Does the current active file exist and has its size
reached or exceeded the configured maximum?"
```

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Constructor                |    ✅    |
| Configuration conversion   |    ✅    |
| File existence             |    ✅    |
| Below threshold            |    ✅    |
| Exactly at threshold       |    ✅    |
| Above threshold            |    ✅    |
| Boundary - one byte below  |    ✅    |
| Boundary - one byte above  |    ✅    |
| Small thresholds           |    ✅    |
| Large thresholds           |    ✅    |
| Configuration immutability |    ✅    |
| File growth between checks |    ✅    |
| Repeated evaluation        |    ✅    |
| Active file path handling  |    ✅    |
| Empty/nonexistent paths    |    ✅    |
| Instance isolation         |    ✅    |
| Zero threshold             |    ✅    |
| Negative threshold         |    ⚠️   |
| Filesystem isolation       |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
