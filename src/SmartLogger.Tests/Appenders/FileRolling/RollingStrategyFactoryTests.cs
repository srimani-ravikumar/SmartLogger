using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileRolling;

[TestFixture]
public class RollingStrategyFactoryTests
{
    #region Strategy Creation Tests

    [Test]
    public void Create_WithDailyStrategy_ShouldReturnDailyRollingStrategy()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Daily);

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<DailyRollingStrategy>());
    }

    [Test]
    public void Create_WithSizeStrategy_ShouldReturnSizeRollingStrategy()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Size);

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<SizeRollingStrategy>());
    }

    [Test]
    public void Create_WithDailyStrategy_ShouldReturnRollingStrategy()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Daily);

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.InstanceOf<IRollingStrategy>());
    }

    [Test]
    public void Create_WithSizeStrategy_ShouldReturnRollingStrategy()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Size);

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.InstanceOf<IRollingStrategy>());
    }

    #endregion

    #region Configuration Propagation Tests

    [Test]
    public void Create_WithSizeStrategy_ShouldUseConfiguredConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Size);

        configuration.Rolling.MaxFileSizeMB = 25;

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.TypeOf<SizeRollingStrategy>());

        // The factory is responsible for passing the configuration
        // to SizeRollingStrategy. The actual interpretation of
        // MaxFileSizeMB belongs to SizeRollingStrategy tests.
    }

    [Test]
    public void Create_WithDailyStrategy_ShouldCreateStrategyWithoutAdditionalConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration(
            RollingStrategyType.Daily);

        // Act
        var strategy = RollingStrategyFactory.Create(configuration);

        // Assert
        Assert.That(strategy, Is.TypeOf<DailyRollingStrategy>());
    }

    #endregion

    #region Unsupported Strategy Tests

    [Test]
    public void Create_WithUnsupportedStrategy_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (RollingStrategyType)999);

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(
            () => RollingStrategyFactory.Create(configuration));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("999"));
    }

    [Test]
    public void Create_WithUnsupportedEnumValue_ShouldThrowNotSupportedException()
    {
        // Arrange
        const int unsupportedValue = 999;

        var configuration = CreateConfiguration(
            (RollingStrategyType)unsupportedValue);

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(
            () => RollingStrategyFactory.Create(configuration));

        Assert.That(exception, Is.Not.Null);
        Assert.That(
            exception!.Message,
            Does.Contain(unsupportedValue.ToString()));
    }

    #endregion

    #region Configuration Validation Tests

    [Test]
    public void Create_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => RollingStrategyFactory.Create(null!));
    }

    [Test]
    public void Create_WithNullRollingConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var configuration = new FileConfiguration
        {
            Rolling = null!
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => RollingStrategyFactory.Create(configuration));
    }

    #endregion

    #region Helpers

    private static FileConfiguration CreateConfiguration(
        RollingStrategyType strategy)
    {
        return new FileConfiguration
        {
            Rolling = new FileRollingConfiguration
            {
                Strategy = strategy
            }
        };
    }

    #endregion
}