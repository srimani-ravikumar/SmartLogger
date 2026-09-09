using Moq;
using SmartLogger.Core;
using SmartLogger.Formatters;

namespace SmartLogger.Tests.Formatters;

[TestFixture]
public class PlainTextFormatterTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_WithValidLayout_ShouldCreateFormatter()
    {
        // Arrange
        var layout = new Mock<ILogLayoutStrategy>();

        // Act
        var formatter = new PlainTextFormatter(layout.Object);

        // Assert
        Assert.That(formatter, Is.Not.Null);
    }

    #endregion

    #region Layout Delegation

    [Test]
    public void Format_WithValidMessage_ShouldCallLayoutRender()
    {
        // Arrange
        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(It.IsAny<LogMessage>()))
            .Returns("formatted message");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(message);

        // Assert
        layout.Verify(
            x => x.Render(It.IsAny<LogMessage>()),
            Times.Once);
    }

    [Test]
    public void Format_WithValidMessage_ShouldPassSameMessageToLayout()
    {
        // Arrange
        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(It.IsAny<LogMessage>()))
            .Returns("formatted message");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(message);

        // Assert
        layout.Verify(
            x => x.Render(It.Is<LogMessage>(x => ReferenceEquals(x, message))),
            Times.Once);
    }

    [Test]
    public void Format_CalledOnce_ShouldCallLayoutRenderExactlyOnce()
    {
        // Arrange
        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(It.IsAny<LogMessage>()))
            .Returns("formatted message");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(message);

        // Assert
        layout.Verify(
            x => x.Render(It.IsAny<LogMessage>()),
            Times.Exactly(1));
    }

    [Test]
    public void Format_CalledMultipleTimes_ShouldCallLayoutForEachRequest()
    {
        // Arrange
        var firstMessage = CreateMessage("First message");
        var secondMessage = CreateMessage("Second message");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(It.IsAny<LogMessage>()))
            .Returns("formatted message");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(firstMessage);
        formatter.Format(secondMessage);
        formatter.Format(firstMessage);

        // Assert
        layout.Verify(
            x => x.Render(It.IsAny<LogMessage>()),
            Times.Exactly(3));
    }

    #endregion

    #region Output Propagation

    [Test]
    public void Format_WhenLayoutReturnsFormattedText_ShouldReturnSameText()
    {
        // Arrange
        const string expected = "[2026-09-10] [INFO] TestSource - Test message";

        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns(expected);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Format_WhenLayoutReturnsEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns(string.Empty);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Format_WhenLayoutReturnsWhitespace_ShouldPreserveWhitespace()
    {
        // Arrange
        const string expected = "  \t formatted output \n  ";

        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns(expected);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Format_WhenLayoutReturnsComplexText_ShouldReturnExactOutput()
    {
        // Arrange
        const string expected =
            "[timestamp] [INFO] [T#123] [correlation-123] " +
            "TestSource - Message with \"quotes\", \\ slash, \nnew line\tand Unicode: தமிழ் 🚀";

        var message = CreateMessage();
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns(expected);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region Message Handling

    [Test]
    public void Format_WithValidMessage_ShouldReturnFormattedOutput()
    {
        // Arrange
        var message = CreateMessage();
        const string expected = "Formatted log output";

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns(expected);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Format_WithSpecialCharactersInMessage_ShouldDelegateWithoutModification()
    {
        // Arrange
        var message = CreateMessage(
            "Message with \"quotes\", \\ slash, \n newline and \t tab");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns("formatted");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(message);

        // Assert
        layout.Verify(
            x => x.Render(It.Is<LogMessage>(m =>
                ReferenceEquals(m, message) &&
                m.Message == message.Message)),
            Times.Once);
    }

    [Test]
    public void Format_WithUnicodeMessage_ShouldDelegateWithoutModification()
    {
        // Arrange
        var message = CreateMessage("தமிழ் 日本語 Ελληνικά 🚀");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Returns("formatted");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        formatter.Format(message);

        // Assert
        layout.Verify(
            x => x.Render(It.Is<LogMessage>(m =>
                ReferenceEquals(m, message) &&
                m.Message == message.Message)),
            Times.Once);
    }

    #endregion

    #region Exception Propagation

    [Test]
    public void Format_WhenLayoutThrowsException_ShouldPropagateException()
    {
        // Arrange
        var message = CreateMessage();
        var expectedException = new Exception("Layout rendering failed");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Throws(expectedException);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var exception = Assert.Throws<Exception>(
            () => formatter.Format(message));

        // Assert
        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void Format_WhenLayoutThrowsException_ShouldPreserveOriginalException()
    {
        // Arrange
        var message = CreateMessage();
        var expectedException =
            new Exception("Original layout exception");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Throws(expectedException);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var exception = Assert.Throws<Exception>(
            () => formatter.Format(message));

        // Assert
        Assert.That(exception!.Message, Is.EqualTo(expectedException.Message));
        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void Format_WhenLayoutThrowsInvalidOperationException_ShouldPropagateException()
    {
        // Arrange
        var message = CreateMessage();
        var expectedException =
            new InvalidOperationException("Invalid layout state");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Throws(expectedException);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => formatter.Format(message));

        // Assert
        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void Format_WhenLayoutThrowsArgumentException_ShouldPropagateException()
    {
        // Arrange
        var message = CreateMessage();
        var expectedException =
            new ArgumentException("Invalid layout argument");

        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(message))
            .Throws(expectedException);

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => formatter.Format(message));

        // Assert
        Assert.That(exception, Is.SameAs(expectedException));
    }

    #endregion

    #region Null Input

    [Test]
    public void Format_WithNullMessage_ShouldPassNullToLayout()
    {
        // Arrange
        var layout = new Mock<ILogLayoutStrategy>();

        layout
            .Setup(x => x.Render(null!))
            .Returns("formatted null message");

        var formatter = new PlainTextFormatter(layout.Object);

        // Act
        var result = formatter.Format(null!);

        // Assert
        Assert.That(result, Is.EqualTo("formatted null message"));

        layout.Verify(
            x => x.Render(null!),
            Times.Once);
    }

    [Test]
    public void Format_WithNullLayout_ShouldThrowNullReferenceException()
    {
        // Arrange
        var formatter = new PlainTextFormatter(null!);
        var message = CreateMessage();

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => formatter.Format(message));
    }

    #endregion

    #region Test Helpers

    private static LogMessage CreateMessage(
        string messageText = "Test message",
        string source = "TestSource",
        string correlationId = "correlation-123")
    {
        return new LogMessage.Builder()
            .WithLevel(LogLevel.INFO)
            .WithMessage(messageText)
            .FromSource(source)
            .WithCorrelationId(correlationId)
            .Build();
    }

    #endregion
}