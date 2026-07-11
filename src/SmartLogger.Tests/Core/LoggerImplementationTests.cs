using Moq;
using SmartLogger.Appenders;
using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LoggerImplementationTests
    {
        private static LoggerImplementation CreateLogger(string name = "TestLogger", LogLevel logLevel = LogLevel.INFO, bool enableDefaultAppender = true)
        {
            return new LoggerImplementation(name, logLevel, enableDefaultAppender);
        }

        private static ILogAppender CreateAppender(bool enabled = true)
        {
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(enabled);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>()));
            return appender.Object;
        }

        private static ILogFilter CreateFilter(bool shouldLog = true)
        {
            var filter = new Mock<ILogFilter>();
            filter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(shouldLog);
            return filter.Object;
        }

        [Test]
        public void Constructor_Default_ShouldInitializeDefaultLogger()
        {
            // Arrange
            var logger = CreateLogger();

            // Act & Assert
            Assert.That(logger, Is.Not.Null);
            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.INFO));
            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithLoggerName_ShouldInitializeLogger()
        {
            // Arrange
            var logger = CreateLogger("CustomLogger");

            // Act & Assert
            Assert.That(logger, Is.Not.Null);
            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithLogLevel_ShouldInitializeLogger()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.DEBUG);

            // Act & Assert
            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void Constructor_WithDefaultAppenderDisabled_ShouldNotAttachConsoleAppender()
        {
            // Arrange
            var logger = CreateLogger(enableDefaultAppender: false);

            // Act & Assert
            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(0));
        }

        [Test]
        public void Log_WithValidInput_ShouldConstructLogMessage()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.WARNING, "hello");

            // Assert
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.WARNING));
            Assert.That(captured.Message, Is.EqualTo("hello"));
            Assert.That(captured.Source, Is.EqualTo("TestLogger"));
            Assert.That(captured.ThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
            Assert.That(captured.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void Log_WithEnabledAppender_ShouldAppendMessage()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_WithMultipleEnabledAppenders_ShouldAppendToAll()
        {
            // Arrange
            var logger = CreateLogger();
            var firstAppender = new Mock<ILogAppender>();
            var secondAppender = new Mock<ILogAppender>();
            firstAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            secondAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddAppender(firstAppender.Object);
            logger.AddAppender(secondAppender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            firstAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
            secondAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_WithCorrelationScope_ShouldPopulateCorrelationId()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            using (LogContext.BeginCorrelationScope("req-123"))
            {
                logger.Log(LogLevel.INFO, "hello");
            }

            // Assert
            Assert.That(captured.CorrelationId, Is.EqualTo("req-123"));
        }

        [Test]
        public void Log_ShouldPopulateSourceName()
        {
            // Arrange
            var logger = CreateLogger("SourceLogger");
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            Assert.That(captured.Source, Is.EqualTo("SourceLogger"));
        }

        [Test]
        public void Log_ShouldPopulateCurrentThreadId()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            Assert.That(captured.ThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
        }

        [Test]
        public void Log_ShouldPopulateUtcTimestamp()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            Assert.That(captured.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void Log_BelowEffectiveMinimumLevel_ShouldNotInvokeFiltersOrAppenders()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.WARNING);
            var filter = new Mock<ILogFilter>();
            var appender = new Mock<ILogAppender>();
            filter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(true);
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddFilter(filter.Object);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            filter.Verify(x => x.ShouldLog(It.IsAny<LogMessage>()), Times.Never);
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_AtEffectiveMinimumLevel_ShouldDispatchLog()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.INFO);
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_AboveEffectiveMinimumLevel_ShouldDispatchLog()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.INFO);
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.ERROR, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void SetLogLevel_ShouldUpdateEffectiveMinimumLevel()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.INFO);

            // Act
            logger.SetLogLevel(LogLevel.ERROR);

            // Assert
            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void Log_WithFilters_ShouldEvaluateFilters()
        {
            // Arrange
            var logger = CreateLogger();
            var filter = new Mock<ILogFilter>();
            var appender = new Mock<ILogAppender>();
            filter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(true);
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddFilter(filter.Object);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            filter.Verify(x => x.ShouldLog(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_WhenFilterRejects_ShouldNotInvokeAppenders()
        {
            // Arrange
            var logger = CreateLogger();
            var filter = new Mock<ILogFilter>();
            var appender = new Mock<ILogAppender>();
            filter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(false);
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddFilter(filter.Object);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_WhenAllFiltersApprove_ShouldInvokeAppenders()
        {
            // Arrange
            var logger = CreateLogger();
            var firstFilter = new Mock<ILogFilter>();
            var secondFilter = new Mock<ILogFilter>();
            var appender = new Mock<ILogAppender>();
            firstFilter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(true);
            secondFilter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(true);
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddFilter(firstFilter.Object);
            logger.AddFilter(secondFilter.Object);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_WhenFirstFilterRejects_ShouldNotEvaluateRemainingFilters()
        {
            // Arrange
            var logger = CreateLogger();
            var firstFilter = new Mock<ILogFilter>();
            var secondFilter = new Mock<ILogFilter>();
            var appender = new Mock<ILogAppender>();
            firstFilter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(false);
            secondFilter.Setup(x => x.ShouldLog(It.IsAny<LogMessage>())).Returns(true);
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddFilter(firstFilter.Object);
            logger.AddFilter(secondFilter.Object);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            secondFilter.Verify(x => x.ShouldLog(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_WithMixedAppenderStates_ShouldInvokeOnlyEnabledAppenders()
        {
            // Arrange
            var logger = CreateLogger();
            var enabledAppender = new Mock<ILogAppender>();
            var disabledAppender = new Mock<ILogAppender>();
            enabledAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            disabledAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
            logger.AddAppender(enabledAppender.Object);
            logger.AddAppender(disabledAppender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            enabledAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
            disabledAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_WithDisabledAppender_ShouldNotAppend()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_WithMultipleEnabledAppenders_ShouldPassSameLogMessageInstance()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage firstMessage = null;
            LogMessage secondMessage = null;
            var firstAppender = new Mock<ILogAppender>();
            var secondAppender = new Mock<ILogAppender>();
            firstAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            secondAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            firstAppender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => firstMessage = message);
            secondAppender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => secondMessage = message);
            logger.AddAppender(firstAppender.Object);
            logger.AddAppender(secondAppender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            Assert.That(firstMessage, Is.SameAs(secondMessage));
        }

        [Test]
        public void Debug_ShouldLogWithDebugLevel()
        {
            // Arrange
            var logger = new LoggerImplementation("TestLogger", LogLevel.DEBUG, true);
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Debug("hello");

            // Assert
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void Info_ShouldLogWithInfoLevel()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Info("hello");

            // Assert
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.INFO));
        }

        [Test]
        public void Warning_ShouldLogWithWarningLevel()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Warning("hello");

            // Assert
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.WARNING));
        }

        [Test]
        public void Error_ShouldLogWithErrorLevel()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Error("hello");

            // Assert
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void Fatal_ShouldLogWithFatalLevel()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Fatal("hello");

            // Assert
            Assert.That(captured.LogLevel, Is.EqualTo(LogLevel.FATAL));
        }

        [Test]
        public void AddAppender_WithValidAppender_ShouldAddAppender()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = CreateAppender();

            // Act
            logger.AddAppender(appender);

            // Assert
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(2));
        }

        [Test]
        public void RemoveAppender_WithExistingAppender_ShouldRemoveAppender()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = CreateAppender();
            logger.AddAppender(appender);

            // Act
            logger.RemoveAppender(appender);

            // Assert
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAppender_WithUnknownAppender_ShouldNotThrow()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = CreateAppender();

            // Act & Assert
            Assert.DoesNotThrow(() => logger.RemoveAppender(appender));
        }

        [Test]
        public void GetLogAppenders_ShouldReturnReadOnlyCollection()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            var appenders = logger.GetLogAppenders();

            // Assert
            Assert.That(appenders, Is.InstanceOf<ReadOnlyCollection<ILogAppender>>());
        }

        [Test]
        public void AddFilter_WithValidFilter_ShouldAddFilter()
        {
            // Arrange
            var logger = CreateLogger();
            var filter = CreateFilter();

            // Act
            logger.AddFilter(filter);

            // Assert
            Assert.That(logger.GetLogFilters().Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveFilter_WithExistingFilter_ShouldRemoveFilter()
        {
            // Arrange
            var logger = CreateLogger();
            var filter = CreateFilter();
            logger.AddFilter(filter);

            // Act
            logger.RemoveFilter(filter);

            // Assert
            Assert.That(logger.GetLogFilters().Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveFilter_WithUnknownFilter_ShouldNotThrow()
        {
            // Arrange
            var logger = CreateLogger();
            var filter = CreateFilter();

            // Act & Assert
            Assert.DoesNotThrow(() => logger.RemoveFilter(filter));
        }

        [Test]
        public void GetLogFilters_ShouldReturnReadOnlyCollection()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            var filters = logger.GetLogFilters();

            // Assert
            Assert.That(filters, Is.InstanceOf<ReadOnlyCollection<ILogFilter>>());
        }

        [Test]
        public void UpdateConfiguration_ShouldReplaceEffectiveLogLevel()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.INFO);

            // Act
            logger.UpdateConfiguration(LogLevel.ERROR, new List<ILogAppender>());

            // Assert
            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void UpdateConfiguration_ShouldReplaceAppenderCollection()
        {
            // Arrange
            var logger = CreateLogger();
            var originalAppender = CreateAppender();
            var replacementAppender = CreateAppender();
            logger.AddAppender(originalAppender);

            // Act
            logger.UpdateConfiguration(LogLevel.INFO, new List<ILogAppender> { replacementAppender });

            // Assert
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(1));
            Assert.That(logger.GetLogAppenders().Single(), Is.SameAs(replacementAppender));
        }

        [Test]
        public void UpdateConfiguration_ShouldDiscardExistingAppenders()
        {
            // Arrange
            var logger = CreateLogger();
            var originalAppender = CreateAppender();
            logger.AddAppender(originalAppender);

            // Act
            logger.UpdateConfiguration(LogLevel.INFO, new List<ILogAppender>());

            // Assert
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(0));
        }

        [Test]
        public void UpdateConfiguration_AfterUpdate_ShouldUseNewConfiguration()
        {
            // Arrange
            var logger = CreateLogger(logLevel: LogLevel.INFO);
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            logger.AddAppender(appender.Object);

            // Act
            logger.UpdateConfiguration(LogLevel.ERROR, new List<ILogAppender>());
            logger.Log(LogLevel.INFO, "hidden");

            // Assert
            appender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Never);
        }

        [Test]
        public void Log_WithNullMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var logger = CreateLogger();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => logger.Log(LogLevel.INFO, null));
        }

        [Test]
        public void Log_WithEmptyMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var logger = CreateLogger();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => logger.Log(LogLevel.INFO, string.Empty));
        }

        [Test]
        public void Log_WithWhitespaceMessage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var logger = CreateLogger();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => logger.Log(LogLevel.INFO, "   "));
        }

        [Test]
        public void Log_WithUnicodeMessage_ShouldAppendSuccessfully()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            // Act
            logger.Log(LogLevel.INFO, "こんにちは世界 🌍");

            // Assert
            Assert.That(captured.Message, Is.EqualTo("こんにちは世界 🌍"));
        }

        [Test]
        public void Log_WithVeryLongMessage_ShouldAppendSuccessfully()
        {
            // Arrange
            var logger = CreateLogger();
            LogMessage captured = null;
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(message => captured = message);
            logger.AddAppender(appender.Object);

            var message = new string('X', 10_000);

            // Act
            logger.Log(LogLevel.INFO, message);

            // Assert
            Assert.That(captured.Message, Is.EqualTo(message));
        }

        [Test]
        public void AddAppender_WithNullAppender_ShouldAllowAddition()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            logger.AddAppender(null);

            // Assert
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(2));
        }

        [Test]
        public void AddFilter_WithNullFilter_ShouldAllowAddition()
        {
            // Arrange
            var logger = CreateLogger();

            // Act
            logger.AddFilter(null);

            // Assert
            Assert.That(logger.GetLogFilters().Count, Is.EqualTo(1));
        }

        [Test]
        public void UpdateConfiguration_WithNullAppenderCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            var logger = CreateLogger();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => logger.UpdateConfiguration(LogLevel.INFO, null));
        }

        [Test]
        public void Log_WhenAppenderCollectionChangesDuringLogging_ShouldUseSnapshot()
        {
            // Arrange
            var logger = CreateLogger();
            var firstAppender = new Mock<ILogAppender>();
            var secondAppender = new Mock<ILogAppender>();
            firstAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            secondAppender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            firstAppender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(_ => logger.RemoveAppender(secondAppender.Object));
            logger.AddAppender(firstAppender.Object);
            logger.AddAppender(secondAppender.Object);

            // Act
            logger.Log(LogLevel.INFO, "hello");

            // Assert
            firstAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
            secondAppender.Verify(x => x.Append(It.IsAny<LogMessage>()), Times.Once);
        }

        [Test]
        public void Log_DuringConcurrentConfigurationUpdate_ShouldNotThrowCollectionModifiedException()
        {
            // Arrange
            var logger = CreateLogger();
            var appender = new Mock<ILogAppender>();
            appender.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            appender.Setup(x => x.Append(It.IsAny<LogMessage>())).Callback<LogMessage>(_ => logger.UpdateConfiguration(LogLevel.INFO, new List<ILogAppender>()));
            logger.AddAppender(appender.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => logger.Log(LogLevel.INFO, "hello"));
        }
    }
}
