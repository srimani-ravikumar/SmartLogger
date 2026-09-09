using SmartLogger.Appenders.FileSystem;
using System.IO.Compression;
using System.Text;

namespace SmartLogger.Tests.Appenders.FileSystem;

[TestFixture]
public class CompressionHelperTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(
                _testDirectory,
                recursive: true);
        }
    }

    #region Existing File Tests

    [Test]
    public void Compress_WithExistingFile_ShouldCreateZipArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "Log entry");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.True);
    }

    [Test]
    public void Compress_WithExistingFile_ShouldPreserveOriginalFileNameInArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "Log entry");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        Assert.That(
            archive.Entries.Count,
            Is.EqualTo(1));

        Assert.That(
            archive.Entries[0].FullName,
            Is.EqualTo("Application.log"));
    }

    [Test]
    public void Compress_WithExistingFile_ShouldPreserveFileContents()
    {
        // Arrange
        const string content =
            "2026-09-09 INFO Application started.";

        var sourcePath = CreateFile(
            "Application.log",
            content);

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        var entry = archive.GetEntry("Application.log");

        Assert.That(entry, Is.Not.Null);

        using var reader = new StreamReader(
            entry!.Open());

        var extractedContent = reader.ReadToEnd();

        Assert.That(
            extractedContent,
            Is.EqualTo(content));
    }

    [Test]
    public void Compress_WithExistingFile_ShouldCreateZipWithExpectedExtension()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "Log entry");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        var zipPath = Path.Combine(
            _testDirectory,
            "Application.zip");

        Assert.That(File.Exists(zipPath), Is.True);
    }

    [Test]
    public void Compress_WithDifferentFileExtension_ShouldCreateZipArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.txt",
            "Text content");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        var zipPath = Path.Combine(
            _testDirectory,
            "Application.zip");

        Assert.That(File.Exists(zipPath), Is.True);
    }

    #endregion

    #region Missing File Tests

    [Test]
    public void Compress_WithNonExistentFile_ShouldDoNothing()
    {
        // Arrange
        var sourcePath = Path.Combine(
            _testDirectory,
            "Application.log");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(sourcePath), Is.False);
        Assert.That(File.Exists(zipPath), Is.False);
    }

    [Test]
    public void Compress_WithNonExistentFile_ShouldNotCreateZipArchive()
    {
        // Arrange
        var sourcePath = Path.Combine(
            _testDirectory,
            "Missing.log");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.False);
    }

    [Test]
    public void Compress_WithNonExistentFile_ShouldNotThrow()
    {
        // Arrange
        var sourcePath = Path.Combine(
            _testDirectory,
            "Missing.log");

        // Act & Assert
        Assert.DoesNotThrow(
            () => CompressionHelper.Compress(sourcePath));
    }

    #endregion

    #region Original File Tests

    [Test]
    public void Compress_WithExistingFile_ShouldDeleteOriginalFile()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "Log entry");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(
            File.Exists(sourcePath),
            Is.False);
    }

    #endregion

    #region Existing Archive Tests

    [Test]
    public void Compress_WhenZipArchiveAlreadyExists_ShouldOverwriteArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "New content");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        CreateFile(
            "Application.zip",
            "Old invalid archive content");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.True);

        using var archive = ZipFile.OpenRead(zipPath);

        Assert.That(
            archive.Entries.Count,
            Is.EqualTo(1));

        Assert.That(
            archive.Entries[0].FullName,
            Is.EqualTo("Application.log"));
    }

    [Test]
    public void Compress_WhenZipArchiveAlreadyExists_ShouldContainCurrentFileContents()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "Current log content");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Create a valid old archive.
        using (var archive = ZipFile.Open(
            zipPath,
            ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(
                "Application.log");

            using var writer = new StreamWriter(
                entry.Open());

            writer.Write("Old log content");
        }

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var newArchive = ZipFile.OpenRead(zipPath);

        var entryAfterCompression =
            newArchive.GetEntry("Application.log");

        Assert.That(
            entryAfterCompression,
            Is.Not.Null);

        using var reader =
            new StreamReader(entryAfterCompression!.Open());

        var content = reader.ReadToEnd();

        Assert.That(
            content,
            Is.EqualTo("Current log content"));
    }

    [Test]
    public void Compress_WhenZipArchiveAlreadyExists_ShouldCreateNewArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application.log",
            "New content");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        File.WriteAllText(
            zipPath,
            "Old archive content");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.True);

        Assert.DoesNotThrow(() =>
        {
            using var archive =
                ZipFile.OpenRead(zipPath);
        });
    }

    #endregion

    #region Empty File Tests

    [Test]
    public void Compress_WithEmptyFile_ShouldCreateZipArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Empty.log",
            string.Empty);

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.True);
    }

    [Test]
    public void Compress_WithEmptyFile_ShouldCreateEmptyArchiveEntry()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Empty.log",
            string.Empty);

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        var entry = archive.GetEntry("Empty.log");

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Length, Is.EqualTo(0));
    }

    [Test]
    public void Compress_WithEmptyFile_ShouldDeleteOriginalFile()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Empty.log",
            string.Empty);

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(
            File.Exists(sourcePath),
            Is.False);
    }

    #endregion

    #region File Name and Path Tests

    [Test]
    public void Compress_WithFileNameContainingSpaces_ShouldCreateArchive()
    {
        // Arrange
        var sourcePath = CreateFile(
            "Application Service.log",
            "Log entry");

        var zipPath = Path.Combine(
            _testDirectory,
            "Application Service.zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(File.Exists(zipPath), Is.True);

        using var archive = ZipFile.OpenRead(zipPath);

        Assert.That(
            archive.GetEntry("Application Service.log"),
            Is.Not.Null);
    }

    [Test]
    public void Compress_WithFileInNestedDirectory_ShouldCreateArchiveInSameDirectory()
    {
        // Arrange
        var nestedDirectory = Path.Combine(
            _testDirectory,
            "Archive",
            "2026");

        Directory.CreateDirectory(nestedDirectory);

        var sourcePath = Path.Combine(
            nestedDirectory,
            "Application.log");

        File.WriteAllText(
            sourcePath,
            "Log entry");

        var expectedZipPath = Path.Combine(
            nestedDirectory,
            "Application.zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        Assert.That(
            File.Exists(expectedZipPath),
            Is.True);
    }

    [Test]
    public void Compress_WithNestedSourceFile_ShouldStoreFileNameOnlyInArchive()
    {
        // Arrange
        var nestedDirectory = Path.Combine(
            _testDirectory,
            "Archive");

        Directory.CreateDirectory(nestedDirectory);

        var sourcePath = Path.Combine(
            nestedDirectory,
            "Application.log");

        File.WriteAllText(
            sourcePath,
            "Log entry");

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        Assert.That(
            archive.Entries.Count,
            Is.EqualTo(1));

        Assert.That(
            archive.Entries[0].FullName,
            Is.EqualTo("Application.log"));
    }

    #endregion

    #region File Content Tests

    [Test]
    public void Compress_WithBinaryContent_ShouldPreserveContent()
    {
        // Arrange
        var expectedBytes = new byte[]
        {
            0x00,
            0x01,
            0x7F,
            0x80,
            0xFE,
            0xFF
        };

        var sourcePath = Path.Combine(
            _testDirectory,
            "Application.log");

        File.WriteAllBytes(
            sourcePath,
            expectedBytes);

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        var entry = archive.GetEntry(
            "Application.log");

        Assert.That(entry, Is.Not.Null);

        using var stream = entry!.Open();
        using var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);

        Assert.That(
            memoryStream.ToArray(),
            Is.EqualTo(expectedBytes));
    }

    [Test]
    public void Compress_WithUnicodeContent_ShouldPreserveContent()
    {
        // Arrange
        const string content =
            "INFO - Application started 🚀 - 应用程序 - தமிழ்";

        var sourcePath = CreateFile(
            "Application.log",
            content);

        var zipPath = Path.ChangeExtension(
            sourcePath,
            ".zip");

        // Act
        CompressionHelper.Compress(sourcePath);

        // Assert
        using var archive = ZipFile.OpenRead(zipPath);

        var entry = archive.GetEntry(
            "Application.log");

        Assert.That(entry, Is.Not.Null);

        using var reader = new StreamReader(
            entry!.Open(),
            Encoding.UTF8);

        var extractedContent = reader.ReadToEnd();

        Assert.That(
            extractedContent,
            Is.EqualTo(content));
    }

    #endregion

    #region Helpers

    private string CreateFile(
        string fileName,
        string content)
    {
        var filePath = Path.Combine(
            _testDirectory,
            fileName);

        File.WriteAllText(
            filePath,
            content);

        return filePath;
    }

    #endregion
}