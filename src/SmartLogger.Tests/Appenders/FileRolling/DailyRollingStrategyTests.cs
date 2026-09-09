using SmartLogger.Appenders.FileRolling;

namespace SmartLogger.Tests.Appenders.FileRolling;

[TestFixture]
public class DailyRollingStrategyTests
{
    private DailyRollingStrategy _strategy = null!;

    [SetUp]
    public void SetUp()
    {
        _strategy = new DailyRollingStrategy();
    }

    #region Initial State Tests

    [Test]
    public void ShouldRoll_OnInitialEvaluation_ShouldReturnFalse()
    {
        // Act
        var result = _strategy.ShouldRoll("Logs/Application.log");

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Daily Boundary Detection Tests

    [Test]
    public void ShouldRoll_OnSameDay_ShouldReturnFalse()
    {
        // Act
        var firstResult = _strategy.ShouldRoll("Logs/Application.log");
        var secondResult = _strategy.ShouldRoll("Logs/Application.log");

        // Assert
        Assert.That(firstResult, Is.False);
        Assert.That(secondResult, Is.False);
    }

    [Test]
    public void ShouldRoll_CalledMultipleTimesOnSameDay_ShouldAlwaysReturnFalse()
    {
        // Act
        var results = new[]
        {
            _strategy.ShouldRoll("Logs/Application.log"),
            _strategy.ShouldRoll("Logs/Application.log"),
            _strategy.ShouldRoll("Logs/Application.log"),
            _strategy.ShouldRoll("Logs/Application.log")
        };

        // Assert
        Assert.That(results, Is.All.False);
    }

    #endregion

    #region Active File Path Tests

    [Test]
    public void ShouldRoll_WithValidActiveFilePath_ShouldEvaluateSuccessfully()
    {
        // Act
        var result = _strategy.ShouldRoll(
            "Logs/Application.log");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WithDifferentActiveFilePaths_ShouldProduceSameRollingDecision()
    {
        // Arrange
        var strategy1 = new DailyRollingStrategy();
        var strategy2 = new DailyRollingStrategy();

        // Act
        var result1 = strategy1.ShouldRoll(
            "Logs/Application.log");

        var result2 = strategy2.ShouldRoll(
            "Logs/OtherApplication.log");

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
    }

    [Test]
    public void ShouldRoll_WithEmptyActiveFilePath_ShouldEvaluateSuccessfully()
    {
        // Act
        var result = _strategy.ShouldRoll(string.Empty);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldRoll_WithNullActiveFilePath_ShouldEvaluateSuccessfully()
    {
        // Act
        var result = _strategy.ShouldRoll(null!);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region State Isolation Tests

    [Test]
    public void ShouldRoll_WithSeparateInstances_ShouldMaintainIndependentState()
    {
        // Arrange
        var strategy1 = new DailyRollingStrategy();
        var strategy2 = new DailyRollingStrategy();

        // Act
        var strategy1Result =
            strategy1.ShouldRoll("Logs/Application.log");

        var strategy2Result =
            strategy2.ShouldRoll("Logs/Application.log");

        // Assert
        Assert.That(strategy1Result, Is.False);
        Assert.That(strategy2Result, Is.False);
    }

    #endregion

    #region Contract Tests

    [Test]
    public void ShouldRoll_ShouldImplementIRollingStrategy()
    {
        // Assert
        Assert.That(
            _strategy,
            Is.InstanceOf<IRollingStrategy>());
    }

    #endregion
}