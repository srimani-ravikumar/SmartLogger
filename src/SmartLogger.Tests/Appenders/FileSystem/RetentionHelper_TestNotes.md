# RetentionHelper Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                              |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `RetentionHelper` class, validating retention configuration, archive directory handling, expiration detection, file deletion, boundary conditions, and cleanup failure isolation. |

# Objective

Validate that **RetentionHelper** correctly removes expired archived log files according to the configured retention policy by:

* Skipping cleanup when archival is disabled.
* Skipping cleanup when the archive directory does not exist.
* Calculating the retention cutoff from the current time.
* Deleting files older than the configured retention period.
* Preserving files newer than the retention cutoff.
* Handling the retention boundary correctly.
* Processing multiple archived files independently.
* Leaving non-expired files untouched.
* Ignoring individual file deletion failures.
* Continuing cleanup when one archived file cannot be deleted.
* Operating only on files directly contained within the configured archive directory.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a unique temporary archive directory.
  * Test files are created exclusively inside the temporary directory.
  * File timestamps are explicitly controlled using `File.SetLastWriteTime`.
  * Temporary directories and files are removed during test cleanup.
  * Tests must not interact with the application's real archive directory.

# Archive Configuration Tests

## Cleanup should skip processing when archival is disabled

Validated by:

* `Cleanup_WhenArchiveIsDisabled_ShouldDoNothing`

When:

```csharp
configuration.Archive.Enabled = false;
```

the method should return immediately without inspecting or modifying the archive directory.

## Cleanup should process files when archival is enabled

Validated by:

* `Cleanup_WhenArchiveIsEnabled_ShouldProcessArchiveDirectory`

When archival is enabled and the archive directory exists, expired files should be evaluated according to the retention policy.

# Archive Directory Tests

## Cleanup should do nothing when the archive directory does not exist

Validated by:

* `Cleanup_WhenArchiveDirectoryDoesNotExist_ShouldDoNothing`

The method explicitly checks:

```csharp
Directory.Exists(archiveDirectory)
```

and should return without throwing when the directory is missing.

## Cleanup should use the configured archive directory

Validated by:

* `Cleanup_ShouldProcessConfiguredArchiveDirectory`

Files should be evaluated only within:

```csharp
configuration.Archive.Directory
```

and not from an unrelated directory.

## Cleanup should not process files outside the configured archive directory

Validated by:

* `Cleanup_ShouldNotDeleteFilesOutsideArchiveDirectory`

An expired file located outside the configured archive directory must remain untouched.

# Retention Expiration Tests

## Cleanup should delete files older than the retention period

Validated by:

* `Cleanup_WithExpiredFile_ShouldDeleteFile`

For example, with:

```text
RetentionDays = 30
```

a file older than 30 days should be deleted.

## Cleanup should preserve files newer than the retention period

Validated by:

* `Cleanup_WithNonExpiredFile_ShouldPreserveFile`

A file newer than the calculated retention cutoff must remain untouched.

## Cleanup should delete multiple expired files

Validated by:

* `Cleanup_WithMultipleExpiredFiles_ShouldDeleteAllExpiredFiles`

Every expired file directly contained in the archive directory should be deleted.

## Cleanup should preserve multiple non-expired files

Validated by:

* `Cleanup_WithMultipleNonExpiredFiles_ShouldPreserveAllFiles`

Non-expired files must remain untouched even when other files in the same directory are deleted.

## Cleanup should process expired and non-expired files independently

Validated by:

* `Cleanup_WithMixedFileAges_ShouldDeleteOnlyExpiredFiles`

For example:

```text
OldArchive.log       → deleted
RecentArchive.log    → preserved
AnotherOldArchive.log → deleted
RecentArchive2.log  → preserved
```

# Retention Boundary Tests

## Cleanup should preserve a file exactly at the retention boundary

Validated by:

* `Cleanup_WhenFileLastWriteTimeEqualsRetentionDate_ShouldPreserveFile`

The implementation uses:

```csharp
fileInfo.LastWriteTime < retentionDate
```

Therefore a file whose timestamp is exactly equal to the calculated retention date should **not** be deleted.

> **Note:** Because the cutoff is calculated using `DateTime.Now`, exact boundary testing can be sensitive to clock precision. The test should provide a sufficiently controlled timestamp or use a small tolerance around the boundary.

## Cleanup should delete a file immediately before the retention boundary

Validated by:

* `Cleanup_WhenFileIsJustOlderThanRetentionDate_ShouldDeleteFile`

A file slightly older than the retention cutoff should satisfy:

```text
LastWriteTime < retentionDate
```

and therefore be deleted.

## Cleanup should preserve a file immediately after the retention boundary

Validated by:

* `Cleanup_WhenFileIsJustNewerThanRetentionDate_ShouldPreserveFile`

A file slightly newer than the retention cutoff should not be deleted.

# Retention Configuration Tests

## Cleanup should respect the configured retention period

Validated by:

* `Cleanup_WithDifferentRetentionDays_ShouldUseConfiguredValue`

Changing:

```csharp
configuration.Retention.RetentionDays
```

should change which files qualify as expired.

## Cleanup should support zero retention days

Validated by:

* `Cleanup_WithZeroRetentionDays_ShouldDeleteOlderFiles`

With:

```text
RetentionDays = 0
```

the retention cutoff is effectively the current time.

Files with a sufficiently earlier `LastWriteTime` should be deleted.

> **Note:** Whether zero is a valid business configuration should ultimately be enforced by configuration validation.

## Cleanup should handle negative retention days according to the current implementation

Validated by:

* `Cleanup_WithNegativeRetentionDays_ShouldNotDeleteRecentFiles`

The current implementation does not validate negative values.

A negative retention period moves the calculated cutoff into the future:

```csharp
DateTime.Now.AddDays(-(-N))
```

Therefore normal existing files will generally not satisfy the expiration condition.

> **Note:** Negative retention values are logically invalid and should ideally be rejected by configuration validation.

# File Type and Enumeration Tests

## Cleanup should process files directly contained in the archive directory

Validated by:

* `Cleanup_ShouldProcessFilesInArchiveDirectory`

The implementation uses:

```csharp
Directory.GetFiles(archiveDirectory)
```

which enumerates files directly in the configured directory.

## Cleanup should not recursively process nested directories

Validated by:

* `Cleanup_ShouldNotProcessFilesInNestedDirectories`

An expired file inside a subdirectory should not be deleted because the current implementation does not use recursive enumeration.

## Cleanup should handle an empty archive directory

Validated by:

* `Cleanup_WithEmptyArchiveDirectory_ShouldDoNothing`

An existing archive directory containing no files should result in no action and no exception.

# Cleanup Failure Tests

## Cleanup should ignore an individual file cleanup failure

Validated by:

* `Cleanup_WhenFileDeletionFails_ShouldIgnoreFailure`

The implementation deliberately catches exceptions around individual file processing.

A failure to delete one archive must not escape from `Cleanup()`.

## Cleanup should continue processing other files after a deletion failure

Validated by:

* `Cleanup_WhenOneFileDeletionFails_ShouldContinueProcessingRemainingFiles`

If one expired file cannot be deleted, subsequent eligible files should still be evaluated.

> **Note:** Deterministically forcing `FileInfo.Delete()` to fail is difficult with the current direct filesystem implementation. This behavior is better validated through an integration test using controlled filesystem permissions/locks, or after introducing a filesystem abstraction.

## Cleanup should not allow cleanup failures to propagate to the logging system

Validated by:

* `Cleanup_WhenFileCleanupFails_ShouldNotThrow`

The retention helper must not allow an individual filesystem failure to cause the logging pipeline to fail.

# Input Validation Tests

## Cleanup should handle a null configuration according to the current implementation

Validated by:

* `Cleanup_WithNullConfiguration_ShouldThrowNullReferenceException`

The current implementation directly accesses:

```csharp
configuration.Archive.Enabled
```

Therefore a null configuration currently results in:

```text
NullReferenceException
```

> **Note:** A future enhancement could explicitly validate the argument and throw `ArgumentNullException`.

## Cleanup should handle a null archive directory according to the current implementation

Validated by:

* `Cleanup_WithNullArchiveDirectory_ShouldReturnWithoutDeletingFiles`

The behavior depends on the underlying `Directory.Exists()` implementation. Since `Directory.Exists(null)` returns `false`, cleanup should terminate without processing files.

# State and Repeatability Tests

## Cleanup should be safe to invoke multiple times

Validated by:

* `Cleanup_CalledMultipleTimes_ShouldRemainSafe`

After expired files have been removed, subsequent cleanup operations should not throw because those files no longer exist.

## Cleanup should not affect already cleaned files

Validated by:

* `Cleanup_CalledAfterPreviousCleanup_ShouldLeaveRemainingFilesUntouched`

A second cleanup invocation should continue to preserve files that have not expired.

# Test Scope

These tests validate only the responsibilities of **RetentionHelper**.

The following responsibilities are intentionally tested separately within their respective components:

* Archive creation
* Archive compression
* Archive directory creation
* File naming
* File rolling decisions
* Rolling orchestration
* Retention scheduling
* Configuration validation
* Logging cleanup errors
* Retry policies
* Filesystem abstraction
* Application lifecycle management

`RetentionHelper` is responsible only for:

```text
Archive directory
       ↓
Enumerate archived files
       ↓
Calculate retention cutoff
       ↓
Identify expired files
       ↓
Delete expired files
       ↓
Ignore individual cleanup failures
```

# Implementation Considerations

The current implementation directly depends on:

```csharp
DateTime.Now
Directory.GetFiles()
FileInfo
```

This creates two important testing considerations:

1. **Time is not injectable**, making exact retention-boundary tests somewhat sensitive to execution time.
2. **Filesystem operations are not injectable**, making deterministic failure simulation difficult.

For a production-grade framework, a future design could introduce a `TimeProvider` for deterministic retention calculations.

However, I would **not introduce abstractions solely to make these tests easier** unless the framework's broader architecture already benefits from them.

More importantly, invalid values such as:

```text
RetentionDays < 0
```

should ideally be rejected during **configuration validation**, rather than making `RetentionHelper` responsible for configuration policy.

# Coverage Summary

| Area                             | Covered |
| -------------------------------- | :-----: |
| Archive enabled                  |    ✅    |
| Archive disabled                 |    ✅    |
| Missing archive directory        |    ✅    |
| Configured archive directory     |    ✅    |
| Directory isolation              |    ✅    |
| Expired files                    |    ✅    |
| Non-expired files                |    ✅    |
| Multiple expired files           |    ✅    |
| Multiple non-expired files       |    ✅    |
| Mixed file ages                  |    ✅    |
| Retention boundary               |    ✅    |
| Zero retention                   |    ✅    |
| Negative retention               |    ⚠️   |
| Nested directories               |    ✅    |
| Empty archive directory          |    ✅    |
| Cleanup failure isolation        |    ⚠️   |
| Continued processing after error |    ⚠️   |
| Null configuration               |    ✅    |
| Null archive directory           |    ✅    |
| Repeated cleanup                 |    ✅    |
| Filesystem isolation             |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>