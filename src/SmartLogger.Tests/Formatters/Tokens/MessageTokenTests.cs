using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class MessageTokenTests
{
    private MessageToken _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new MessageToken();
    }

    [Test]
    public void Token_ShouldReturnMessageTokenIdentifier()
    {
        // Act
        var result = _sut.Token;

        // Assert
        Assert.That(result, Is.EqualTo("%MESSAGE"));
    }

    [Test]
    public void Render_WithValidMessage_ShouldReturnMessageContent()
    {
        // Arrange
        const string expectedMessage = "Application started successfully.";
        var message = CreateMessage(expectedMessage);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void Render_WithEmptyMessage_ShouldReturnEmptyString()
    {
        // Arrange
        var message = CreateMessage(string.Empty);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Render_WithNullMessageContent_ShouldReturnEmptyString()
    {
        // Arrange
        var message = CreateMessage(null);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Render_WithWhitespaceMessage_ShouldPreserveWhitespace()
    {
        // Arrange
        const string expectedMessage = "   ";
        var message = CreateMessage(expectedMessage);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void Render_WithSpecialCharacters_ShouldReturnMessageUnchanged()
    {
        // Arrange
        const string expectedMessage = "Hello\nWorld\t!";
        var message = CreateMessage(expectedMessage);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void Render_WithUnicodeMessage_ShouldReturnMessageUnchanged()
    {
        // Arrange
        const string expectedMessage = "Hello 世界 🌍";
        var message = CreateMessage(expectedMessage);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessage));
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

    [Test]
    public void Render_WithSameMessage_ShouldReturnSameResult()
    {
        // Arrange
        var message = CreateMessage("Test message");

        // Act
        var firstResult = _sut.Render(message);
        var secondResult = _sut.Render(message);

        // Assert
        Assert.That(secondResult, Is.EqualTo(firstResult));
    }

    [Test]
    public void Render_WithDifferentMessages_ShouldReturnCorrespondingContent()
    {
        // Arrange
        var firstMessage = CreateMessage("First message");
        var secondMessage = CreateMessage("Second message");

        // Act
        var firstResult = _sut.Render(firstMessage);
        var secondResult = _sut.Render(secondMessage);

        // Assert
        Assert.That(firstResult, Is.EqualTo("First message"));
        Assert.That(secondResult, Is.EqualTo("Second message"));
    }

    private static LogMessage CreateMessage(string? message)
    {
        // Adjust this factory method if LogMessage requires additional constructor arguments.
        return new LogMessage.Builder().WithMessage(message).Build();
    }
}