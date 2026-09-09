using SmartLogger.Appenders.FileNaming;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileNaming;

[TestFixture]
public class FileNamingStrategyFactoryTests
{
    #region Create Tests

    [Test]
    public void Create_WithDateStrategy_ShouldReturnDateFileNamingStrategy()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Date
            }
        };

        // Act
        var strategy = FileNamingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<DateFileNamingStrategy>());
    }

    [Test]
    public void Create_WithDateStrategy_ShouldReturnFileNamingStrategy()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Date
            }
        };

        // Act
        var strategy = FileNamingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.InstanceOf<IFileNamingStrategy>());
    }

    [Test]
    public void Create_WithDateStrategy_ShouldUseConfiguredStrategy()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            FileName = "Application",
            Extension = "log",
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Date,
                DateFormat = "yyyy-MM-dd"
            }
        };

        // Act
        var strategy = FileNamingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.TypeOf<DateFileNamingStrategy>());
    }

    #endregion

    #region Unsupported Strategy Tests

    [Test]
    public void Create_WithTimestampStrategy_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Timestamp
            }
        };

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(
            () => FileNamingStrategyFactory.Create(configuration));

        Assert.That(
            exception!.Message,
            Does.Contain("Timestamp"));
    }

    [Test]
    public void Create_WithCustomStrategy_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Custom
            }
        };

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(
            () => FileNamingStrategyFactory.Create(configuration));

        Assert.That(
            exception!.Message,
            Does.Contain("Custom"));
    }

    [Test]
    public void Create_WithUnsupportedEnumValue_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = new FileNamingConfiguration
            {
                Strategy = (FileNamingStrategyType)999
            }
        };

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(
            () => FileNamingStrategyFactory.Create(configuration));

        Assert.That(
            exception!.Message,
            Does.Contain("999"));
    }

    #endregion

    #region Configuration Validation Tests

    [Test]
    public void Create_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => FileNamingStrategyFactory.Create(null!));
    }

    [Test]
    public void Create_WithNullNamingConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Naming = null!
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => FileNamingStrategyFactory.Create(configuration));
    }

    #endregion
}