using SmartLogger.Core;
using SmartLogger.Formatters;

namespace SmartLogger.Tests.Formatters;

[TestFixture]
public class XmlFormatterTests
{
    #region Basic Formatting

    [Test]
    public void Format_WithValidMessage_ShouldReturnMessageContent()
    {
        // Arrange
        var formatter = new XmlFormatter();
        var message = CreateMessage("Test message");

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(message.Message));
    }

    [Test]
    public void Format_WithValidMessage_ShouldNotIncludeAdditionalFields()
    {
        // Arrange
        const string messageText = "Test message";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
        Assert.That(result, Does.Not.Contain(message.Source));
        Assert.That(result, Does.Not.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Not.Contain(message.ThreadId.ToString()));
        Assert.That(result, Does.Not.Contain(message.CorrelationId));
    }

    [Test]
    public void Format_WithValidMessage_ShouldPreserveExactContent()
    {
        // Arrange
        const string messageText = "  Test message  ";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.SameAs(message.Message));
    }

    #endregion

    #region Content Preservation

    [Test]
    public void Format_WithWhitespaceMessage_ShouldPreserveWhitespace()
    {
        // Arrange
        const string messageText = "  Test   message \t ";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    [Test]
    public void Format_WithMultilineMessage_ShouldPreserveLineBreaks()
    {
        // Arrange
        const string messageText = "Line 1\nLine 2\r\nLine 3\tEnd";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    [Test]
    public void Format_WithSpecialCharacters_ShouldReturnCharactersUnchanged()
    {
        // Arrange
        const string messageText = "<message>Hello & \"World\" 'Test' \\ Path</message>";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    [Test]
    public void Format_WithUnicodeMessage_ShouldPreserveUnicodeContent()
    {
        // Arrange
        const string messageText = "தமிழ் 日本語 Ελληνικά 🚀";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    #endregion

    #region Boundary Tests

    [Test]
    public void Format_WithEmptyMessage_ShouldReturnEmptyString()
    {
        // Arrange
        var formatter = new XmlFormatter();
        var message = CreateMessage(string.Empty);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Format_WithWhitespaceOnlyMessage_ShouldPreserveWhitespace()
    {
        // Arrange
        const string messageText = "   \t   ";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    #endregion

    #region Null Input

    [Test]
    public void Format_WithNullMessage_ShouldThrowNullReferenceException()
    {
        // Arrange
        var formatter = new XmlFormatter();

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => formatter.Format(null!));
    }

    [Test]
    public void Format_WithValidMessage_ShouldNotThrow()
    {
        // Arrange
        var formatter = new XmlFormatter();
        var message = CreateMessage();

        // Act & Assert
        Assert.DoesNotThrow(() => formatter.Format(message));
    }

    #endregion

    #region Current XML Behavior

    [Test]
    public void Format_WithXmlSensitiveMessage_ShouldReturnRawMessage()
    {
        // Arrange
        const string messageText =
            "<message>Hello & welcome</message>";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
    }

    [Test]
    public void Format_ShouldNotAddXmlWrapper()
    {
        // Arrange
        const string messageText = "Test message";

        var formatter = new XmlFormatter();
        var message = CreateMessage(messageText);

        // Act
        var result = formatter.Format(message);

        // Assert
        Assert.That(result, Is.EqualTo(messageText));
        Assert.That(result, Does.Not.StartWith("<"));
        Assert.That(result, Does.Not.EndWith(">"));
    }

    #endregion

    #region Test Helpers

    private static LogMessage CreateMessage(
        string messageText = "Test message")
    {
        return new LogMessage.Builder()
            .WithLevel(LogLevel.INFO)
            .WithMessage(messageText)
            .FromSource("TestSource")
            .WithCorrelationId("correlation-123")
            .Build();
    }

    #endregion
}