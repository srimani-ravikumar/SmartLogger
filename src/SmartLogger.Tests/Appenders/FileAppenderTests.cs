using Moq;
using SmartLogger.Appenders;
using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders;

[TestFixture]
public class FileAppenderTests
{
    private string _tempDirectory = null!;
    private FileConfiguration _configuration = null!;
    private Mock<ILogOutputFormatterStrategy> _formatter = null!;
    private Mock<IFileNamingStrategy> _namingStrategy = null!;
    private Mock<IRollingStrategy> _rollingStrategy = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerTests",
            Guid.NewGuid().ToString("N"));

        _configuration = new FileConfiguration
        {
            Directory = Path.Combine(_tempDirectory, "Logs"),
            Archive = new ArchiveConfiguration
            {
                Enabled = false,
                Directory = Path.Combine(_tempDirectory, "Archive"),
                Compress = false
            }
        };

        _formatter = new Mock<ILogOutputFormatterStrategy>();
        _namingStrategy = new Mock<IFileNamingStrategy>();
        _rollingStrategy = new Mock<IRollingStrategy>();

        _namingStrategy
            .Setup(x => x.CreateActiveFileName())
            .Returns("application.log");

        _namingStrategy
            .Setup(x => x.CreateRolledFileName(It.IsAny<int>()))
            .Returns<int>(index => $"application-{index}.log");

        _formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns<LogMessage>(message => message.Message);

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(false);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public void Constructor_WithValidConfiguration_ShouldInitializeAppender()
    {
        var appender = CreateAppender();

        Assert.That(appender, Is.Not.Null);
        Assert.That(appender.GetFormatter(), Is.SameAs(_formatter.Object));
        Assert.That(appender.GetLogLevel(LogLevel.DEBUG), Is.EqualTo(LogLevel.INFO));
    }

    [Test]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FileAppender(
                null!,
                LogLevel.INFO,
                _formatter.Object,
                _rollingStrategy.Object,
                _namingStrategy.Object));
    }

    [Test]
    public void Constructor_WithNullNamingStrategy_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FileAppender(
                _configuration,
                LogLevel.INFO,
                _formatter.Object,
                _rollingStrategy.Object,
                null!));
    }

    [Test]
    public void Constructor_WithNullRollingStrategy_ShouldInitializeAppender()
    {
        var appender = CreateAppender(rollingStrategy: null);

        Assert.That(appender, Is.Not.Null);
    }

    [Test]
    public void Append_WithNullMessage_ShouldNotInvokeFormatter()
    {
        var appender = CreateAppender();

        appender.Append(null!);

        _formatter.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Never);
    }

    [Test]
    public void Append_WithDisabledMessage_ShouldNotInvokeFormatter()
    {
        var appender = CreateAppender(logLevel: LogLevel.WARNING);
        var message = CreateMessage(LogLevel.INFO);

        appender.Append(message);

        _formatter.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Never);
    }

    [Test]
    public void Append_WithDisabledMessage_ShouldNotWrite()
    {
        var appender = CreateAppender(logLevel: LogLevel.WARNING);
        var message = CreateMessage(LogLevel.INFO);

        appender.Append(message);

        Assert.That(ReadActiveFile(), Is.Empty);
    }

    [Test]
    public void Append_WithEnabledMessage_ShouldFormatMessage()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);
        var message = CreateMessage(LogLevel.INFO);

        appender.Append(message);

        _formatter.Verify(
            x => x.Format(It.Is<LogMessage>(m => ReferenceEquals(m, message))),
            Times.Once);
    }

    [Test]
    public void Append_WithEnabledMessage_ShouldWriteFormattedMessage()
    {
        const string formattedMessage = "[INFO] formatted";
        _formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns(formattedMessage);

        var appender = CreateAppender();
        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(ReadActiveFile(), Is.EqualTo(formattedMessage + Environment.NewLine));
    }

    [Test]
    public void Append_WithMultipleEnabledMessages_ShouldAppendAllMessages()
    {
        var appender = CreateAppender();

        appender.Append(CreateMessage(LogLevel.INFO, "first"));
        appender.Append(CreateMessage(LogLevel.ERROR, "second"));

        Assert.That(
            ReadActiveFile(),
            Is.EqualTo($"first{Environment.NewLine}second{Environment.NewLine}"));
    }

    [Test]
    public void IsEnabled_WithEqualLogLevel_ShouldReturnTrue()
    {
        var appender = CreateAppender(logLevel: LogLevel.WARNING);

        Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.True);
    }

    [Test]
    public void IsEnabled_WithHigherLogLevel_ShouldReturnTrue()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);

        Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.True);
    }

    [Test]
    public void IsEnabled_WithLowerLogLevel_ShouldReturnFalse()
    {
        var appender = CreateAppender(logLevel: LogLevel.ERROR);

        Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.False);
    }

    [Test]
    public void IsEnabled_WithNoneThreshold_ShouldEnableAllLevels()
    {
        var appender = CreateAppender(logLevel: LogLevel.NONE);

        Assert.Multiple(() =>
        {
            Assert.That(appender.IsEnabled(LogLevel.NONE), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.DEBUG), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.INFO), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.FATAL), Is.True);
        });
    }

    [Test]
    public void IsEnabled_WithNoneMessageLevel_ShouldFollowThreshold()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);

        Assert.That(appender.IsEnabled(LogLevel.NONE), Is.False);
    }

    [Test]
    public void SetLogLevel_ShouldUpdateCurrentLogLevel()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);

        appender.SetLogLevel(LogLevel.ERROR);

        Assert.That(appender.GetLogLevel(LogLevel.INFO), Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void SetLogLevel_ShouldAffectSubsequentAppendOperations()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);

        appender.SetLogLevel(LogLevel.ERROR);
        appender.Append(CreateMessage(LogLevel.WARNING, "ignored"));
        appender.Append(CreateMessage(LogLevel.ERROR, "written"));

        Assert.That(
            ReadActiveFile(),
            Is.EqualTo($"written{Environment.NewLine}"));
    }

    [Test]
    public void GetLogLevel_WithDifferentArgument_ShouldReturnConfiguredLogLevel()
    {
        var appender = CreateAppender(logLevel: LogLevel.ERROR);

        Assert.That(appender.GetLogLevel(LogLevel.DEBUG), Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void GetFormatter_AfterConstruction_ShouldReturnConfiguredFormatter()
    {
        var appender = CreateAppender();

        Assert.That(appender.GetFormatter(), Is.SameAs(_formatter.Object));
    }

    [Test]
    public void SetFormatter_WithValidFormatter_ShouldReplaceCurrentFormatter()
    {
        var appender = CreateAppender();
        var replacement = new Mock<ILogOutputFormatterStrategy>().Object;

        appender.SetFormatter(replacement);

        Assert.That(appender.GetFormatter(), Is.SameAs(replacement));
    }

    [Test]
    public void SetFormatter_ShouldUseNewFormatterForSubsequentMessages()
    {
        var replacement = new Mock<ILogOutputFormatterStrategy>();
        replacement
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns("replacement");

        var appender = CreateAppender();
        appender.SetFormatter(replacement.Object);

        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(ReadActiveFile(), Is.EqualTo($"replacement{Environment.NewLine}"));
        replacement.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Once);
    }

    [Test]
    public void SetFormatter_WithNullFormatter_ShouldThrowArgumentNullException()
    {
        var appender = CreateAppender();

        Assert.Throws<ArgumentNullException>(() => appender.SetFormatter(null!));
    }

    [Test]
    public void SetFormatter_WithNullFormatter_ShouldPreserveExistingFormatter()
    {
        var appender = CreateAppender();

        Assert.Throws<ArgumentNullException>(() => appender.SetFormatter(null!));

        Assert.That(appender.GetFormatter(), Is.SameAs(_formatter.Object));
    }

    [Test]
    public void UpdateConfiguration_WithValidValues_ShouldUpdateConfiguration()
    {
        var appender = CreateAppender(logLevel: LogLevel.INFO);
        var replacement = new Mock<ILogOutputFormatterStrategy>().Object;

        appender.UpdateConfiguration(LogLevel.ERROR, replacement);

        Assert.Multiple(() =>
        {
            Assert.That(appender.GetLogLevel(LogLevel.INFO), Is.EqualTo(LogLevel.ERROR));
            Assert.That(appender.GetFormatter(), Is.SameAs(replacement));
        });
    }

    [Test]
    public void UpdateConfiguration_ShouldAffectSubsequentAppendOperations()
    {
        var replacement = new Mock<ILogOutputFormatterStrategy>();
        replacement
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns("updated");

        var appender = CreateAppender(logLevel: LogLevel.INFO);
        appender.UpdateConfiguration(LogLevel.ERROR, replacement.Object);

        appender.Append(CreateMessage(LogLevel.WARNING, "ignored"));
        appender.Append(CreateMessage(LogLevel.ERROR, "accepted"));

        Assert.That(
            ReadActiveFile(),
            Is.EqualTo($"updated{Environment.NewLine}"));
    }

    [Test]
    public void UpdateConfiguration_WithNullFormatter_ShouldStoreNullFormatter()
    {
        var appender = CreateAppender();

        appender.UpdateConfiguration(LogLevel.INFO, null!);

        Assert.That(appender.GetFormatter(), Is.Null);
    }

    [Test]
    public void Append_WhenFormatterThrows_ShouldPropagateException()
    {
        var expected = new InvalidOperationException("format failed");
        _formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Throws(expected);

        var appender = CreateAppender();

        var actual = Assert.Throws<InvalidOperationException>(() =>
            appender.Append(CreateMessage(LogLevel.INFO)));

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void Constructor_WithValidConfiguration_ShouldCreateActiveLogFile()
    {
        CreateAppender();

        Assert.That(
            File.Exists(Path.Combine(_configuration.Directory, "application.log")),
            Is.True);
    }

    [Test]
    public void Constructor_WithMissingDirectory_ShouldCreateDirectory()
    {
        CreateAppender();

        Assert.That(Directory.Exists(_configuration.Directory), Is.True);
    }

    [Test]
    public void Append_WhenRollingStrategyDoesNotRequestRoll_ShouldWriteToExistingActiveFile()
    {
        var appender = CreateAppender();

        appender.Append(CreateMessage(LogLevel.INFO, "first"));
        appender.Append(CreateMessage(LogLevel.INFO, "second"));

        _rollingStrategy.Verify(
            x => x.ShouldRoll(It.IsAny<string>()),
            Times.AtLeastOnce);

        Assert.That(
            ReadActiveFile(),
            Is.EqualTo($"first{Environment.NewLine}second{Environment.NewLine}"));
    }

    private FileAppender CreateAppender(
        LogLevel logLevel = LogLevel.INFO,
        IRollingStrategy? rollingStrategy = null)
    {
        return new FileAppender(
            _configuration,
            logLevel,
            _formatter.Object,
            rollingStrategy ?? _rollingStrategy.Object,
            _namingStrategy.Object);
    }

    private static LogMessage CreateMessage(
        LogLevel level,
        string message = "test message")
    {
        return new LogMessage.Builder()
            .WithLevel(level)
            .WithMessage(message)
            .Build();
    }

    private string ReadActiveFile()
    {
        var path = Path.Combine(_configuration.Directory, "application.log");
        return File.Exists(path)
            ? File.ReadAllText(path)
            : string.Empty;
    }
}