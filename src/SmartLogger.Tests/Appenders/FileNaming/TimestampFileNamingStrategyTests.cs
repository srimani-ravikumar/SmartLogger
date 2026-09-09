using SmartLogger.Appenders.FileNaming;
using SmartLogger.Core;
using System.Text.RegularExpressions;

namespace SmartLogger.Tests.Appenders.FileNaming;

[TestFixture]
public class TimestampFileNamingStrategyTests
{
    private FileConfiguration _configuration = null!;
    private TimestampFileNamingStrategy _strategy = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = new FileConfiguration
        {
            FileName = "Application",
            Extension = "log",
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Timestamp,
                DateFormat = "yyyy-MM-dd-HH-mm-ss"
            }
        };

        _strategy = new TimestampFileNamingStrategy(_configuration);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidConfiguration_ShouldInitializeSuccessfully()
    {
        // Act
        var strategy = new TimestampFileNamingStrategy(_configuration);

        // Assert
        Assert.That(strategy, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TimestampFileNamingStrategy(null!));
    }

    #endregion

    #region Active File Name Tests

    [Test]
    public void CreateActiveFileName_WithConfiguredFileName_ShouldIncludeFileName()
    {
        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.StartWith("Application-"));
    }

    [Test]
    public void CreateActiveFileName_WithConfiguredExtension_ShouldUseExtension()
    {
        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.EndWith(".log"));
    }

    [Test]
    public void CreateActiveFileName_ShouldContainTimestamp()
    {
        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        var pattern =
            @"^Application-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}\.log$";

        Assert.That(
            Regex.IsMatch(result, pattern),
            Is.True,
            $"Generated file name '{result}' does not match the expected timestamp format.");
    }

    [Test]
    public void CreateActiveFileName_ShouldGenerateFilesystemSafeName()
    {
        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.Not.Contain("/"));
        Assert.That(result, Does.Not.Contain("\\"));
        Assert.That(result, Does.Not.Contain(":"));
        Assert.That(result, Does.Not.Contain("*"));
        Assert.That(result, Does.Not.Contain("?"));
        Assert.That(result, Does.Not.Contain("\""));
        Assert.That(result, Does.Not.Contain("<"));
        Assert.That(result, Does.Not.Contain(">"));
        Assert.That(result, Does.Not.Contain("|"));
    }

    [Test]
    public void CreateActiveFileName_WithCustomFileName_ShouldUseConfiguredName()
    {
        // Arrange
        _configuration.FileName = "SalesApp";

        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.StartWith("SalesApp-"));
    }

    [Test]
    public void CreateActiveFileName_WithCustomExtension_ShouldUseConfiguredExtension()
    {
        // Arrange
        _configuration.Extension = "json";

        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.EndWith(".json"));
    }

    [Test]
    public void CreateActiveFileName_WithUnicodeFileName_ShouldGenerateName()
    {
        // Arrange
        _configuration.FileName = "应用程序日志";

        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.StartWith("应用程序日志-"));
        Assert.That(result, Does.EndWith(".log"));
    }

    [Test]
    public void CreateActiveFileName_WithSpacesInFileName_ShouldGenerateName()
    {
        // Arrange
        _configuration.FileName = "Application Service";

        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.StartWith("Application Service-"));
        Assert.That(result, Does.EndWith(".log"));
    }

    [Test]
    public void CreateActiveFileName_ShouldReturnFileNameOnly()
    {
        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.Not.Contain("/"));
        Assert.That(result, Does.Not.Contain("\\"));
    }

    #endregion

    #region Rolled File Name Tests

    [Test]
    public void CreateRolledFileName_WithConfiguredFileName_ShouldIncludeFileName()
    {
        // Act
        var result = _strategy.CreateRolledFileName();

        // Assert
        Assert.That(result, Does.StartWith("Application-"));
    }

    [Test]
    public void CreateRolledFileName_WithConfiguredExtension_ShouldUseExtension()
    {
        // Act
        var result = _strategy.CreateRolledFileName();

        // Assert
        Assert.That(result, Does.EndWith(".log"));
    }

    [Test]
    public void CreateRolledFileName_ShouldContainTimestamp()
    {
        // Act
        var result = _strategy.CreateRolledFileName();

        // Assert
        var pattern =
            @"^Application-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}-\d+\.log$";

        Assert.That(
            Regex.IsMatch(result, pattern),
            Is.True,
            $"Generated rolled file name '{result}' does not match the expected format.");
    }

    [Test]
    public void CreateRolledFileName_WithPositiveIndex_ShouldIncludeIndex()
    {
        // Act
        var result = _strategy.CreateRolledFileName(2);

        // Assert
        Assert.That(result, Does.EndWith("-2.log"));
    }

    [Test]
    public void CreateRolledFileName_WithoutIndex_ShouldUseDefaultIndex()
    {
        // Act
        var resultWithoutIndex = _strategy.CreateRolledFileName();
        var resultWithZero = _strategy.CreateRolledFileName(0);

        // Assert
        Assert.That(
            resultWithoutIndex,
            Does.EndWith("-0.log"));

        Assert.That(
            resultWithZero,
            Does.EndWith("-0.log"));
    }

    [Test]
    public void CreateRolledFileName_WithZeroIndex_ShouldGenerateValidName()
    {
        // Act
        var result = _strategy.CreateRolledFileName(0);

        // Assert
        var pattern =
            @"^Application-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}-0\.log$";

        Assert.That(Regex.IsMatch(result, pattern), Is.True);
    }

    [Test]
    public void CreateRolledFileName_WithPositiveIndex_ShouldGenerateValidName()
    {
        // Act
        var result = _strategy.CreateRolledFileName(999);

        // Assert
        Assert.That(result, Does.EndWith("-999.log"));
    }

    [Test]
    public void CreateRolledFileName_WithDifferentIndexes_ShouldGenerateDifferentNames()
    {
        // Act
        var result1 = _strategy.CreateRolledFileName(1);
        var result2 = _strategy.CreateRolledFileName(2);

        // Assert
        Assert.That(result1, Is.Not.EqualTo(result2));
        Assert.That(result1, Does.EndWith("-1.log"));
        Assert.That(result2, Does.EndWith("-2.log"));
    }

    [Test]
    public void CreateRolledFileName_WithNegativeIndex_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _strategy.CreateRolledFileName(-1));
    }

    [Test]
    public void CreateRolledFileName_WithCustomFileName_ShouldUseConfiguredName()
    {
        // Arrange
        _configuration.FileName = "SalesApp";

        // Act
        var result = _strategy.CreateRolledFileName(1);

        // Assert
        Assert.That(result, Does.StartWith("SalesApp-"));
        Assert.That(result, Does.EndWith("-1.log"));
    }

    [Test]
    public void CreateRolledFileName_WithCustomExtension_ShouldUseConfiguredExtension()
    {
        // Arrange
        _configuration.Extension = "json";

        // Act
        var result = _strategy.CreateRolledFileName(1);

        // Assert
        Assert.That(result, Does.EndWith("-1.json"));
    }

    [Test]
    public void CreateRolledFileName_ShouldReturnFileNameOnly()
    {
        // Act
        var result = _strategy.CreateRolledFileName(1);

        // Assert
        Assert.That(result, Does.Not.Contain("/"));
        Assert.That(result, Does.Not.Contain("\\"));
    }

    #endregion

    #region Configuration Isolation Tests

    [Test]
    public void CreateActiveFileName_ShouldNotDependOnRollingConfiguration()
    {
        // Arrange
        _configuration.Rolling.Strategy = RollingStrategyType.Daily;
        var dailyResult = _strategy.CreateActiveFileName();

        _configuration.Rolling.Strategy = RollingStrategyType.Size;
        var sizeResult = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(
            ExtractNameWithoutTimestamp(dailyResult),
            Is.EqualTo(ExtractNameWithoutTimestamp(sizeResult)));
    }

    [Test]
    public void CreateActiveFileName_ShouldNotDependOnArchiveConfiguration()
    {
        // Arrange
        _configuration.Archive.Enabled = true;
        var enabledResult = _strategy.CreateActiveFileName();

        _configuration.Archive.Enabled = false;
        var disabledResult = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(
            ExtractNameWithoutTimestamp(enabledResult),
            Is.EqualTo(ExtractNameWithoutTimestamp(disabledResult)));
    }

    [Test]
    public void CreateActiveFileName_ShouldNotDependOnRetentionConfiguration()
    {
        // Arrange
        _configuration.Retention.RetentionDays = 30;
        var thirtyDayResult = _strategy.CreateActiveFileName();

        _configuration.Retention.RetentionDays = 90;
        var ninetyDayResult = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(
            ExtractNameWithoutTimestamp(thirtyDayResult),
            Is.EqualTo(ExtractNameWithoutTimestamp(ninetyDayResult)));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void CreateActiveFileName_WithVeryLongFileName_ShouldGenerateName()
    {
        // Arrange
        _configuration.FileName = new string('A', 200);

        // Act
        var result = _strategy.CreateActiveFileName();

        // Assert
        Assert.That(result, Does.StartWith(new string('A', 200) + "-"));
        Assert.That(result, Does.EndWith(".log"));
    }

    #endregion

    #region Helpers

    private static string ExtractNameWithoutTimestamp(string fileName)
    {
        var extensionIndex = fileName.LastIndexOf('.');

        var withoutExtension =
            extensionIndex >= 0
                ? fileName[..extensionIndex]
                : fileName;

        var timestampSeparator =
            withoutExtension.LastIndexOf('-');

        return timestampSeparator >= 0
            ? withoutExtension[..timestampSeparator]
            : withoutExtension;
    }

    #endregion
}