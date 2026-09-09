using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class LevelTokenTests
{
    private LevelToken _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new LevelToken();
    }

    [Test]
    public void Token_ShouldReturnLevelTokenIdentifier()
    {
        // Act
        var result = _sut.Token;

        // Assert
        Assert.That(result, Is.EqualTo("%LEVEL"));
    }

    [Test]
    public void Render_WithStandardLogLevel_ShouldReturnFormattedLevel()
    {
        // Arrange
        var message = CreateMessage(LogLevel.INFO);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("INFO "));
    }

    [Test]
    public void Render_WithShortLogLevel_ShouldPadResultToFiveCharacters()
    {
        // Arrange
        var message = CreateMessage(LogLevel.DEBUG);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Has.Length.EqualTo(5));
        Assert.That(result, Is.EqualTo("DEBUG"));
    }

    [Test]
    public void Render_WithFiveCharacterLogLevel_ShouldReturnUnchangedLevel()
    {
        // Arrange
        var message = CreateMessage(LogLevel.ERROR);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("ERROR"));
        Assert.That(result, Has.Length.EqualTo(5));
    }

    [Test]
    public void Render_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _sut.Render(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("message"));
    }

    private static LogMessage CreateMessage(LogLevel logLevel)
    {
        // Adjust this factory method if LogMessage requires additional constructor arguments.
        return new LogMessage.Builder().WithLevel(logLevel).Build();
    }
}