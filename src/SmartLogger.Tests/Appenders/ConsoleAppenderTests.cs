using Moq;
using SmartLogger.Appenders;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders;

[TestFixture]
[NonParallelizable]
public class ConsoleAppenderTests
{
    private TextWriter _originalOut = null!;
    private TextWriter _originalError = null!;

    [SetUp]
    public void SetUp()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static LogMessage CreateMessage(
        LogLevel level,
        string message = "Test message")
    {
        return new LogMessage.Builder()
            .WithLevel(level)
            .WithMessage(message)
            .Build();
    }

    private static Mock<ILogOutputFormatterStrategy> CreateFormatter(
        string formattedMessage = "Formatted message")
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>();

        formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns(formattedMessage);

        return formatter;
    }

    private static (StringWriter output, StringWriter error) CaptureConsole()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        Console.SetOut(output);
        Console.SetError(error);

        return (output, error);
    }

    // -------------------------------------------------------------------------
    // Initialization Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Constructor_WithoutArguments_ShouldSetDefaultLogLevelToInfo()
    {
        var appender = new ConsoleAppender();

        Assert.That(
            appender.GetLogLevel(LogLevel.DEBUG),
            Is.EqualTo(LogLevel.INFO));
    }

    [Test]
    public void Constructor_WithoutArguments_ShouldInitializeDefaultFormatter()
    {
        var appender = new ConsoleAppender();

        Assert.That(appender.GetFormatter(), Is.Not.Null);
    }

    [Test]
    public void Constructor_WithLogLevel_ShouldSetConfiguredLogLevel()
    {
        var formatter = CreateFormatter().Object;

        var appender = new ConsoleAppender(
            LogLevel.ERROR,
            formatter);

        Assert.That(
            appender.GetLogLevel(LogLevel.INFO),
            Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void Constructor_WithFormatter_ShouldSetConfiguredFormatter()
    {
        var formatter = CreateFormatter().Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter);

        Assert.That(
            appender.GetFormatter(),
            Is.SameAs(formatter));
    }

    // -------------------------------------------------------------------------
    // Log Level Tests
    // -------------------------------------------------------------------------

    [Test]
    public void IsEnabled_WithLevelEqualToThreshold_ShouldReturnTrue()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        Assert.That(
            appender.IsEnabled(LogLevel.INFO),
            Is.True);
    }

    [Test]
    public void IsEnabled_WithLevelAboveThreshold_ShouldReturnTrue()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        Assert.That(
            appender.IsEnabled(LogLevel.ERROR),
            Is.True);
    }

    [Test]
    public void IsEnabled_WithLevelBelowThreshold_ShouldReturnFalse()
    {
        var appender = new ConsoleAppender(
            LogLevel.WARNING,
            CreateFormatter().Object);

        Assert.That(
            appender.IsEnabled(LogLevel.INFO),
            Is.False);
    }

    [Test]
    public void IsEnabled_WithInfoThreshold_ShouldFollowLogLevelOrdering()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        Assert.Multiple(() =>
        {
            Assert.That(appender.IsEnabled(LogLevel.NONE), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.DEBUG), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.INFO), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.FATAL), Is.True);
        });
    }

    [Test]
    public void IsEnabled_WithWarningThreshold_ShouldFollowLogLevelOrdering()
    {
        var appender = new ConsoleAppender(
            LogLevel.WARNING,
            CreateFormatter().Object);

        Assert.Multiple(() =>
        {
            Assert.That(appender.IsEnabled(LogLevel.NONE), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.DEBUG), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.INFO), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.FATAL), Is.True);
        });
    }

    [Test]
    public void IsEnabled_WithErrorThreshold_ShouldFollowLogLevelOrdering()
    {
        var appender = new ConsoleAppender(
            LogLevel.ERROR,
            CreateFormatter().Object);

        Assert.Multiple(() =>
        {
            Assert.That(appender.IsEnabled(LogLevel.NONE), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.DEBUG), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.INFO), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.True);
            Assert.That(appender.IsEnabled(LogLevel.FATAL), Is.True);
        });
    }

    [Test]
    public void IsEnabled_WithFatalThreshold_ShouldFollowLogLevelOrdering()
    {
        var appender = new ConsoleAppender(
            LogLevel.FATAL,
            CreateFormatter().Object);

        Assert.Multiple(() =>
        {
            Assert.That(appender.IsEnabled(LogLevel.NONE), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.DEBUG), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.INFO), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.WARNING), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.ERROR), Is.False);
            Assert.That(appender.IsEnabled(LogLevel.FATAL), Is.True);
        });
    }

    [Test]
    public void SetLogLevel_WithNewLevel_ShouldUpdateThreshold()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        appender.SetLogLevel(LogLevel.ERROR);

        Assert.That(
            appender.GetLogLevel(LogLevel.DEBUG),
            Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void SetLogLevel_ThenAppend_ShouldUseUpdatedThreshold()
    {
        var formatter = CreateFormatter("formatted").Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter);

        var (output, error) = CaptureConsole();

        appender.SetLogLevel(LogLevel.ERROR);

        // INFO is below ERROR threshold.
        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });

        // ERROR meets the threshold and is routed to stderr.
        appender.Append(CreateMessage(LogLevel.ERROR));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(
                error.ToString(),
                Is.EqualTo("formatted" + Environment.NewLine));
        });
    }

    [Test]
    public void GetLogLevel_WithAnyArgument_ShouldReturnConfiguredThreshold()
    {
        var appender = new ConsoleAppender(
            LogLevel.ERROR,
            CreateFormatter().Object);

        Assert.Multiple(() =>
        {
            Assert.That(
                appender.GetLogLevel(LogLevel.NONE),
                Is.EqualTo(LogLevel.ERROR));

            Assert.That(
                appender.GetLogLevel(LogLevel.DEBUG),
                Is.EqualTo(LogLevel.ERROR));

            Assert.That(
                appender.GetLogLevel(LogLevel.FATAL),
                Is.EqualTo(LogLevel.ERROR));
        });
    }

    // -------------------------------------------------------------------------
    // LogLevel.NONE Tests
    // -------------------------------------------------------------------------

    [Test]
    public void IsEnabled_WithNoneThreshold_ShouldEnableAllDefinedLevels()
    {
        var appender = new ConsoleAppender(
            LogLevel.NONE,
            CreateFormatter().Object);

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
    public void IsEnabled_WithNoneMessageLevel_ShouldFollowNumericComparison()
    {
        var appender = new ConsoleAppender(
            LogLevel.NONE,
            CreateFormatter().Object);

        Assert.That(
            appender.IsEnabled(LogLevel.NONE),
            Is.True);
    }

    // -------------------------------------------------------------------------
    // Append Input Validation Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Append_WithNullMessage_ShouldNotWriteOutput()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(null!);

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    [Test]
    public void Append_WithNullMessage_ShouldNotInvokeFormatter()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        appender.Append(null!);

        formatter.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Never);
    }

    [Test]
    public void Append_WithLevelBelowThreshold_ShouldNotFormatMessage()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.ERROR,
            formatter.Object);

        appender.Append(CreateMessage(LogLevel.WARNING));

        formatter.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Never);
    }

    [Test]
    public void Append_WithLevelBelowThreshold_ShouldNotWriteOutput()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.ERROR,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.WARNING));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    // -------------------------------------------------------------------------
    // Formatting Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Append_WithEnabledMessage_ShouldFormatMessage()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var message = CreateMessage(LogLevel.INFO);

        appender.Append(message);

        formatter.Verify(
            x => x.Format(message),
            Times.Once);
    }

    [Test]
    public void Append_WithEnabledMessage_ShouldWriteFormattedMessage()
    {
        var formatter = CreateFormatter("Expected formatted output");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(
            output.ToString(),
            Is.EqualTo("Expected formatted output" + Environment.NewLine));
    }

    [Test]
    public void Append_WithEnabledMessage_ShouldFormatExactlyOnce()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        appender.Append(CreateMessage(LogLevel.INFO));

        formatter.Verify(
            x => x.Format(It.IsAny<LogMessage>()),
            Times.Once);
    }

    [Test]
    public void Append_WhenFormatterThrows_ShouldPropagateException()
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>();

        formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Throws<InvalidOperationException>();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        Assert.Throws<InvalidOperationException>(() =>
            appender.Append(CreateMessage(LogLevel.INFO)));
    }

    [Test]
    public void Append_WhenFormatterThrows_ShouldNotWriteOutput()
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>();

        formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Throws<InvalidOperationException>();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, error) = CaptureConsole();

        Assert.Throws<InvalidOperationException>(() =>
            appender.Append(CreateMessage(LogLevel.INFO)));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    [Test]
    public void Append_WhenFormatterReturnsEmptyString_ShouldWriteEmptyLine()
    {
        var formatter = CreateFormatter(string.Empty);

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(
            output.ToString(),
            Is.EqualTo(Environment.NewLine));
    }

    [Test]
    public void Append_WhenFormatterReturnsNull_ShouldWriteEmptyLine()
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>();

        formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns((string)null!);

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(
            output.ToString(),
            Is.EqualTo(Environment.NewLine));
    }

    // -------------------------------------------------------------------------
    // Console Routing Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Append_WithDebugMessage_ShouldWriteToStandardOutput()
    {
        AssertWritesToStandardOutput(LogLevel.DEBUG);
    }

    [Test]
    public void Append_WithInfoMessage_ShouldWriteToStandardOutput()
    {
        AssertWritesToStandardOutput(LogLevel.INFO);
    }

    [Test]
    public void Append_WithWarningMessage_ShouldWriteToStandardOutput()
    {
        AssertWritesToStandardOutput(LogLevel.WARNING);
    }

    [Test]
    public void Append_WithErrorMessage_ShouldWriteToStandardError()
    {
        var formatter = CreateFormatter("error output");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.ERROR));

        Assert.Multiple(() =>
        {
            Assert.That(error.ToString(), Is.EqualTo("error output" + Environment.NewLine));
            Assert.That(output.ToString(), Is.Empty);
        });
    }

    [Test]
    public void Append_WithFatalMessage_ShouldWriteToStandardError()
    {
        var formatter = CreateFormatter("fatal output");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.FATAL));

        Assert.Multiple(() =>
        {
            Assert.That(error.ToString(), Is.EqualTo("fatal output" + Environment.NewLine));
            Assert.That(output.ToString(), Is.Empty);
        });
    }

    [Test]
    public void Append_WithErrorMessage_ShouldNotWriteToStandardOutput()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.ERROR));

        Assert.That(output.ToString(), Is.Empty);
    }

    [Test]
    public void Append_WithFatalMessage_ShouldNotWriteToStandardOutput()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.FATAL));

        Assert.That(output.ToString(), Is.Empty);
    }

    [Test]
    public void Append_WithNoneMessage_ShouldWriteToStandardOutput_WhenEnabled()
    {
        var formatter = CreateFormatter("none output");

        var appender = new ConsoleAppender(
            LogLevel.NONE,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.NONE));

        Assert.Multiple(() =>
        {
            Assert.That(
                output.ToString(),
                Is.EqualTo("none output" + Environment.NewLine));

            Assert.That(error.ToString(), Is.Empty);
        });
    }

    // -------------------------------------------------------------------------
    // Formatter Configuration Tests
    // -------------------------------------------------------------------------

    [Test]
    public void GetFormatter_ShouldReturnConfiguredFormatter()
    {
        var formatter = CreateFormatter().Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter);

        Assert.That(
            appender.GetFormatter(),
            Is.SameAs(formatter));
    }

    [Test]
    public void SetFormatter_WithValidFormatter_ShouldReplaceExistingFormatter()
    {
        var firstFormatter = CreateFormatter("first").Object;
        var secondFormatter = CreateFormatter("second").Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            firstFormatter);

        appender.SetFormatter(secondFormatter);

        Assert.That(
            appender.GetFormatter(),
            Is.SameAs(secondFormatter));
    }

    [Test]
    public void SetFormatter_ThenAppend_ShouldUseNewFormatter()
    {
        var firstFormatter = CreateFormatter("first").Object;
        var secondFormatter = CreateFormatter("second").Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            firstFormatter);

        var (output, _) = CaptureConsole();

        appender.SetFormatter(secondFormatter);
        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.That(
            output.ToString(),
            Is.EqualTo("second" + Environment.NewLine));
    }

    [Test]
    public void SetFormatter_WithNullFormatter_ShouldThrowArgumentNullException()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        Assert.Throws<ArgumentNullException>(() =>
            appender.SetFormatter(null!));
    }

    [Test]
    public void SetFormatter_WithNullFormatter_ShouldNotReplaceExistingFormatter()
    {
        var originalFormatter = CreateFormatter().Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            originalFormatter);

        Assert.Throws<ArgumentNullException>(() =>
            appender.SetFormatter(null!));

        Assert.That(
            appender.GetFormatter(),
            Is.SameAs(originalFormatter));
    }

    // -------------------------------------------------------------------------
    // Runtime Configuration Tests
    // -------------------------------------------------------------------------

    [Test]
    public void SetLogLevel_AndSetFormatter_ShouldUpdateBothConfigurations()
    {
        var formatter = CreateFormatter().Object;

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter);

        appender.SetLogLevel(LogLevel.ERROR);

        var newFormatter = CreateFormatter().Object;
        appender.SetFormatter(newFormatter);

        Assert.Multiple(() =>
        {
            Assert.That(
                appender.GetLogLevel(LogLevel.INFO),
                Is.EqualTo(LogLevel.ERROR));

            Assert.That(
                appender.GetFormatter(),
                Is.SameAs(newFormatter));
        });
    }

    [Test]
    public void SetLogLevel_AndSetFormatter_ThenAppend_ShouldUseLatestConfiguration()
    {
        var formatter = CreateFormatter("updated output");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter("old output").Object);

        var (_, error) = CaptureConsole();

        appender.SetLogLevel(LogLevel.ERROR);
        appender.SetFormatter(formatter.Object);

        appender.Append(CreateMessage(LogLevel.ERROR));

        Assert.That(
            error.ToString(),
            Is.EqualTo("updated output" + Environment.NewLine));
    }

    // -------------------------------------------------------------------------
    // Boundary Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Append_WithNoneThreshold_ShouldFollowDefinedComparisonBehavior()
    {
        var formatter = CreateFormatter("output");

        var appender = new ConsoleAppender(
            LogLevel.NONE,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.DEBUG));
        appender.Append(CreateMessage(LogLevel.INFO));
        appender.Append(CreateMessage(LogLevel.WARNING));
        appender.Append(CreateMessage(LogLevel.ERROR));
        appender.Append(CreateMessage(LogLevel.FATAL));

        Assert.That(
            output.ToString(),
            Is.EqualTo(
                "output" + Environment.NewLine +
                "output" + Environment.NewLine +
                "output" + Environment.NewLine));

        Assert.That(
            error.ToString(),
            Is.EqualTo(
                "output" + Environment.NewLine +
                "output" + Environment.NewLine));
    }

    [Test]
    public void Append_WithFatalThreshold_ShouldOnlyWriteFatalMessages()
    {
        var formatter = CreateFormatter("fatal");

        var appender = new ConsoleAppender(
            LogLevel.FATAL,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.DEBUG));
        appender.Append(CreateMessage(LogLevel.INFO));
        appender.Append(CreateMessage(LogLevel.WARNING));
        appender.Append(CreateMessage(LogLevel.ERROR));
        appender.Append(CreateMessage(LogLevel.FATAL));

        Assert.That(output.ToString(), Is.Empty);
        Assert.That(
            error.ToString(),
            Is.EqualTo("fatal" + Environment.NewLine));
    }

    [Test]
    public void Append_WithMessageEqualToThreshold_ShouldWriteMessage()
    {
        var formatter = CreateFormatter("threshold");

        var appender = new ConsoleAppender(
            LogLevel.WARNING,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.WARNING));

        Assert.That(
            output.ToString(),
            Is.EqualTo("threshold" + Environment.NewLine));
    }

    [Test]
    public void Append_WithMessageImmediatelyBelowThreshold_ShouldNotWriteMessage()
    {
        var formatter = CreateFormatter();

        var appender = new ConsoleAppender(
            LogLevel.WARNING,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    [Test]
    public void Append_WithMessageImmediatelyAboveThreshold_ShouldWriteMessage()
    {
        var formatter = CreateFormatter("above");

        var appender = new ConsoleAppender(
            LogLevel.WARNING,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.ERROR));

        Assert.Multiple(() =>
        {
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(
                error.ToString(),
                Is.EqualTo("above" + Environment.NewLine));
        });
    }

    // -------------------------------------------------------------------------
    // Message Content Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Append_WithUnicodeMessage_ShouldWriteFormattedMessage()
    {
        const string formatted = "தமிழ் 🚀 日本語";

        var formatter = CreateFormatter(formatted);

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO, "Unicode message"));

        Assert.That(
            output.ToString(),
            Is.EqualTo(formatted + Environment.NewLine));
    }

    [Test]
    public void Append_WithVeryLongMessage_ShouldWriteFormattedMessage()
    {
        var formatted = new string('A', 100_000);

        var formatter = CreateFormatter(formatted);

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO, "Long message"));

        Assert.That(
            output.ToString(),
            Is.EqualTo(formatted + Environment.NewLine));
    }

    [Test]
    public void Append_WithSpecialCharacters_ShouldWriteFormattedMessage()
    {
        const string formatted = "Line1\tLine2\r\nLine3|{}[]<>";

        var formatter = CreateFormatter(formatted);

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        appender.Append(CreateMessage(LogLevel.INFO, "Special characters"));

        Assert.That(
            output.ToString(),
            Is.EqualTo(formatted + Environment.NewLine));
    }

    // -------------------------------------------------------------------------
    // Concurrency Tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task Append_Concurrently_ShouldCompleteWithoutException()
    {
        var formatter = CreateFormatter("concurrent");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        const int numberOfMessages = 100;

        var tasks = new Task[numberOfMessages];

        for (var i = 0; i < numberOfMessages; i++)
        {
            var messageNumber = i;

            tasks[i] = Task.Run(() =>
            {
                appender.Append(
                    CreateMessage(
                        LogLevel.INFO,
                        $"Message {messageNumber}"));
            });
        }

        Assert.DoesNotThrowAsync(
            async () => await Task.WhenAll(tasks));

        Assert.That(
            output.ToString(),
            Does.Contain("concurrent"));
    }

    [Test]
    public async Task Append_Concurrently_ShouldWriteCompleteFormattedMessages()
    {
        const int numberOfMessages = 100;

        var formatter = new Mock<ILogOutputFormatterStrategy>();

        formatter
            .Setup(x => x.Format(It.IsAny<LogMessage>()))
            .Returns<LogMessage>(message =>
                $"Message-{message.Message}");

        var appender = new ConsoleAppender(
            LogLevel.INFO,
            formatter.Object);

        var (output, _) = CaptureConsole();

        var tasks = new Task[numberOfMessages];

        for (var i = 0; i < numberOfMessages; i++)
        {
            var messageNumber = i;

            tasks[i] = Task.Run(() =>
            {
                appender.Append(
                    CreateMessage(
                        LogLevel.INFO,
                        messageNumber.ToString()));
            });
        }

        await Task.WhenAll(tasks);

        var lines = output
            .ToString()
            .Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

        Assert.That(lines.Length, Is.EqualTo(numberOfMessages));

        foreach (var line in lines)
        {
            Assert.That(
                line,
                Does.Match(@"^Message-\d+$"));
        }
    }

    [Test]
    public async Task SetLogLevel_AndIsEnabled_Concurrently_ShouldCompleteWithoutException()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter().Object);

        var tasks = new Task[100];

        for (var i = 0; i < tasks.Length; i++)
        {
            var index = i;

            tasks[i] = Task.Run(() =>
            {
                if (index % 2 == 0)
                {
                    appender.SetLogLevel(LogLevel.DEBUG);
                }
                else
                {
                    appender.SetLogLevel(LogLevel.ERROR);
                }

                _ = appender.IsEnabled(LogLevel.INFO);
            });
        }

        Assert.DoesNotThrowAsync(
            async () => await Task.WhenAll(tasks));
    }

    [Test]
    public async Task SetFormatter_AndAppend_Concurrently_ShouldCompleteWithoutException()
    {
        var appender = new ConsoleAppender(
            LogLevel.INFO,
            CreateFormatter("initial").Object);

        var (output, _) = CaptureConsole();

        var tasks = new Task[100];

        for (var i = 0; i < tasks.Length; i++)
        {
            var index = i;

            tasks[i] = Task.Run(() =>
            {
                if (index % 2 == 0)
                {
                    appender.SetFormatter(
                        CreateFormatter("formatter-a").Object);
                }
                else
                {
                    appender.Append(
                        CreateMessage(
                            LogLevel.INFO,
                            $"Message-{index}"));
                }
            });
        }

        Assert.DoesNotThrowAsync(
            async () => await Task.WhenAll(tasks));

        Assert.That(output.ToString(), Is.Not.Null);
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private static void AssertWritesToStandardOutput(LogLevel level)
    {
        var formatter = CreateFormatter("standard output");

        var appender = new ConsoleAppender(
            level,
            formatter.Object);

        var (output, error) = CaptureConsole();

        appender.Append(CreateMessage(level));

        Assert.Multiple(() =>
        {
            Assert.That(
                output.ToString(),
                Is.EqualTo("standard output" + Environment.NewLine));

            Assert.That(
                error.ToString(),
                Is.Empty);
        });
    }
}