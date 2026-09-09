# CompressionHelper Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                               |
| ------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `CompressionHelper` class, validating ZIP archive creation, original file removal, missing file handling, archive overwrite behavior, archive contents, and filesystem edge cases. |

# Objective

Validate that **CompressionHelper** correctly compresses archived log files into ZIP archives by:

* Ignoring compression requests for files that do not exist.
* Creating a ZIP archive using the source log file name.
* Producing the expected `.zip` file path.
* Preserving the original file contents inside the archive.
* Deleting the original log file after successful compression.
* Overwriting an existing ZIP archive.
* Ensuring an existing archive does not prevent successful compression.
* Handling empty files correctly.
* Handling files with different extensions correctly.
* Operating only on the supplied archive file path.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Tests use uniquely created temporary directories.
  * Each test creates its own source and archive files.
  * Temporary files and directories are deleted during test cleanup.
  * Tests use the actual `System.IO.Compression` implementation to validate real ZIP output.

# Input File Tests

## Compression should successfully compress an existing log file

Validated by:

* `Compress_WithExistingFile_ShouldCreateZipArchive`

An existing log file should result in a `.zip` archive being created.

For:

```text id="2gc4k7"
Application.log
```

the expected archive is:

```text id="u1e2rv"
Application.zip
```

## Compression should preserve the source file name inside the ZIP archive

Validated by:

* `Compress_WithExistingFile_ShouldPreserveOriginalFileNameInArchive`

The ZIP archive should contain an entry matching:

```text id="p9x9pd"
Application.log
```

rather than storing the file using an unrelated or generated name.

## Compression should preserve the original file contents

Validated by:

* `Compress_WithExistingFile_ShouldPreserveFileContents`

The content extracted from the generated ZIP entry should match the original log file contents exactly.

## Compression should create the archive using the `.zip` extension

Validated by:

* `Compress_WithExistingFile_ShouldCreateZipWithExpectedExtension`

The archive path should be generated using:

```csharp id="3n1j9g"
Path.ChangeExtension(archiveFilePath, ".zip")
```

## Compression should handle files with non-log extensions

Validated by:

* `Compress_WithDifferentFileExtension_ShouldCreateZipArchive`

The helper should not hard-code `.log` as the source extension.

For example:

```text id="4h2w3s"
Application.txt
```

should produce:

```text id="b0q4kf"
Application.zip
```

# Missing File Tests

## Compression should do nothing when the source file does not exist

Validated by:

* `Compress_WithNonExistentFile_ShouldDoNothing`

When the supplied path does not exist, the method should return without creating an archive.

## Compression should not create a ZIP archive when the source file does not exist

Validated by:

* `Compress_WithNonExistentFile_ShouldNotCreateZipArchive`

Ensures that a missing source file does not result in an empty or invalid ZIP archive.

## Compression should not throw when the source file does not exist

Validated by:

* `Compress_WithNonExistentFile_ShouldNotThrow`

The method explicitly treats a missing source file as a no-op.

# Original File Tests

## Compression should delete the original file after successful compression

Validated by:

* `Compress_WithExistingFile_ShouldDeleteOriginalFile`

After successful compression:

```text id="a2l8cf"
Application.log   → deleted
Application.zip   → exists
```

## Compression should not delete the original file when compression does not occur

Validated by:

* `Compress_WithNonExistentFile_ShouldNotDeleteOriginalFile`

A nonexistent source naturally remains absent and no additional filesystem operation should be performed.

# Existing Archive Tests

## Compression should overwrite an existing ZIP archive

Validated by:

* `Compress_WhenZipArchiveAlreadyExists_ShouldOverwriteArchive`

If:

```text id="g7flv2"
Application.zip
```

already exists, the helper should delete the existing archive and create a new one.

## Compression should replace stale archive contents

Validated by:

* `Compress_WhenZipArchiveAlreadyExists_ShouldContainCurrentFileContents`

The resulting archive must contain the contents of the current source file rather than stale data from the previous ZIP archive.

## Compression should successfully replace an existing archive

Validated by:

* `Compress_WhenZipArchiveAlreadyExists_ShouldCreateNewArchive`

The presence of an existing `.zip` file should not prevent successful compression.

# Empty File Tests

## Compression should successfully compress an empty file

Validated by:

* `Compress_WithEmptyFile_ShouldCreateZipArchive`

An empty log file is still a valid input and should produce a valid ZIP archive.

## Compression should preserve the empty file as a ZIP entry

Validated by:

* `Compress_WithEmptyFile_ShouldCreateEmptyArchiveEntry`

The resulting ZIP should contain the original file name with zero-length content.

## Compression should delete the empty source file after successful compression

Validated by:

* `Compress_WithEmptyFile_ShouldDeleteOriginalFile`

The successful compression lifecycle should be the same regardless of source file size.

# File Name and Path Tests

## Compression should preserve file names containing spaces

Validated by:

* `Compress_WithFileNameContainingSpaces_ShouldCreateArchive`

For example:

```text id="m48ypk"
Application Service.log
```

should produce a valid archive containing:

```text id="1yq3h9"
Application Service.log
```

## Compression should support nested source directories

Validated by:

* `Compress_WithFileInNestedDirectory_ShouldCreateArchiveInSameDirectory`

The ZIP file should be created beside the source file because `Path.ChangeExtension()` changes only the extension.

## Compression should not include the source directory in the ZIP entry name

Validated by:

* `Compress_WithNestedSourceFile_ShouldStoreFileNameOnlyInArchive`

The implementation uses:

```csharp id="d0b0pj"
Path.GetFileName(archiveFilePath)
```

Therefore the ZIP entry should contain only the file name rather than the full filesystem path.

For example:

```text id="fyx6zq"
D:\Logs\Archive\Application.log
```

should produce a ZIP entry:

```text id="9r0s5t"
Application.log
```

# File Content Tests

## Compression should preserve binary file contents

Validated by:

* `Compress_WithBinaryContent_ShouldPreserveContent`

The helper should correctly compress arbitrary byte content rather than assuming the source is text.

## Compression should support Unicode file contents

Validated by:

* `Compress_WithUnicodeContent_ShouldPreserveContent`

Unicode log content should remain unchanged after compression and extraction.

# Failure and Atomicity Tests

## Original file should remain when ZIP creation fails

Validated by:

* `Compress_WhenCompressionFails_ShouldNotDeleteOriginalFile`

The original file should only be deleted after successful completion of:

```csharp id="08b1mz"
CreateEntryFromFile(...)
```

This protects the source file from being lost if archive creation fails.

> **Note:** This scenario is difficult to deterministically force with the current static implementation because `ZipFile` and `File` are directly used. It should be covered at the integration/system level unless the filesystem operations become injectable.

## Existing archive should not cause source file loss when archive creation fails

Validated by:

* `Compress_WhenArchiveCreationFails_ShouldPreserveSourceFile`

The source file must remain available if the new ZIP archive cannot be successfully created.

> **Note:** Like the previous test, deterministic simulation of filesystem failures is better suited to integration testing or an abstraction around filesystem/compression operations.

# Test Scope

These tests validate only the responsibilities of **CompressionHelper**.

The following responsibilities are intentionally tested separately within their respective components:

* Archive directory selection
* Archive naming strategy
* Rolling decisions
* Retention policy
* Log file creation
* Log file writing
* Archive scheduling
* Compression configuration selection
* File cleanup orchestration
* Error logging
* Retry behavior
* Filesystem permission management

`CompressionHelper` is responsible only for:

```text
Existing source file
        ↓
Create/replace .zip archive
        ↓
Store source file in archive
        ↓
Delete source after successful compression
```

# Coverage Summary

| Area                       | Covered |
| -------------------------- | :-----: |
| Existing file compression  |    ✅    |
| ZIP creation               |    ✅    |
| ZIP extension              |    ✅    |
| Archive contents           |    ✅    |
| Content preservation       |    ✅    |
| Original file deletion     |    ✅    |
| Missing source file        |    ✅    |
| Existing ZIP overwrite     |    ✅    |
| Stale archive replacement  |    ✅    |
| Empty files                |    ✅    |
| Different extensions       |    ✅    |
| Nested directories         |    ✅    |
| File names with spaces     |    ✅    |
| File name preservation     |    ✅    |
| Binary content             |    ✅    |
| Unicode content            |    ✅    |
| Compression failure safety |    ⚠️   |
| Filesystem isolation       |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>