using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;
using System.Globalization;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class TimestampTokenTests
{
    private TimestampToken _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new TimestampToken();
    }

    [Test]
    public void Token_ShouldReturnTimestampTokenIdentifier()
    {
        // Act
        var result = _sut.Token;

        // Assert
        Assert.That(result, Is.EqualTo("%TIMESTAMP"));
    }

    [Test]
    public void Render_WithDefaultFormat_ShouldReturnFormattedTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 123);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 14:30:45.123"));
    }

    [Test]
    public void Render_WithDefaultFormat_ShouldIncludeMilliseconds()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 7);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 14:30:45.007"));
    }

    [Test]
    public void Render_WithCustomFormat_ShouldReturnFormattedTimestamp()
    {
        // Arrange
        _sut = new TimestampToken("dd/MM/yyyy HH:mm:ss");

        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("10/09/2026 14:30:45"));
    }

    [Test]
    public void Render_WithDateOnlyFormat_ShouldReturnFormattedDate()
    {
        // Arrange
        _sut = new TimestampToken("yyyy-MM-dd");

        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10"));
    }

    [Test]
    public void Render_WithTimeOnlyFormat_ShouldReturnFormattedTime()
    {
        // Arrange
        _sut = new TimestampToken("HH:mm:ss");

        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("14:30:45"));
    }

    [Test]
    public void Render_WithLiteralTextFormat_ShouldReturnFormattedTimestamp()
    {
        // Arrange
        _sut = new TimestampToken("'Date:' yyyy-MM-dd 'Time:' HH:mm:ss");

        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("Date: 2026-09-10 Time: 14:30:45"));
    }

    [Test]
    public void Render_CalledMultipleTimes_ShouldUseConfiguredFormat()
    {
        // Arrange
        _sut = new TimestampToken("yyyy-MM-dd");

        var firstMessage = CreateMessage(
            new DateTime(2026, 9, 10, 14, 30, 45));

        var secondMessage = CreateMessage(
            new DateTime(2027, 12, 25, 10, 15, 30));

        // Act
        var firstResult = _sut.Render(firstMessage);
        var secondResult = _sut.Render(secondMessage);

        // Assert
        Assert.That(firstResult, Is.EqualTo("2026-09-10"));
        Assert.That(secondResult, Is.EqualTo("2027-12-25"));
    }

    [Test]
    public void Render_WithDifferentCurrentCultures_ShouldReturnSameResult()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 123);
        var message = CreateMessage(timestamp);

        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var resultUs = _sut.Render(message);

            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var resultGermany = _sut.Render(message);

            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            var resultFrance = _sut.Render(message);

            // Assert
            Assert.That(resultUs, Is.EqualTo("2026-09-10 14:30:45.123"));
            Assert.That(resultGermany, Is.EqualTo(resultUs));
            Assert.That(resultFrance, Is.EqualTo(resultUs));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void Render_WithDifferentCurrentUICultures_ShouldReturnSameResult()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 123);
        var message = CreateMessage(timestamp);

        var originalUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var resultUs = _sut.Render(message);

            CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");
            var resultJapan = _sut.Render(message);

            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
            var resultFrance = _sut.Render(message);

            // Assert
            Assert.That(resultUs, Is.EqualTo("2026-09-10 14:30:45.123"));
            Assert.That(resultJapan, Is.EqualTo(resultUs));
            Assert.That(resultFrance, Is.EqualTo(resultUs));
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }

    [Test]
    public void Render_WithMidnightTimestamp_ShouldReturnCorrectResult()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 0, 0, 0, 0);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 00:00:00.000"));
    }

    [Test]
    public void Render_WithEndOfDayTimestamp_ShouldReturnCorrectResult()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 23, 59, 59, 999);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 23:59:59.999"));
    }

    [Test]
    public void Render_WithZeroMilliseconds_ShouldRenderThreeZeroDigits()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 0);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 14:30:45.000"));
    }

    [Test]
    public void Render_WithMaximumMilliseconds_ShouldRenderCorrectValue()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 10, 14, 30, 45, 999);
        var message = CreateMessage(timestamp);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("2026-09-10 14:30:45.999"));
    }

    [Test]
    public void Render_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _sut.Render(null!));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Render_WithNullMessage_ShouldIdentifyMessageParameter()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _sut.Render(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("message"));
    }

    private static LogMessage CreateMessage(DateTime timestamp)
    {
        // Adjust this factory method if LogMessage requires additional constructor arguments.
        return new LogMessage.Builder().Build();
    }
}