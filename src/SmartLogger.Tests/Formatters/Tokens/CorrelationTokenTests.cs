using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens
{
    [TestFixture]
    public class CorrelationTokenTests
    {
        [Test]
        public void Token_ShouldReturnCorrelationTokenIdentifier()
        {
            // Arrange
            var token = new CorrelationToken();

            // Act
            var result = token.Token;

            // Assert
            Assert.That(result, Is.EqualTo("%CORRELATION"));
        }

        [Test]
        public void Render_WithValidCorrelationId_ShouldReturnCorrelationId()
        {
            // Arrange
            var token = new CorrelationToken();

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId("correlation-123")
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo("correlation-123"));
        }

        [Test]
        public void Render_WithNullCorrelationId_ShouldReturnNA()
        {
            // Arrange
            var token = new CorrelationToken();

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId(null)
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo("N/A"));
        }

        [Test]
        public void Render_WithEmptyCorrelationId_ShouldReturnNA()
        {
            // Arrange
            var token = new CorrelationToken();

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId(string.Empty)
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo("N/A"));
        }

        [Test]
        public void Render_WithWhitespaceCorrelationId_ShouldReturnNA()
        {
            // Arrange
            var token = new CorrelationToken();

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("   ")
                .WithCorrelationId("     ")
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo("N/A"));
        }

        [Test]
        public void Render_WithCorrelationIdContainingWhitespace_ShouldPreserveOriginalValue()
        {
            // Arrange
            var token = new CorrelationToken();

            var correlationId = "  correlation-123  ";

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId(correlationId)
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo(correlationId));
        }

        [Test]
        public void Render_WithUnicodeCorrelationId_ShouldReturnCorrelationId()
        {
            // Arrange
            var token = new CorrelationToken();

            var correlationId = "ஸ்மார்ட்Logger_日本語_日志";

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId(correlationId)
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo(correlationId));
        }

        [Test]
        public void Render_WithVeryLongCorrelationId_ShouldReturnCorrelationId()
        {
            // Arrange
            var token = new CorrelationToken();

            var correlationId = new string('A', 5000);

            var message = new LogMessage.Builder()
                .WithLevel(LogLevel.INFO)
                .WithMessage("Test message")
                .WithCorrelationId(correlationId)
                .Build();

            // Act
            var result = token.Render(message);

            // Assert
            Assert.That(result, Is.EqualTo(correlationId));
        }

        [Test]
        public void Render_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Arrange
            var token = new CorrelationToken();

            LogMessage message = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                token.Render(message));

            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void Render_WithNullMessage_ShouldIdentifyMessageParameter()
        {
            // Arrange
            var token = new CorrelationToken();

            LogMessage message = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                token.Render(message));

            Assert.That(ex.ParamName, Is.EqualTo(nameof(message)));
        }
    }
}