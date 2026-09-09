using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileRolling;

[TestFixture]
public class SizeRollingStrategyTests
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

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidConfiguration_ShouldInitializeSuccessfully()
    {
        // Arrange
        var configuration = CreateConfiguration(1);

        // Act
        var strategy = new SizeRollingStrategy(configuration);

        // Assert
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.InstanceOf<IRollingStrategy>());
    }

    [Test]
    public void Constructor_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => new SizeRollingStrategy(null!));
    }

    [Test]
    public void ShouldRoll_WithConfiguredMaxFileSize_ShouldUseMegabyteThreshold()
    {
        // Arrange
        var configuration = CreateConfiguration(1);
        var strategy = new SizeRollingStrategy(configuration);

        var filePath = CreateFileWithSize(1_048_575);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region File Existence Tests

    [Test]
    public void ShouldRoll_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var configuration = CreateConfiguration(1);
        var strategy = new SizeRollingStrategy(configuration);

        var filePath = Path.Combine(
            _testDirectory,
            "non-existent.log");

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WithMissingFile_ShouldReturnFalse()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = Path.Combine(
            _testDirectory,
            "Application.log");

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region File Size Threshold Tests

    [Test]
    public void ShouldRoll_WhenFileSizeIsBelowThreshold_ShouldReturnFalse()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            512 * 1024);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WhenFileSizeEqualsThreshold_ShouldReturnTrue()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WhenFileSizeExceedsThreshold_ShouldReturnTrue()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            1_048_577);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WhenFileSizeIsOneByteBelowThreshold_ShouldReturnFalse()
    {
        // Arrange
        const long threshold = 1_048_576;

        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            threshold - 1);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WhenFileSizeIsOneByteAboveThreshold_ShouldReturnTrue()
    {
        // Arrange
        const long threshold = 1_048_576;

        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            threshold + 1);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void ShouldRoll_WithDifferentMaxFileSize_ShouldUseConfiguredThreshold()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(2));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WithOneMBThreshold_ShouldUseCorrectThreshold()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WithLargeMaxFileSize_ShouldUseCorrectThreshold()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(100));

        var filePath = CreateFileWithSize(
            100L * 1024 * 1024);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WhenConfigurationChangesAfterConstruction_ShouldRetainOriginalThreshold()
    {
        // Arrange
        var configuration = CreateConfiguration(1);

        var strategy = new SizeRollingStrategy(configuration);

        // Change configuration after strategy construction.
        configuration.Rolling.MaxFileSizeMB = 10;

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        // Strategy captured the original 1 MB threshold.
        Assert.That(result, Is.True);
    }

    #endregion

    #region File State Tests

    [Test]
    public void ShouldRoll_WhenFileGrowsBetweenChecks_ShouldReflectCurrentFileSize()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            512 * 1024);

        // Act
        var firstResult = strategy.ShouldRoll(filePath);

        // Grow the existing file beyond the threshold.
        using (var stream = new FileStream(
            filePath,
            FileMode.Append,
            FileAccess.Write))
        {
            stream.WriteByte(0);
            stream.SetLength(1_048_576);
        }

        var secondResult = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(firstResult, Is.False);
        Assert.That(secondResult, Is.True);
    }

    [Test]
    public void ShouldRoll_CalledMultipleTimesBelowThreshold_ShouldReturnFalse()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            512 * 1024);

        // Act
        var firstResult = strategy.ShouldRoll(filePath);
        var secondResult = strategy.ShouldRoll(filePath);
        var thirdResult = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(firstResult, Is.False);
        Assert.That(secondResult, Is.False);
        Assert.That(thirdResult, Is.False);
    }

    [Test]
    public void ShouldRoll_CalledMultipleTimesAtOrAboveThreshold_ShouldReturnTrue()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var firstResult = strategy.ShouldRoll(filePath);
        var secondResult = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(firstResult, Is.True);
        Assert.That(secondResult, Is.True);
    }

    #endregion

    #region Active File Path Tests

    [Test]
    public void ShouldRoll_WithValidActiveFilePath_ShouldEvaluateFile()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WithEmptyFilePath_ShouldReturnFalse()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        // Act
        var result = strategy.ShouldRoll(string.Empty);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WithNonExistentFilePath_ShouldReturnFalse()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var filePath = Path.Combine(
            _testDirectory,
            "missing.log");

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Instance Isolation Tests

    [Test]
    public void ShouldRoll_WithSeparateInstances_ShouldUseTheirOwnConfiguration()
    {
        // Arrange
        var oneMbStrategy = new SizeRollingStrategy(
            CreateConfiguration(1));

        var tenMbStrategy = new SizeRollingStrategy(
            CreateConfiguration(10));

        var filePath = CreateFileWithSize(
            1_048_576);

        // Act
        var oneMbResult =
            oneMbStrategy.ShouldRoll(filePath);

        var tenMbResult =
            tenMbStrategy.ShouldRoll(filePath);

        // Assert
        Assert.That(oneMbResult, Is.True);
        Assert.That(tenMbResult, Is.False);
    }

    #endregion

    #region Invalid Configuration Tests

    [Test]
    public void ShouldRoll_WithZeroMaxFileSize_ShouldReturnTrueForExistingFile()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(0));

        var filePath = CreateFileWithSize(0);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldRoll_WithNegativeMaxFileSize_ShouldReturnTrueForExistingFile()
    {
        // Arrange
        var strategy = new SizeRollingStrategy(
            CreateConfiguration(-1));

        var filePath = CreateFileWithSize(0);

        // Act
        var result = strategy.ShouldRoll(filePath);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region Helpers

    private static FileConfiguration CreateConfiguration(
        long maxFileSizeMB)
    {
        return new FileConfiguration
        {
            Rolling = new FileRollingConfiguration
            {
                MaxFileSizeMB = maxFileSizeMB
            }
        };
    }

    private string CreateFileWithSize(long sizeInBytes)
    {
        var filePath = Path.Combine(
            _testDirectory,
            $"{Guid.NewGuid():N}.log");

        using var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.ReadWrite);

        stream.SetLength(sizeInBytes);

        return filePath;
    }

    #endregion
}