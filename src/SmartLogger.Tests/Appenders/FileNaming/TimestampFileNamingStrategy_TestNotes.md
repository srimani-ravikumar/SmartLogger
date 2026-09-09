# TimestampFileNamingStrategy Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                     |
| ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `TimestampFileNamingStrategy` class, validating timestamp-based active and rolled file name generation, configuration usage, index handling, and invalid input behavior. |

# Objective

Validate that **TimestampFileNamingStrategy** correctly generates timestamp-based log file names by:

* Using the configured base file name.
* Using the configured file extension.
* Generating filesystem-safe timestamp-based names.
* Generating names using a consistent timestamp format.
* Generating rolled file names with the supplied rolling index.
* Supporting the default rolling index.
* Rejecting invalid configuration input appropriately.
* Remaining independent of directory, rolling, archival, retention, and filesystem operations.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates an independent `FileConfiguration`.
  * Tests should avoid depending on the exact system clock value.
  * Timestamp assertions should validate the expected naming pattern and configured values.

# Constructor Tests

## Strategy should initialize successfully with a valid configuration

Validated by:

* `Constructor_WithValidConfiguration_ShouldInitializeSuccessfully`

Verifies that a valid `FileConfiguration` can be supplied and the strategy is ready to generate file names.

## Strategy should reject a null configuration

Validated by:

* `Constructor_WithNullConfiguration_ShouldThrowArgumentNullException`

Ensures the strategy cannot operate without the configuration required to construct file names.

# Active File Name Tests

## Strategy should generate an active file name using the configured base name

Validated by:

* `CreateActiveFileName_WithConfiguredFileName_ShouldIncludeFileName`

For:

```text
FileName = Application
```

the generated name should begin with:

```text
Application-
```

## Strategy should generate an active file name using the configured extension

Validated by:

* `CreateActiveFileName_WithConfiguredExtension_ShouldUseExtension`

For:

```text
Extension = log
```

the generated file name should end with:

```text
.log
```

## Strategy should generate a timestamp-based active file name

Validated by:

* `CreateActiveFileName_ShouldContainTimestamp`

The generated name should contain a timestamp using the configured timestamp naming convention.

Expected structure:

```text
{FileName}-{yyyy-MM-dd-HH-mm-ss}.{Extension}
```

## Strategy should generate filesystem-safe timestamp characters

Validated by:

* `CreateActiveFileName_ShouldGenerateFilesystemSafeName`

The generated file name should not contain characters that are invalid for normal Windows file names.

The timestamp format should therefore use separators such as `-` rather than characters such as `/`, `:`, or other filesystem-sensitive characters.

## Strategy should generate different timestamp names across different timestamp boundaries

Validated by:

* `CreateActiveFileName_AcrossDifferentTimestamps_ShouldGenerateDifferentNames`

Verifies that the timestamp component represents the generation time rather than producing a permanently fixed file name.

> **Note:** This test should control or abstract the clock if the implementation allows it. Tests should not rely on arbitrary `Thread.Sleep()` timing.

# Rolled File Name Tests

## Strategy should generate a rolled file name using the configured file name

Validated by:

* `CreateRolledFileName_WithConfiguredFileName_ShouldIncludeFileName`

Ensures rolled files use the same configured base file name as active files.

## Strategy should generate a rolled file name using the configured extension

Validated by:

* `CreateRolledFileName_WithConfiguredExtension_ShouldUseExtension`

Ensures the configured extension is preserved for rolled files.

## Strategy should generate a timestamp-based rolled file name

Validated by:

* `CreateRolledFileName_ShouldContainTimestamp`

The generated name should contain a timestamp using the same timestamp format as the active file name.

## Strategy should include the rolling index in the rolled file name

Validated by:

* `CreateRolledFileName_WithPositiveIndex_ShouldIncludeIndex`

For example:

```text
Application-2026-09-09-23-15-30-2.log
```

where:

```text
2
```

is the supplied rolling index.

## Strategy should support the default rolling index

Validated by:

* `CreateRolledFileName_WithoutIndex_ShouldUseDefaultIndex`

Verifies that:

```csharp
CreateRolledFileName()
```

behaves consistently with:

```csharp
CreateRolledFileName(0)
```

according to the defined naming convention.

## Strategy should generate different rolled names for different indexes

Validated by:

* `CreateRolledFileName_WithDifferentIndexes_ShouldGenerateDifferentNames`

For the same timestamp window:

```text
CreateRolledFileName(0)
CreateRolledFileName(1)
```

should produce distinguishable file names.

This prevents multiple rolled files generated within the same rolling window from overwriting one another.

# Configuration Tests

## Strategy should respect custom file names

Validated by:

* `CreateActiveFileName_WithCustomFileName_ShouldUseConfiguredName`
* `CreateRolledFileName_WithCustomFileName_ShouldUseConfiguredName`

For example:

```text
FileName = SalesApp
```

should produce names beginning with:

```text
SalesApp-
```

## Strategy should respect custom file extensions

Validated by:

* `CreateActiveFileName_WithCustomExtension_ShouldUseConfiguredExtension`
* `CreateRolledFileName_WithCustomExtension_ShouldUseConfiguredExtension`

For example:

```text
Extension = txt
```

should produce:

```text
SalesApp-2026-09-09-23-15-30.txt
```

or the corresponding rolled form.

## Strategy should support different configured extensions

Validated by:

* `CreateActiveFileName_WithJsonExtension_ShouldGenerateJsonFileName`

Verifies that the naming strategy does not hard-code `.log`.

# Index Validation Tests

## Strategy should support index zero

Validated by:

* `CreateRolledFileName_WithZeroIndex_ShouldGenerateValidName`

Index `0` represents the default/initial rolling index and should produce a valid rolled file name.

## Strategy should support positive indexes

Validated by:

* `CreateRolledFileName_WithPositiveIndex_ShouldGenerateValidName`

Verifies support for indexes such as:

```text
1
2
10
999
```

## Strategy should reject negative indexes

Validated by:

* `CreateRolledFileName_WithNegativeIndex_ShouldThrowArgumentOutOfRangeException`

A negative rolling index has no meaningful representation in the naming contract and should be rejected rather than producing an ambiguous file name.

> **Note:** This test assumes the intended implementation contract explicitly validates negative indexes. If the current implementation allows them, this should instead be documented as current behavior and considered for enhancement.

# Naming Consistency Tests

## Active and rolled file names should use the same timestamp format

Validated by:

* `CreateActiveFileName_AndRolledFileName_ShouldUseSameTimestampFormat`

Ensures both naming operations follow the same timestamp convention.

## Generated file names should preserve the configured base name and extension

Validated by:

* `CreateActiveFileName_ShouldPreserveConfiguredNameAndExtension`
* `CreateRolledFileName_ShouldPreserveConfiguredNameAndExtension`

The timestamp and rolling index should augment the configured name rather than replacing it.

## Generated file names should not contain directory information

Validated by:

* `CreateActiveFileName_ShouldReturnFileNameOnly`
* `CreateRolledFileName_ShouldReturnFileNameOnly`

Verifies that:

```text
Directory
```

from `FileConfiguration` is not incorporated into the result.

The strategy is responsible for **file naming**, not path construction.

# Configuration Isolation Tests

## Strategy should ignore rolling configuration

Validated by:

* `CreateActiveFileName_ShouldNotDependOnRollingConfiguration`
* `CreateRolledFileName_ShouldNotDependOnRollingConfiguration`

Changes to:

```text
FileConfiguration.Rolling
```

must not alter the generated naming format.

## Strategy should ignore archive configuration

Validated by:

* `CreateActiveFileName_ShouldNotDependOnArchiveConfiguration`
* `CreateRolledFileName_ShouldNotDependOnArchiveConfiguration`

Archival behavior belongs to the archive component and should not affect file naming.

## Strategy should ignore retention configuration

Validated by:

* `CreateActiveFileName_ShouldNotDependOnRetentionConfiguration`
* `CreateRolledFileName_ShouldNotDependOnRetentionConfiguration`

Retention policy should have no influence on generated file names.

# Edge Case Tests

## Strategy should support a very long file name

Validated by:

* `CreateActiveFileName_WithVeryLongFileName_ShouldGenerateName`

Ensures the strategy does not impose an unnecessary artificial length restriction.

## Strategy should support Unicode file names

Validated by:

* `CreateActiveFileName_WithUnicodeFileName_ShouldGenerateName`

For example:

```text
FileName = 应用程序日志
```

should be preserved in the generated file name.

## Strategy should support file names containing spaces

Validated by:

* `CreateActiveFileName_WithSpacesInFileName_ShouldGenerateName`

For example:

```text
Application Service
```

should remain valid in the generated name.

## Strategy should handle custom extension values correctly

Validated by:

* `CreateActiveFileName_WithCustomExtension_ShouldGenerateName`

Verifies that extensions such as:

```text
txt
json
log
```

are correctly incorporated.

# Test Scope

These tests validate only the public behavior of **TimestampFileNamingStrategy**.

The following responsibilities are intentionally tested separately within their respective components:

* Directory/path construction
* File creation
* File writing
* File rolling decisions
* Daily rolling logic
* Size-based rolling logic
* Archive processing
* Archive compression
* Retention cleanup
* File system interaction
* Logger configuration
* Naming strategy selection
* Custom naming strategy implementation
* Thread synchronization during rolling

The strategy should be treated as a **pure naming responsibility** and should not perform filesystem operations.

# Coverage Summary

| Area                              | Covered |
| --------------------------------- | :-----: |
| Constructor                       |    ✅    |
| Configuration usage               |    ✅    |
| Active file name generation       |    ✅    |
| Timestamp generation              |    ✅    |
| Rolled file name generation       |    ✅    |
| Rolling index handling            |    ✅    |
| Default index                     |    ✅    |
| Index validation                  |    ✅    |
| File extension handling           |    ✅    |
| Custom file names                 |    ✅    |
| Unicode / special names           |    ✅    |
| Filesystem-safe naming            |    ✅    |
| Directory isolation               |    ✅    |
| Rolling configuration isolation   |    ✅    |
| Archive configuration isolation   |    ✅    |
| Retention configuration isolation |    ✅    |
| Exception handling                |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
