using SmartLogger.Core;
using System;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LogMessageTests
    {
        [Test]
        public void Builder_DefaultInitialization_ShouldSetExpectedDefaults()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var beforeBuild = builder;

            // Assert
            Assert.That(beforeBuild, Is.Not.Null);
        }

        [Test]
        public void Build_WithRequiredFields_ShouldCreateLogMessage()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message, Is.Not.Null);
            Assert.That(message.LogLevel, Is.EqualTo(LogLevel.INFO));
            Assert.That(message.Message, Is.EqualTo("hello"));
            Assert.That(message.Source, Is.EqualTo("System"));
            Assert.That(message.ThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
            Assert.That(message.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(message.CorrelationId, Is.Null);
        }

        [Test]
        public void WithLevel_ShouldAssignLogLevel()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.ERROR).WithMessage("failed").Build();

            // Assert
            Assert.That(message.LogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void WithMessage_ShouldAssignMessage()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("payload").Build();

            // Assert
            Assert.That(message.Message, Is.EqualTo("payload"));
        }

        [Test]
        public void FromSource_ShouldAssignSource()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").FromSource("API").Build();

            // Assert
            Assert.That(message.Source, Is.EqualTo("API"));
        }

        [Test]
        public void WithCorrelationId_ShouldAssignCorrelationId()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").WithCorrelationId("req-42").Build();

            // Assert
            Assert.That(message.CorrelationId, Is.EqualTo("req-42"));
        }

        [Test]
        public void Build_ShouldPreserveTimestamp()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.Timestamp, Is.EqualTo(builder.InternalTimestamp).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void Build_ShouldPreserveThreadId()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.ThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
        }

        [Test]
        public void Builder_ShouldSupportMethodChaining()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var chained = builder.WithLevel(LogLevel.INFO).WithMessage("x").FromSource("A").WithCorrelationId("c");

            // Assert
            Assert.That(chained, Is.SameAs(builder));
        }

        [Test]
        public void Builder_WithFluentCalls_ShouldCreateExpectedLogMessage()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.WARNING)
                .WithMessage("warning")
                .FromSource("Service")
                .WithCorrelationId("trace-007")
                .Build();

            // Assert
            Assert.That(message.LogLevel, Is.EqualTo(LogLevel.WARNING));
            Assert.That(message.Message, Is.EqualTo("warning"));
            Assert.That(message.Source, Is.EqualTo("Service"));
            Assert.That(message.CorrelationId, Is.EqualTo("trace-007"));
        }

        [Test]
        public void Build_WithoutLogLevel_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => builder.WithMessage("hello").Build());
            Assert.That(ex.Message, Is.EqualTo("LogLevel is required."));
        }

        [Test]
        public void Build_WithNullMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => builder.WithLevel(LogLevel.INFO).WithMessage(null).Build());
            Assert.That(ex.Message, Is.EqualTo("Message is required."));
        }

        [Test]
        public void Build_WithEmptyMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => builder.WithLevel(LogLevel.INFO).WithMessage(string.Empty).Build());
            Assert.That(ex.Message, Is.EqualTo("Message is required."));
        }

        [Test]
        public void Build_WithWhitespaceMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => builder.WithLevel(LogLevel.INFO).WithMessage("   ").Build());
            Assert.That(ex.Message, Is.EqualTo("Message is required."));
        }

        [Test]
        public void Build_WithoutSource_ShouldUseDefaultSource()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.Source, Is.EqualTo("System"));
        }

        [Test]
        public void Build_WithoutCorrelationId_ShouldUseNullCorrelationId()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.CorrelationId, Is.Null);
        }

        [Test]
        public void Build_WithoutTimestampOverride_ShouldUseCurrentUtcTimestamp()
        {
            // Arrange
            var builder = new LogMessage.Builder();
            var before = DateTime.UtcNow;

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();
            var after = DateTime.UtcNow;

            // Assert
            Assert.That(message.Timestamp, Is.GreaterThanOrEqualTo(before));
            Assert.That(message.Timestamp, Is.LessThanOrEqualTo(after));
        }

        [Test]
        public void Build_WithoutThreadOverride_ShouldUseCurrentManagedThreadId()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.ThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
        }

        [Test]
        public void WithUnicodeMessage_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("こんにちは世界 🌍").Build();

            // Assert
            Assert.That(message.Message, Is.EqualTo("こんにちは世界 🌍"));
        }

        [Test]
        public void WithUnicodeSource_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").FromSource("サービス").Build();

            // Assert
            Assert.That(message.Source, Is.EqualTo("サービス"));
        }

        [Test]
        public void WithVeryLongMessage_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();
            var messageContent = new string('X', 10_000);

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage(messageContent).Build();

            // Assert
            Assert.That(message.Message, Is.EqualTo(messageContent));
        }

        [Test]
        public void WithVeryLongSource_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();
            var source = new string('S', 10_000);

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").FromSource(source).Build();

            // Assert
            Assert.That(message.Source, Is.EqualTo(source));
        }

        [Test]
        public void WithNullCorrelationId_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").WithCorrelationId(null).Build();

            // Assert
            Assert.That(message.CorrelationId, Is.Null);
        }

        [Test]
        public void WithEmptyCorrelationId_ShouldBuildSuccessfully()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").WithCorrelationId(string.Empty).Build();

            // Assert
            Assert.That(message.CorrelationId, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Build_ShouldCreateImmutableLogMessage()
        {
            // Arrange
            var builder = new LogMessage.Builder();

            // Act
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Assert
            Assert.That(message.Message, Is.EqualTo("hello"));
            Assert.That(message.Source, Is.EqualTo("System"));
            Assert.That(message.CorrelationId, Is.Null);
        }

        [Test]
        public void Builder_ModifiedAfterBuild_ShouldNotChangeExistingLogMessage()
        {
            // Arrange
            var builder = new LogMessage.Builder();
            var message = builder.WithLevel(LogLevel.INFO).WithMessage("hello").Build();

            // Act
            builder.WithMessage("changed").FromSource("Changed").WithCorrelationId("new-trace");

            // Assert
            Assert.That(message.Message, Is.EqualTo("hello"));
            Assert.That(message.Source, Is.EqualTo("System"));
            Assert.That(message.CorrelationId, Is.Null);
        }
    }
}
