using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class ThreadTokenTests
{
    private ThreadToken _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ThreadToken();
    }

    [Test]
    public void Token_ShouldReturnThreadTokenIdentifier()
    {
        // Act
        var result = _sut.Token;

        // Assert
        Assert.That(result, Is.EqualTo("%THREAD"));
    }

    [Test]
    public void Render_WithValidThreadId_ShouldReturnThreadIdAsString()
    {
        // Arrange
        const int threadId = 123;
        var message = CreateMessage(threadId);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("123"));
    }

    [Test]
    public void Render_WithZeroThreadId_ShouldReturnZeroAsString()
    {
        // Arrange
        var message = CreateMessage(0);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("0"));
    }

    [Test]
    public void Render_WithPositiveThreadId_ShouldReturnThreadIdAsString()
    {
        // Arrange
        const int threadId = 42;
        var message = CreateMessage(threadId);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo("42"));
    }

    [Test]
    public void Render_WithLargeThreadId_ShouldReturnThreadIdAsString()
    {
        // Arrange
        const int threadId = int.MaxValue;
        var message = CreateMessage(threadId);

        // Act
        var result = _sut.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(int.MaxValue.ToString()));
    }

    [Test]
    public void Render_WithSameThreadId_ShouldReturnSameResult()
    {
        // Arrange
        var firstMessage = CreateMessage(123);
        var secondMessage = CreateMessage(123);

        // Act
        var firstResult = _sut.Render(firstMessage);
        var secondResult = _sut.Render(secondMessage);

        // Assert
        Assert.That(secondResult, Is.EqualTo(firstResult));
    }

    [Test]
    public void Render_WithDifferentThreadIds_ShouldReturnCorrespondingValues()
    {
        // Arrange
        var firstMessage = CreateMessage(101);
        var secondMessage = CreateMessage(202);

        // Act
        var firstResult = _sut.Render(firstMessage);
        var secondResult = _sut.Render(secondMessage);

        // Assert
        Assert.That(firstResult, Is.EqualTo("101"));
        Assert.That(secondResult, Is.EqualTo("202"));
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

    private static LogMessage CreateMessage(int threadId)
    {
        // Adjust this factory method if LogMessage requires additional constructor arguments.
        return new LogMessage.Builder().Build();
    }
}