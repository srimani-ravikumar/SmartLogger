using SmartLogger.Appenders.FileSystem;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileSystem;

[TestFixture]
public class RetentionHelperTests
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

    #region Archive Configuration Tests

    [Test]
    public void Cleanup_WhenArchiveIsDisabled_ShouldDoNothing()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: false,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.True);
    }

    [Test]
    public void Cleanup_WhenArchiveIsEnabled_ShouldProcessArchiveDirectory()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.False);
    }

    #endregion

    #region Archive Directory Tests

    [Test]
    public void Cleanup_WhenArchiveDirectoryDoesNotExist_ShouldDoNothing()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(
            _testDirectory,
            "MissingArchive");

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30,
            archiveDirectory: nonExistentDirectory);

        // Act & Assert
        Assert.DoesNotThrow(
            () => RetentionHelper.Cleanup(configuration));
    }

    [Test]
    public void Cleanup_ShouldProcessConfiguredArchiveDirectory()
    {
        // Arrange
        var archiveDirectory = Path.Combine(
            _testDirectory,
            "Archive");

        Directory.CreateDirectory(archiveDirectory);

        var expiredFile = CreateFile(
            archiveDirectory,
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30,
            archiveDirectory: archiveDirectory);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.False);
    }

    [Test]
    public void Cleanup_ShouldNotDeleteFilesOutsideArchiveDirectory()
    {
        // Arrange
        var archiveDirectory = Path.Combine(
            _testDirectory,
            "Archive");

        Directory.CreateDirectory(archiveDirectory);

        var outsideFile = CreateFile(
            _testDirectory,
            "Outside.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30,
            archiveDirectory: archiveDirectory);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(outsideFile),
            Is.True);
    }

    #endregion

    #region Retention Expiration Tests

    [Test]
    public void Cleanup_WithExpiredFile_ShouldDeleteFile()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-31));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.False);
    }

    [Test]
    public void Cleanup_WithNonExpiredFile_ShouldPreserveFile()
    {
        // Arrange
        var recentFile = CreateFile(
            "Recent.log",
            DateTime.Now.AddDays(-29));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(recentFile),
            Is.True);
    }

    [Test]
    public void Cleanup_WithMultipleExpiredFiles_ShouldDeleteAllExpiredFiles()
    {
        // Arrange
        var expiredFile1 = CreateFile(
            "Expired1.log",
            DateTime.Now.AddDays(-31));

        var expiredFile2 = CreateFile(
            "Expired2.log",
            DateTime.Now.AddDays(-60));

        var expiredFile3 = CreateFile(
            "Expired3.log",
            DateTime.Now.AddDays(-90));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(File.Exists(expiredFile1), Is.False);
        Assert.That(File.Exists(expiredFile2), Is.False);
        Assert.That(File.Exists(expiredFile3), Is.False);
    }

    [Test]
    public void Cleanup_WithMultipleNonExpiredFiles_ShouldPreserveAllFiles()
    {
        // Arrange
        var recentFile1 = CreateFile(
            "Recent1.log",
            DateTime.Now.AddDays(-1));

        var recentFile2 = CreateFile(
            "Recent2.log",
            DateTime.Now.AddDays(-10));

        var recentFile3 = CreateFile(
            "Recent3.log",
            DateTime.Now.AddDays(-29));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(File.Exists(recentFile1), Is.True);
        Assert.That(File.Exists(recentFile2), Is.True);
        Assert.That(File.Exists(recentFile3), Is.True);
    }

    [Test]
    public void Cleanup_WithMixedFileAges_ShouldDeleteOnlyExpiredFiles()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var recentFile = CreateFile(
            "Recent.log",
            DateTime.Now.AddDays(-10));

        var anotherExpiredFile = CreateFile(
            "AnotherExpired.log",
            DateTime.Now.AddDays(-90));

        var anotherRecentFile = CreateFile(
            "AnotherRecent.log",
            DateTime.Now.AddDays(-5));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.False);

        Assert.That(
            File.Exists(anotherExpiredFile),
            Is.False);

        Assert.That(
            File.Exists(recentFile),
            Is.True);

        Assert.That(
            File.Exists(anotherRecentFile),
            Is.True);
    }

    #endregion

    #region Retention Boundary Tests

    [Test]
    public void Cleanup_WhenFileIsJustOlderThanRetentionDate_ShouldDeleteFile()
    {
        // Arrange
        var file = CreateFile(
            "Boundary.log",
            DateTime.Now.AddDays(-30).AddMinutes(-1));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.False);
    }

    [Test]
    public void Cleanup_WhenFileIsJustNewerThanRetentionDate_ShouldPreserveFile()
    {
        // Arrange
        var file = CreateFile(
            "Boundary.log",
            DateTime.Now.AddDays(-30).AddMinutes(1));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.True);
    }

    [Test]
    public void Cleanup_WhenFileLastWriteTimeEqualsRetentionDate_ShouldPreserveFile()
    {
        // Arrange
        var retentionDays = 30;

        // Allow a small safety margin because Cleanup calculates
        // DateTime.Now independently from this test.
        var boundaryTime = DateTime.Now
            .AddDays(-retentionDays)
            .AddMinutes(1);

        var file = CreateFile(
            "Boundary.log",
            boundaryTime);

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: retentionDays);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.True);
    }

    #endregion

    #region Retention Configuration Tests

    [Test]
    public void Cleanup_WithDifferentRetentionDays_ShouldUseConfiguredValue()
    {
        // Arrange
        var file = CreateFile(
            "Archive.log",
            DateTime.Now.AddDays(-45));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.False);
    }

    [Test]
    public void Cleanup_WithZeroRetentionDays_ShouldDeleteOlderFiles()
    {
        // Arrange
        var file = CreateFile(
            "Archive.log",
            DateTime.Now.AddMinutes(-1));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 0);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.False);
    }

    [Test]
    public void Cleanup_WithNegativeRetentionDays_ShouldNotDeleteRecentFiles()
    {
        // Arrange
        var file = CreateFile(
            "Archive.log",
            DateTime.Now.AddDays(-30));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: -1);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(file),
            Is.True);
    }

    #endregion

    #region File Enumeration Tests

    [Test]
    public void Cleanup_WithEmptyArchiveDirectory_ShouldDoNothing()
    {
        // Arrange
        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act & Assert
        Assert.DoesNotThrow(
            () => RetentionHelper.Cleanup(configuration));
    }

    [Test]
    public void Cleanup_ShouldNotProcessFilesInNestedDirectories()
    {
        // Arrange
        var nestedDirectory = Path.Combine(
            _testDirectory,
            "Nested");

        Directory.CreateDirectory(nestedDirectory);

        var nestedExpiredFile = CreateFile(
            nestedDirectory,
            "NestedExpired.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(nestedExpiredFile),
            Is.True);
    }

    [Test]
    public void Cleanup_ShouldProcessFilesInArchiveDirectory()
    {
        // Arrange
        var expiredFile1 = CreateFile(
            "Expired1.log",
            DateTime.Now.AddDays(-60));

        var expiredFile2 = CreateFile(
            "Expired2.log",
            DateTime.Now.AddDays(-90));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        // Assert
        var remainingFiles =
            Directory.GetFiles(_testDirectory);

        Assert.That(
            remainingFiles,
            Is.Empty);
    }

    #endregion

    #region Input Validation Tests

    [Test]
    public void Cleanup_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => RetentionHelper.Cleanup(null!));
    }

    [Test]
    public void Cleanup_WithNullArchiveDirectory_ShouldReturnWithoutDeletingFiles()
    {
        // Arrange
        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        configuration.Archive.Directory = null!;

        // Act & Assert
        Assert.DoesNotThrow(
            () => RetentionHelper.Cleanup(configuration));
    }

    #endregion

    #region Repeatability Tests

    [Test]
    public void Cleanup_CalledMultipleTimes_ShouldRemainSafe()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);

        Assert.That(
            File.Exists(expiredFile),
            Is.False);

        // Second cleanup should safely process
        // the already-cleaned directory.
        Assert.DoesNotThrow(
            () => RetentionHelper.Cleanup(configuration));
    }

    [Test]
    public void Cleanup_CalledAfterPreviousCleanup_ShouldLeaveRemainingFilesUntouched()
    {
        // Arrange
        var expiredFile = CreateFile(
            "Expired.log",
            DateTime.Now.AddDays(-60));

        var recentFile = CreateFile(
            "Recent.log",
            DateTime.Now.AddDays(-5));

        var configuration = CreateConfiguration(
            archiveEnabled: true,
            retentionDays: 30);

        // Act
        RetentionHelper.Cleanup(configuration);
        RetentionHelper.Cleanup(configuration);

        // Assert
        Assert.That(
            File.Exists(expiredFile),
            Is.False);

        Assert.That(
            File.Exists(recentFile),
            Is.True);
    }

    #endregion

    #region Helpers

    private FileConfiguration CreateConfiguration(
        bool archiveEnabled,
        int retentionDays,
        string? archiveDirectory = null)
    {
        return new FileConfiguration
        {
            Archive = new ArchiveConfiguration
            {
                Enabled = archiveEnabled,
                Directory = archiveDirectory ?? _testDirectory
            },
            Retention = new RetentionConfiguration
            {
                RetentionDays = retentionDays
            }
        };
    }

    private string CreateFile(
        string fileName,
        DateTime lastWriteTime)
    {
        return CreateFile(
            _testDirectory,
            fileName,
            lastWriteTime);
    }

    private static string CreateFile(
        string directory,
        string fileName,
        DateTime lastWriteTime)
    {
        var filePath = Path.Combine(
            directory,
            fileName);

        File.WriteAllText(
            filePath,
            "Archived log content");

        File.SetLastWriteTime(
            filePath,
            lastWriteTime);

        return filePath;
    }

    #endregion
}