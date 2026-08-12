using SmartLogger.Core;

namespace SmartLogger.Tests.Core;

[TestFixture]
public class LogLevelTests
{
    [Test]
    public void LogLevel_ShouldHaveExpectedNumericValues()
    {
        // Arrange

        // Act

        // Assert
        Assert.That((int)LogLevel.DEBUG, Is.EqualTo(1));
        Assert.That((int)LogLevel.INFO, Is.EqualTo(2));
        Assert.That((int)LogLevel.WARNING, Is.EqualTo(3));
        Assert.That((int)LogLevel.ERROR, Is.EqualTo(4));
        Assert.That((int)LogLevel.FATAL, Is.EqualTo(5));
    }

    [TestCase(LogLevel.DEBUG, LogLevel.DEBUG, true)]
    [TestCase(LogLevel.DEBUG, LogLevel.INFO, false)]
    [TestCase(LogLevel.DEBUG, LogLevel.WARNING, false)]
    [TestCase(LogLevel.DEBUG, LogLevel.ERROR, false)]
    [TestCase(LogLevel.DEBUG, LogLevel.FATAL, false)]

    [TestCase(LogLevel.INFO, LogLevel.DEBUG, true)]
    [TestCase(LogLevel.INFO, LogLevel.INFO, true)]
    [TestCase(LogLevel.INFO, LogLevel.WARNING, false)]
    [TestCase(LogLevel.INFO, LogLevel.ERROR, false)]
    [TestCase(LogLevel.INFO, LogLevel.FATAL, false)]

    [TestCase(LogLevel.WARNING, LogLevel.DEBUG, true)]
    [TestCase(LogLevel.WARNING, LogLevel.INFO, true)]
    [TestCase(LogLevel.WARNING, LogLevel.WARNING, true)]
    [TestCase(LogLevel.WARNING, LogLevel.ERROR, false)]
    [TestCase(LogLevel.WARNING, LogLevel.FATAL, false)]

    [TestCase(LogLevel.ERROR, LogLevel.DEBUG, true)]
    [TestCase(LogLevel.ERROR, LogLevel.INFO, true)]
    [TestCase(LogLevel.ERROR, LogLevel.WARNING, true)]
    [TestCase(LogLevel.ERROR, LogLevel.ERROR, true)]
    [TestCase(LogLevel.ERROR, LogLevel.FATAL, false)]

    [TestCase(LogLevel.FATAL, LogLevel.DEBUG, true)]
    [TestCase(LogLevel.FATAL, LogLevel.INFO, true)]
    [TestCase(LogLevel.FATAL, LogLevel.WARNING, true)]
    [TestCase(LogLevel.FATAL, LogLevel.ERROR, true)]
    [TestCase(LogLevel.FATAL, LogLevel.FATAL, true)]
    public void IsGreaterOrEqual_WithAllLogLevels_ShouldReturnExpectedResult(
        LogLevel current,
        LogLevel threshold,
        bool expected)
    {
        // Arrange

        // Act
        var result = current.IsGreaterOrEqual(threshold);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(10, 5, true)]
    [TestCase(0, 1, false)]
    [TestCase(10, 10, true)]
    [TestCase(5, 10, false)]
    public void IsGreaterOrEqual_WithUndefinedValues_ShouldCompareUnderlyingValue(
        int currentValue,
        int thresholdValue,
        bool expected)
    {
        // Arrange
        var current = (LogLevel)currentValue;
        var threshold = (LogLevel)thresholdValue;

        // Act
        var result = current.IsGreaterOrEqual(threshold);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}