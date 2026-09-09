# FileLifecycleManager Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                        |
| ------- | ---------- | ------- | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Unit test plan covering active-file lifecycle, writing, rolling, archiving, compression, retention, directory management, and concurrency behavior |

# Objective

Validate that `FileLifecycleManager` correctly manages the complete lifecycle of an active log file:

* Creates required directories and the initial active log file.
* Writes valid formatted messages to the active file.
* Ignores null, empty, and whitespace-only messages.
* Recreates a missing active file when required.
* Evaluates rolling only when a rolling strategy is configured.
* Archives the active file when rolling is required.
* Generates collision-free archive paths using the naming strategy.
* Compresses archives according to archive configuration.
* Invokes retention cleanup after successful rolling.
* Creates a fresh active file after rolling.
* Maintains lifecycle consistency under concurrent writes.

# Test Environment

* .NET
* NUnit
* Moq
* Real temporary filesystem directories/files for file lifecycle behavior
* Mock `IRollingStrategy`
* Mock `IFileNamingStrategy`
* `[SetUp]` creates isolated temporary directories
* `[TearDown]` removes temporary directories and files
* Tests must not depend on the machine's existing `Logs` directory or filesystem state

# Constructor Creates a Valid Initial File Lifecycle

**Requirement:**Constructor must validate dependencies, initialize directories, and create a fresh active file.

**Validated by:**

* `Constructor_WithValidConfiguration_CreatesActiveDirectory`
* `Constructor_WithArchiveEnabled_CreatesArchiveDirectory`
* `Constructor_WithArchiveDisabled_DoesNotRequireArchiveDirectory`
* `Constructor_WithValidConfiguration_CreatesInitialActiveFile`
* `Constructor_CreatesActiveFileUsingNamingStrategy`
* `Constructor_WithNullConfiguration_ThrowsArgumentNullException`
* `Constructor_WithNullNamingStrategy_ThrowsArgumentNullException`
* `Constructor_WithNullRollingStrategy_AllowsConstruction`

**Notes:**

* `IRollingStrategy` is explicitly nullable and therefore `null` must be supported.
* The active file must exist immediately after construction.
* Existing active file behavior should be validated separately because `CreateFreshActiveFile()` only creates the file when it does not already exist.

# Write Ignores Invalid Messages

**Requirement:**Invalid formatted messages must not modify the active log file.

**Validated by:**

* `Write_WithNullMessage_DoesNotWrite`
* `Write_WithEmptyMessage_DoesNotWrite`
* `Write_WithWhitespaceMessage_DoesNotWrite`
* `Write_WithNewLineOnlyMessage_DoesNotWrite`

**Notes:**

Current implementation uses `string.IsNullOrWhiteSpace()`.

# Write Appends Formatted Messages

**Requirement:**Valid messages must be appended to the active file without overwriting existing content.

**Validated by:**

* `Write_WithValidMessage_AppendsMessageToActiveFile`
* `Write_MultipleMessages_AppendsAllMessagesInOrder`
* `Write_PreservesExistingActiveFileContent`
* `Write_AppendsEnvironmentNewLineAfterMessage`

**Notes:**

The lifecycle manager owns the newline behavior by appending `Environment.NewLine`.

# Write Recreates a Missing Active File

**Requirement:**If the active file disappears after construction, the next valid write must recreate it before writing.

**Validated by:**

* `Write_WhenActiveFileIsMissing_RecreatesActiveFile`
* `Write_WhenActiveFileIsMissing_WritesMessageToNewFile`

**Notes:**

This validates the `EnsureActiveFile()` responsibility.

# Rolling Is Not Evaluated Without a Rolling Strategy

**Requirement:**A `null` rolling strategy must disable rolling checks.

**Validated by:**

* `Write_WithNullRollingStrategy_DoesNotAttemptRolling`
* `Write_WithNullRollingStrategy_AppendsToActiveFile`

**Notes:**

`IRollingStrategy.ShouldRoll()` must never be invoked when the strategy is `null`.

# Rolling Strategy Determines Whether Archiving Occurs

**Requirement:**The lifecycle manager must delegate the rolling decision to `IRollingStrategy`.

**Validated by:**

* `Write_WhenRollingStrategyReturnsFalse_DoesNotArchive`
* `Write_WhenRollingStrategyReturnsFalse_ContinuesWritingToActiveFile`
* `Write_WhenRollingStrategyReturnsTrue_ArchivesActiveFile`
* `Write_WhenRollingStrategyReturnsTrue_CreatesFreshActiveFile`
* `Write_WhenRollingStrategyReturnsTrue_CallsRollingStrategyWithActiveFilePath`

# Active File Is Archived During Rolling

**Requirement:** When rolling is required, the current active file must be moved to the archive directory.

** Validated by:**

* `Rolling_MovesActiveFileToArchiveDirectory`
* `Rolling_RemovesOriginalActiveFile`
* `Rolling_PreservesArchivedFileContent`
* `Rolling_UsesRolledFileNameFromNamingStrategy`

**Notes:**

The test should use real temporary filesystem paths because `File.Move()` is part of the behavior under test.

# Archive File Names Handle Collisions

**Requirement:** Archive file names must be unique when an archive with the generated name already exists.

** Validated by:**

* `Rolling_WhenArchiveNameIsAvailable_UsesIndexZero`
* `Rolling_WhenArchiveNameExists_UsesNextAvailableIndex`
* `Rolling_WhenMultipleArchiveNamesExist_ContinuesUntilAvailable`
* `Rolling_UsesIncrementingArchiveIndexStartingFromZero`

**Notes:**

`CreateUniqueArchiveFilePath()` starts at index `0` and increments until an unused path is found.

# Archive Compression Follows Configuration

** Requirement:** Archived files must be compressed only when both archive and compression are enabled.

** Validated by:**

* `Rolling_WhenArchiveAndCompressionEnabled_CreatesZipArchive`
* `Rolling_WhenArchiveEnabledButCompressionDisabled_PreservesUncompressedArchive`
* `Rolling_WhenArchiveDisabled_DoesNotCompressArchive`

**Notes:**

Compression behavior itself belongs to `CompressionHelper` this class only verifies that the lifecycle manager invokes it according to configuration.

# Retention Cleanup Occurs After Rolling

**Requirement:**Retention cleanup must execute after an archive has been created.

**Validated by:**

* `Rolling_InvokesRetentionCleanupAfterArchiving`
* `Rolling_WithCompressionEnabled_PerformsCleanupAfterCompression`
* `Rolling_WhenNoRollOccurs_DoesNotInvokeRetentionCleanup`

**Notes:**

Because `RetentionHelper.Cleanup()` is static, exact invocation verification is difficult without introducing an abstraction. Where necessary, validate the observable filesystem result instead.

# Fresh Active File Is Created After Rolling

**Requirement:**A new active file must be available after the previous active file has been archived.

**Validated by:**

* `Rolling_CreatesNewActiveFile`
* `Rolling_NewActiveFileUsesNamingStrategy`
* `Rolling_NewActiveFileIsEmpty`
* `Rolling_NewWritesGoToFreshActiveFile`

# Complete Rolling Lifecycle Is Performed in Correct Order

**Requirement:**A successful roll must perform the lifecycle in this order:

```text
Detect roll
    ↓
Archive active file
    ↓
Compress archive if enabled
    ↓
Cleanup expired archives
    ↓
Create fresh active file
    ↓
Write new message
```

**Validated by: **

* `Rolling_PerformsCompleteArchiveCleanupAndRecreationLifecycle`
* `Rolling_NewMessageIsWrittenToFreshActiveFile`
* `Rolling_PreviousMessageExistsOnlyInArchivedFile`

**Notes:**

The test should focus primarily on observable state rather than private -method invocation.

# Directory Management Is Correct

* *Requirement:**Required directories must exist before files are created.

**Validated by:**

* `Constructor_CreatesMissingActiveDirectory`
* `Constructor_WithArchiveEnabled_CreatesMissingArchiveDirectory`
* `Constructor_WithExistingDirectories_DoesNotFail`
* `Constructor_WithNestedActiveDirectory_CreatesEntireDirectoryTree`
* `Constructor_WithNestedArchiveDirectory_CreatesEntireDirectoryTree`

# Existing Active File Is Preserved During Construction

**Requirement:**Construction must not overwrite an already existing active file.

**Validated by:**

* `Constructor_WhenActiveFileAlreadyExists_PreservesExistingFile`
* `Constructor_WhenActiveFileAlreadyExists_PreservesExistingContent`

**Notes:**

Current implementation checks `File.Exists()` before calling `File.Create()`.

# Multiple Lifecycle Operations Remain Consistent

**Requirement:**Repeated writes and rolls must maintain correct active/archive state.

**Validated by:**

* `MultipleWrites_PreserveAllMessages`
* `MultipleRolls_CreateSeparateArchives`
* `MultipleRolls_CreateFreshActiveFileEachTime`
* `MultipleRolls_PreserveArchivedContents`
* `MultipleRolls_UseNextAvailableArchiveNameWhenCollisionOccurs`

# Concurrent Writes Are Serialized

**Requirement:**Concurrent calls to `Write()` must be protected by `_syncRoot` so that lifecycle transitions and writes are not performed concurrently.

**Validated by:**

* `ConcurrentWrites_DoNotCorruptActiveFile`
* `ConcurrentWrites_PreserveAllMessages`
* `ConcurrentWrites_DoNotCauseConcurrentLifecycleFailures`
* `ConcurrentRollingWrites_MaintainValidActiveAndArchiveState`

**Notes:**

Do not make assertions about the exact ordering of messages because concurrent invocation does not guarantee caller ordering. Validate message presence/count and filesystem consistency instead.

# Failure Behavior

**Requirement:**Filesystem failures must not be silently converted into an incorrect lifecycle state unless explicitly handled by the implementation.

**Validated by:**

* `Write_WhenActiveDirectoryBecomesUnavailable_PropagatesFilesystemException`
* `Rolling_WhenArchiveMoveFails_DoesNotCreateFalseSuccessfulArchive`
* `Rolling_WhenArchiveCompressionFails_DoesNotSilentlyReportSuccessfulCompression`

**Notes:**

These tests should reflect the **current implementation**. `FileLifecycleManager` does not contain general exception handling, so filesystem/compression exceptions are expected to propagate.

Deterministic failure injection would be significantly easier if filesystem operations were abstracted behind an `IFileSystem` abstraction. That is a potential future design improvement rather than something to introduce solely for testing.

# Important Current-Implementation Edge Case

**Requirement:** Rolling behavior must expose any configuration inconsistency rather than hiding it.

**Validated by:**

* `Rolling_WhenArchiveDisabled_CurrentImplementationAttemptsArchiveMove`

**Important note:**

There is a potential design issue in the current implementation:

```csharp
EnsureDirectoriesExist();
```

only creates `_archiveDirectory` when:

```csharp
_configuration.Archive.Enabled
```

is `true`.

However, `EnsureActiveFile()` can still call:

```csharp
ArchiveActiveFile();
```

when rolling occurs, and `ArchiveActiveFile()` always executes:

```csharp
File.Move(_activeFilePath, archivePath);
```

Therefore, **rolling with `Archive.Enabled == false` can fail if the archive directory does not already exist**.

The test should document this as current behavior rather than hiding it. If the intended contract is *"rolling always requires an archive"*, then the configuration should enforce that relationship. If `Archive.Enabled == false` is supposed to disable archiving entirely, the lifecycle flow needs to change.

# Test Scope

### In Scope

* Constructor validation
* Directory creation
* Active file creation
* Active file recreation
* Message validation
* Message appending
* Rolling strategy delegation
* Active file rolling
* Archive path generation
* Archive collision handling
* Compression decision
* Retention cleanup orchestration
* Fresh active file creation
* Repeated lifecycle operations
* Concurrent writes
* Observable filesystem consistency

### Out of Scope

* `IRollingStrategy` internal algorithms
* `IFileNamingStrategy` internal algorithms
* Compression algorithm correctness
* Retention-date calculation
* Logger formatting
* Log levels
* Logger configuration validation
* Scheduling/background rolling
* External storage
* Distributed coordination
* Performance/load benchmarking

# Coverage Summary

| Area                                | Coverage     |
| ----------------------------------- | ------------ |
| Constructor & Dependency Validation | Full         |
| Directory Management                | Full         |
| Active File Creation                | Full         |
| Message Validation                  | Full         |
| Message Writing                     | Full         |
| Missing Active File Recovery        | Full         |
| Rolling Strategy Integration        | Full         |
| Archive Creation                    | Full         |
| Archive Collision Handling          | Full         |
| Compression Orchestration           | Full         |
| Retention Orchestration             | Full         |
| Fresh Active File Creation          | Full         |
| Repeated Lifecycle Operations       | Full         |
| Concurrency                         | Targeted     |
| Failure Behavior                    | Targeted     |
| External Collaborator Internals     | Out of Scope |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>