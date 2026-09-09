using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class SourceTokenTests
{
    private SourceToken _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new SourceToken();
    }

    [Test]
    public void Token_ShouldReturnSourceTokenIdentifier()
    {
        // Act
        var result = _sut.Token;

        // Assert
        Assert.That(result, Is.EqualTo("%SOURCE"));
    }

    [Test]
    public void Render_WithValidSource_ShouldReturnSource()
    {
        // Arrange
        const string expectedSource = "SmartLogger.Core.Logger";
        var message = CreateMessage(expectedSource);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedSource));
    }

    [Test]
    public void Render_WithEmptySource_ShouldReturnEmptyString()
    {
        // Arrange
        var message = CreateMessage(string.Empty);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Render_WithWhitespaceSource_ShouldPreserveWhitespace()
    {
        // Arrange
        const string expectedSource = "   ";
        var message = CreateMessage(expectedSource);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedSource));
    }

    [Test]
    public void Render_WithSpecialCharacters_ShouldReturnSourceUnchanged()
    {
        // Arrange
        const string expectedSource = "Logger\nSource\tName";
        var message = CreateMessage(expectedSource);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedSource));
    }

    [Test]
    public void Render_WithUnicodeSource_ShouldReturnSourceUnchanged()
    {
        // Arrange
        const string expectedSource = "Logger.日志.ロガー";
        var message = CreateMessage(expectedSource);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedSource));
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
        var message = CreateMessage("SmartLogger.Core.Logger");

        // Act
        var firstResult = _sut.Render(message);
        var secondResult = _sut.Render(message);

        // Assert
        Assert.That(secondResult, Is.EqualTo(firstResult));
    }

    [Test]
    public void Render_WithDifferentSources_ShouldReturnCorrespondingSource()
    {
        // Arrange
        var firstMessage = CreateMessage("SmartLogger.Core.Logger");
        var secondMessage = CreateMessage("SmartLogger.Core.Appender");

        // Act
        var firstResult = _sut.Render(firstMessage);
        var secondResult = _sut.Render(secondMessage);

        // Assert
        Assert.That(firstResult, Is.EqualTo("SmartLogger.Core.Logger"));
        Assert.That(secondResult, Is.EqualTo("SmartLogger.Core.Appender"));
    }

    private static LogMessage CreateMessage(string source)
    {
        // Adjust this factory method if LogMessage requires additional constructor arguments.
        return new LogMessage.Builder().FromSource(source).Build();
    }
}
