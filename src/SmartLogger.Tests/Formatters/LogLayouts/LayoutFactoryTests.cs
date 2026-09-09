using SmartLogger.Core;
using SmartLogger.Formatters.LogLayouts;

namespace SmartLogger.Tests.Formatters.LogLayouts;

[TestFixture]
public class LayoutFactoryTests
{
    #region Layout Selection

    [Test]
    public void Create_WithSimpleLayout_ShouldReturnPatternLayout()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        // Act
        var result = LayoutFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<PatternLayout>());
    }

    [Test]
    public void Create_WithDetailedLayout_ShouldReturnPatternLayout()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        // Act
        var result = LayoutFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<PatternLayout>());
    }

    [Test]
    public void Create_WithCustomLayout_ShouldReturnPatternLayout()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            "[%LEVEL] %SOURCE: %MESSAGE");

        // Act
        var result = LayoutFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<PatternLayout>());
    }

    #endregion

    #region Simple Layout

    [Test]
    public void Create_WithSimpleLayout_ShouldUseExpectedPattern()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(
            result,
            Does.Contain(message.LogLevel.ToString()));

        Assert.That(
            result,
            Does.Contain(message.Source));

        Assert.That(
            result,
            Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithSimpleLayout_ShouldRenderConfiguredTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.Timestamp.ToString("yyyy")));
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithSimpleLayout_ShouldNotRenderThreadOrCorrelationTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        var message = CreateMessage(
            correlationId: "simple-correlation");

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Not.Contain("T#"));
        Assert.That(result, Does.Not.Contain(message.CorrelationId));
    }

    #endregion

    #region Detailed Layout

    [Test]
    public void Create_WithDetailedLayout_ShouldUseExpectedPattern()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain($"T#{message.ThreadId}"));
        Assert.That(result, Does.Contain(message.CorrelationId));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithDetailedLayout_ShouldRenderAllConfiguredTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain($"T#{message.ThreadId}"));
        Assert.That(result, Does.Contain(message.CorrelationId));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithDetailedLayout_ShouldRenderThreadToken()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain($"T#{message.ThreadId}"));
    }

    [Test]
    public void Create_WithDetailedLayout_ShouldRenderCorrelationToken()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        var message = CreateMessage(
            correlationId: "correlation-456");

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.CorrelationId));
    }

    #endregion

    #region Custom Layout

    [Test]
    public void Create_WithCustomLayout_ShouldUseConfiguredPattern()
    {
        // Arrange
        const string pattern = "[%LEVEL] %SOURCE: %MESSAGE";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(
            $"[{message.LogLevel}] {message.Source}: {message.Message}"));
    }

    [Test]
    public void Create_WithCustomLayout_ShouldRenderConfiguredTokens()
    {
        // Arrange
        const string pattern =
            "%TIMESTAMP | %LEVEL | %THREAD | %CORRELATION | %SOURCE | %MESSAGE";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain(message.ThreadId.ToString()));
        Assert.That(result, Does.Contain(message.CorrelationId));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithCustomLayout_WithSingleToken_ShouldRenderToken()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            "%MESSAGE");

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(message.Message));
    }

    [Test]
    public void Create_WithCustomLayout_WithRepeatedToken_ShouldRenderEachOccurrence()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            "%LEVEL - %LEVEL - %MESSAGE");

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        var expectedLevel = message.LogLevel.ToString();

        Assert.That(
            result,
            Is.EqualTo($"{expectedLevel} - {expectedLevel} - {message.Message}"));
    }

    [Test]
    public void Create_WithCustomLayout_WithLiteralText_ShouldPreserveLiteralText()
    {
        // Arrange
        const string pattern =
            "START :: [%LEVEL] :: %MESSAGE :: END";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(
            result,
            Does.StartWith("START ::"));

        Assert.That(
            result,
            Does.EndWith(":: END"));
    }

    [Test]
    public void Create_WithCustomLayout_WithEmptyPattern_ShouldReturnEmptyOutput()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            string.Empty);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    #endregion

    #region Token Registration

    [Test]
    public void Create_ShouldRegisterAllSupportedTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            "%TIMESTAMP|%LEVEL|%MESSAGE|%SOURCE|%THREAD|%CORRELATION");

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain(message.Message));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.ThreadId.ToString()));
        Assert.That(result, Does.Contain(message.CorrelationId));
        Assert.That(result, Does.Contain(message.Timestamp.ToString("yyyy")));
    }

    [Test]
    public void Create_WithSimpleLayout_ShouldResolveRegisteredTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithDetailedLayout_ShouldResolveRegisteredTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Detailed);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain(message.LogLevel.ToString()));
        Assert.That(result, Does.Contain(message.ThreadId.ToString()));
        Assert.That(result, Does.Contain(message.CorrelationId));
        Assert.That(result, Does.Contain(message.Source));
        Assert.That(result, Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithCustomLayout_ShouldResolveRegisteredTokens()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            "%MESSAGE");

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Is.EqualTo(message.Message));
    }

    #endregion

    #region Layout Instance Tests

    [Test]
    public void Create_CalledMultipleTimes_ShouldReturnDifferentLayoutInstances()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogMessageLayoutType.Simple);

        // Act
        var first = LayoutFactory.Create(configuration);
        var second = LayoutFactory.Create(configuration);

        // Assert
        Assert.That(first, Is.Not.SameAs(second));
    }

    [Test]
    public void Create_WithDifferentLayoutTypes_ShouldCreateIndependentLayouts()
    {
        // Arrange
        var simpleConfiguration =
            CreateConfiguration(LogMessageLayoutType.Simple);

        var detailedConfiguration =
            CreateConfiguration(LogMessageLayoutType.Detailed);

        var message = CreateMessage();

        // Act
        var simpleLayout =
            LayoutFactory.Create(simpleConfiguration);

        var detailedLayout =
            LayoutFactory.Create(detailedConfiguration);

        var simpleResult = simpleLayout.Render(message);
        var detailedResult = detailedLayout.Render(message);

        // Assert
        Assert.That(simpleResult, Does.Not.Contain($"T#{message.ThreadId}"));
        Assert.That(detailedResult, Does.Contain($"T#{message.ThreadId}"));
    }

    #endregion

    #region Unsupported Layout

    [Test]
    public void Create_WithUnsupportedLayoutType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (LogMessageLayoutType)999);

        // Act & Assert
        Assert.Throws<NotSupportedException>(
            () => LayoutFactory.Create(configuration));
    }

    [Test]
    public void Create_WithUnsupportedLayoutType_ShouldIncludeLayoutTypeInExceptionMessage()
    {
        // Arrange
        var unsupportedLayout =
            (LogMessageLayoutType)999;

        var configuration = CreateConfiguration(
            unsupportedLayout);

        // Act
        var exception = Assert.Throws<NotSupportedException>(
            () => LayoutFactory.Create(configuration));

        // Assert
        Assert.That(
            exception!.Message,
            Does.Contain(unsupportedLayout.ToString()));
    }

    #endregion

    #region Null Configuration

    [Test]
    public void Create_WithNullAppenderConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => LayoutFactory.Create(null!));
    }

    [Test]
    public void Create_WithNullFormatterConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var configuration = new AppenderConfiguration
        {
            Formatter = null!
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => LayoutFactory.Create(configuration));
    }

    #endregion

    #region Custom Pattern Boundaries

    [Test]
    public void Create_WithCustomLayout_WithWhitespacePattern_ShouldPreserveWhitespace()
    {
        // Arrange
        const string pattern = "  %LEVEL   -   %MESSAGE  ";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(
            result,
            Is.EqualTo($"  {message.LogLevel}   -   {message.Message}  "));
    }

    [Test]
    public void Create_WithCustomLayout_WithSpecialCharacters_ShouldPreserveLiteralCharacters()
    {
        // Arrange
        const string pattern =
            "<log>[%LEVEL] & %MESSAGE \"test\"</log>";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(
            result,
            Does.Contain("<log>"));

        Assert.That(
            result,
            Does.Contain("&"));

        Assert.That(
            result,
            Does.Contain("\"test\""));

        Assert.That(
            result,
            Does.Contain(message.Message));
    }

    [Test]
    public void Create_WithCustomLayout_WithUnicodePattern_ShouldPreserveUnicodeCharacters()
    {
        // Arrange
        const string pattern =
            "ログ [%LEVEL] - संदेश - %MESSAGE - 🚀";

        var configuration = CreateConfiguration(
            LogMessageLayoutType.Custom,
            pattern);

        var message = CreateMessage();

        // Act
        var layout = LayoutFactory.Create(configuration);
        var result = layout.Render(message);

        // Assert
        Assert.That(result, Does.Contain("ログ"));
        Assert.That(result, Does.Contain("संदेश"));
        Assert.That(result, Does.Contain("🚀"));
        Assert.That(result, Does.Contain(message.Message));
    }

    #endregion

    #region Test Helpers

    private static AppenderConfiguration CreateConfiguration(
        LogMessageLayoutType layoutType,
        string pattern = "")
    {
        return new AppenderConfiguration
        {
            Formatter = new FormatterConfiguration
            {
                LayoutType = layoutType,
                Pattern = pattern
            }
        };
    }

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