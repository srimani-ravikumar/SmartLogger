using SmartLogger.Core;
using SmartLogger.Filters;

namespace SmartLogger.Tests.Filters;

[TestFixture]
public class LevelFilterTests
{
    [Test]
    public void Constructor_WithoutLevel_ShouldDefaultToDebug()
    {
        var filter = new LevelFilter();

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.DEBUG));
    }

    [Test]
    public void Constructor_WithLogLevel_ShouldSetThreshold()
    {
        var filter = new LevelFilter(LogLevel.ERROR);

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void Constructor_WithNoneLevel_ShouldSetThresholdToNone()
    {
        var filter = new LevelFilter(LogLevel.NONE);

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.NONE));
    }

    [Test]
    public void Constructor_WithFatalLevel_ShouldSetThresholdToFatal()
    {
        var filter = new LevelFilter(LogLevel.FATAL);

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.FATAL));
    }

    [Test]
    public void ShouldLog_WithNullMessage_ShouldReturnFalse()
    {
        var filter = new LevelFilter(LogLevel.DEBUG);

        Assert.That(filter.ShouldLog(null!), Is.False);
    }

    [Test]
    public void ShouldLog_WithEqualLevel_ShouldReturnTrue()
    {
        var filter = new LevelFilter(LogLevel.INFO);
        var message = CreateMessage(LogLevel.INFO);

        Assert.That(filter.ShouldLog(message), Is.True);
    }

    [Test]
    public void ShouldLog_WithHigherLevel_ShouldReturnTrue()
    {
        var filter = new LevelFilter(LogLevel.INFO);
        var message = CreateMessage(LogLevel.ERROR);

        Assert.That(filter.ShouldLog(message), Is.True);
    }

    [Test]
    public void ShouldLog_WithLowerLevel_ShouldReturnFalse()
    {
        var filter = new LevelFilter(LogLevel.ERROR);
        var message = CreateMessage(LogLevel.WARNING);

        Assert.That(filter.ShouldLog(message), Is.False);
    }

    [Test]
    public void ShouldLog_WithDebugThreshold_ShouldAllowDebugAndAbove()
    {
        var filter = new LevelFilter(LogLevel.DEBUG);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithInfoThreshold_ShouldFilterBelowInfo()
    {
        var filter = new LevelFilter(LogLevel.INFO);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.NONE)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithWarningThreshold_ShouldFilterBelowWarning()
    {
        var filter = new LevelFilter(LogLevel.WARNING);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.NONE)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithErrorThreshold_ShouldFilterBelowError()
    {
        var filter = new LevelFilter(LogLevel.ERROR);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.NONE)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithFatalThreshold_ShouldAllowOnlyFatal()
    {
        var filter = new LevelFilter(LogLevel.FATAL);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.NONE)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithNoneThreshold_ShouldAllowAllLevels()
    {
        var filter = new LevelFilter(LogLevel.NONE);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.NONE)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.DEBUG)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.INFO)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.WARNING)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_WithNoneMessageAndDebugThreshold_ShouldReturnFalse()
    {
        var filter = new LevelFilter(LogLevel.DEBUG);

        Assert.That(
            filter.ShouldLog(CreateMessage(LogLevel.NONE)),
            Is.False);
    }

    [Test]
    public void ShouldLog_WithNoneMessageAndInfoThreshold_ShouldReturnFalse()
    {
        var filter = new LevelFilter(LogLevel.INFO);

        Assert.That(
            filter.ShouldLog(CreateMessage(LogLevel.NONE)),
            Is.False);
    }

    [Test]
    public void ShouldLog_WithNoneMessageAndNoneThreshold_ShouldReturnTrue()
    {
        var filter = new LevelFilter(LogLevel.NONE);

        Assert.That(
            filter.ShouldLog(CreateMessage(LogLevel.NONE)),
            Is.True);
    }

    [Test]
    public void SetLogLevel_ShouldUpdateThreshold()
    {
        var filter = new LevelFilter(LogLevel.INFO);

        filter.SetLogLevel(LogLevel.ERROR);

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void GetLogLevel_ShouldReturnCurrentThreshold()
    {
        var filter = new LevelFilter(LogLevel.WARNING);

        Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.WARNING));
    }

    [Test]
    public void SetLogLevel_ShouldAffectSubsequentFiltering()
    {
        var filter = new LevelFilter(LogLevel.INFO);

        var warningMessage = CreateMessage(LogLevel.WARNING);
        Assert.That(filter.ShouldLog(warningMessage), Is.True);

        filter.SetLogLevel(LogLevel.ERROR);

        Assert.Multiple(() =>
        {
            Assert.That(filter.ShouldLog(warningMessage), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.True);
        });
    }

    [Test]
    public void SetLogLevel_CalledMultipleTimes_ShouldUseLatestThreshold()
    {
        var filter = new LevelFilter(LogLevel.DEBUG);

        filter.SetLogLevel(LogLevel.WARNING);
        filter.SetLogLevel(LogLevel.FATAL);

        Assert.Multiple(() =>
        {
            Assert.That(filter.GetLogLevel(), Is.EqualTo(LogLevel.FATAL));
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.ERROR)), Is.False);
            Assert.That(filter.ShouldLog(CreateMessage(LogLevel.FATAL)), Is.True);
        });
    }

    [Test]
    public void ShouldLog_ForEveryThreshold_ShouldFollowExpectedComparison()
    {
        var levels = new[]
        {
            LogLevel.NONE,
            LogLevel.DEBUG,
            LogLevel.INFO,
            LogLevel.WARNING,
            LogLevel.ERROR,
            LogLevel.FATAL
        };

        foreach (var threshold in levels)
        {
            var filter = new LevelFilter(threshold);

            foreach (var messageLevel in levels)
            {
                var expected = (int)messageLevel >= (int)threshold;

                Assert.That(
                    filter.ShouldLog(CreateMessage(messageLevel)),
                    Is.EqualTo(expected),
                    $"Unexpected result for threshold={threshold}, messageLevel={messageLevel}.");
            }
        }
    }

    private static LogMessage CreateMessage(LogLevel level)
    {
        return new LogMessage.Builder()
            .WithLevel(level)
            .WithMessage("test message")
            .Build();
    }
}