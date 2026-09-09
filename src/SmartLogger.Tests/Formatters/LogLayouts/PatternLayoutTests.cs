using Moq;
using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.LogLayouts;

[TestFixture]
public class PatternLayoutTests
{
    [Test]
    public void Constructor_WithValidPattern_CreatesInstanceSuccessfully()
    {
        var registry = CreateRegistry();

        var layout = new PatternLayout("[%LEVEL] %MESSAGE", registry);

        Assert.That(layout, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullPattern_CreatesInstanceSuccessfully()
    {
        var registry = CreateRegistry();

        var layout = new PatternLayout(null!, registry);

        Assert.That(layout, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullTokenRegistry_CreatesInstanceSuccessfully()
    {
        var layout = new PatternLayout("[%LEVEL]", null!);

        Assert.That(layout, Is.Not.Null);
    }

    [Test]
    public void Render_WithRegisteredToken_ReplacesTokenWithRenderedValue()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");
        var layout = CreateLayout("[%LEVEL]", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("INFO"));
    }

    [Test]
    public void Render_WithLiteralText_PreservesLiteralText()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");
        var layout = CreateLayout("PREFIX [%LEVEL] SUFFIX", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("PREFIX [INFO] SUFFIX"));
    }

    [Test]
    public void Render_WithMultipleTokens_ReplacesAllRegisteredTokens()
    {
        var message = CreateMessage();
        var levelToken = CreateToken("%LEVEL", "INFO");
        var sourceToken = CreateToken("%SOURCE", "OrderService");

        var layout = CreateLayout(
            "[%LEVEL] %SOURCE - %MESSAGE",
            levelToken,
            sourceToken);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("[INFO] OrderService - Application started"));
    }

    [Test]
    public void Render_WithUnknownToken_PreservesUnknownToken()
    {
        var message = CreateMessage();
        var levelToken = CreateToken("%LEVEL", "INFO");

        var layout = CreateLayout("[%LEVEL] %UNKNOWN", levelToken);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("[INFO] %UNKNOWN"));
    }

    [Test]
    public void Render_WithRepeatedToken_ReplacesEveryOccurrence()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");

        var layout = CreateLayout("%LEVEL | %LEVEL | %LEVEL", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("INFO | INFO | INFO"));
    }

    [Test]
    public void Render_WithRegisteredToken_InvokesTokenRenderer()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");

        var layout = CreateLayout("[%LEVEL]", token);

        _ = layout.Render(message);

        token.Verify(x => x.Render(message), Times.Once);
    }

    [Test]
    public void Render_WithRepeatedToken_UsesRendererOnceForReplacement()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");

        var layout = CreateLayout("%LEVEL | %LEVEL | %LEVEL", token);

        _ = layout.Render(message);

        // PatternLayout calculates the replacement value once per token
        // and then string.Replace performs the global substitution.
        token.Verify(x => x.Render(message), Times.Once);
    }

    [Test]
    public void Render_PassesOriginalLogMessageToTokenRenderer()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");

        var layout = CreateLayout("[%LEVEL]", token);

        _ = layout.Render(message);

        token.Verify(x => x.Render(It.Is<LogMessage>(m => ReferenceEquals(m, message))), Times.Once);
    }

    [Test]
    public void Render_WithEmptyPattern_ReturnsRawMessage()
    {
        var message = CreateMessage();
        var layout = new PatternLayout(string.Empty, CreateRegistry());

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo(message.Message));
    }

    [Test]
    public void Render_WithWhitespacePattern_ReturnsRawMessage()
    {
        var message = CreateMessage();
        var layout = new PatternLayout("   \t\r\n", CreateRegistry());

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo(message.Message));
    }

    [Test]
    public void Render_WithNullPattern_ReturnsRawMessage()
    {
        var message = CreateMessage();
        var layout = new PatternLayout(null!, CreateRegistry());

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo(message.Message));
    }

    [Test]
    public void Render_WithTokenOnlyPattern_ReturnsRenderedToken()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");
        var layout = CreateLayout("%LEVEL", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("INFO"));
    }

    [Test]
    public void Render_WithLiteralOnlyPattern_ReturnsPatternUnchanged()
    {
        var message = CreateMessage();
        var layout = new PatternLayout("This is literal text.", CreateRegistry());

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("This is literal text."));
    }

    [Test]
    public void Render_WithWhitespaceAroundTokens_PreservesWhitespace()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");
        var layout = CreateLayout("  [%LEVEL]   ", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("  [INFO]   "));
    }

    [Test]
    public void Render_WithSpecialCharactersAndUnicode_PreservesContent()
    {
        var message = CreateMessage("Hello — 世界 🌍 <test> & \"quoted\"");
        var token = CreateToken("%MESSAGE", message.Message);
        var layout = CreateLayout("Message: %MESSAGE", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo($"Message: {message.Message}"));
    }

    [Test]
    public void Render_WhenTokenRendererReturnsEmptyString_RemovesToken()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", string.Empty);
        var layout = CreateLayout("before-%LEVEL-after", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("before--after"));
    }

    [Test]
    public void Render_WhenTokenRendererReturnsWhitespace_PreservesRenderedValue()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "   ");
        var layout = CreateLayout("before-%LEVEL-after", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("before-   -after"));
    }

    [Test]
    public void Render_WhenTokenRendererReturnsSpecialCharacters_PreservesRenderedValue()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "<INFO> & \"warning\"");
        var layout = CreateLayout("[%LEVEL]", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("[<INFO> & \"warning\"]"));
    }

    [Test]
    public void Render_WhenTokenRendererReturnsUnicode_PreservesRenderedValue()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO 世界 🚀");
        var layout = CreateLayout("[%LEVEL]", token);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("[INFO 世界 🚀]"));
    }

    [Test]
    public void Render_WithMultipleRegisteredTokens_ProcessesEachTokenIndependently()
    {
        var message = CreateMessage();
        var levelToken = CreateToken("%LEVEL", "INFO");
        var sourceToken = CreateToken("%SOURCE", "OrderService");

        var layout = CreateLayout("%LEVEL - %SOURCE", levelToken, sourceToken);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("INFO - OrderService"));
        levelToken.Verify(x => x.Render(message), Times.Once);
        sourceToken.Verify(x => x.Render(message), Times.Once);
    }

    [Test]
    public void Render_WithMultipleTokens_PreservesUnrelatedLiteralContent()
    {
        var message = CreateMessage();
        var levelToken = CreateToken("%LEVEL", "INFO");
        var sourceToken = CreateToken("%SOURCE", "OrderService");

        var layout = CreateLayout(
            "START::%LEVEL::MIDDLE::%SOURCE::END",
            levelToken,
            sourceToken);

        var result = layout.Render(message);

        Assert.That(result, Is.EqualTo("START::INFO::MIDDLE::OrderService::END"));
    }

    [Test]
    public void Render_WithNullMessageAndEmptyPattern_ThrowsNullReferenceException()
    {
        var layout = new PatternLayout(string.Empty, CreateRegistry());

        Assert.That(
            () => layout.Render(null!),
            Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void Render_WithNullTokenRegistry_ThrowsNullReferenceException()
    {
        var layout = new PatternLayout("[%LEVEL]", null!);

        Assert.That(
            () => layout.Render(CreateMessage()),
            Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void Render_WhenTokenRendererThrows_PropagatesException()
    {
        var message = CreateMessage();
        var expected = new InvalidOperationException("Token rendering failed.");

        var token = new Mock<ITokenRendererStrategy>();
        token.SetupGet(x => x.Token).Returns("%LEVEL");
        token.Setup(x => x.Render(message)).Throws(expected);

        var layout = CreateLayout("[%LEVEL]", token);

        var exception = Assert.Throws<InvalidOperationException>(() => layout.Render(message));

        Assert.That(exception, Is.SameAs(expected));
    }

    [Test]
    public void Render_CalledMultipleTimes_ProducesCorrectResults()
    {
        var firstMessage = CreateMessage("First message");
        var secondMessage = CreateMessage("Second message");
        var token = new Mock<ITokenRendererStrategy>();

        token.SetupGet(x => x.Token).Returns("%MESSAGE");
        token.Setup(x => x.Render(firstMessage)).Returns("First message");
        token.Setup(x => x.Render(secondMessage)).Returns("Second message");

        var layout = CreateLayout("Message=%MESSAGE", token);

        var firstResult = layout.Render(firstMessage);
        var secondResult = layout.Render(secondMessage);

        Assert.That(firstResult, Is.EqualTo("Message=First message"));
        Assert.That(secondResult, Is.EqualTo("Message=Second message"));
        token.Verify(x => x.Render(firstMessage), Times.Once);
        token.Verify(x => x.Render(secondMessage), Times.Once);
    }

    [Test]
    public void Render_CalledMultipleTimes_DoesNotMutatePattern()
    {
        var message = CreateMessage();
        var token = CreateToken("%LEVEL", "INFO");
        var layout = CreateLayout("[%LEVEL]", token);

        var firstResult = layout.Render(message);
        var secondResult = layout.Render(message);

        Assert.That(firstResult, Is.EqualTo("[INFO]"));
        Assert.That(secondResult, Is.EqualTo("[INFO]"));
    }

    [Test]
    public void Render_WithDifferentMessages_ProducesCorrespondingResults()
    {
        var firstMessage = CreateMessage("First");
        var secondMessage = CreateMessage("Second");
        var token = new Mock<ITokenRendererStrategy>();

        token.SetupGet(x => x.Token).Returns("%MESSAGE");
        token.Setup(x => x.Render(firstMessage)).Returns("First");
        token.Setup(x => x.Render(secondMessage)).Returns("Second");

        var layout = CreateLayout("%MESSAGE", token);

        Assert.That(layout.Render(firstMessage), Is.EqualTo("First"));
        Assert.That(layout.Render(secondMessage), Is.EqualTo("Second"));
    }

    private static PatternLayout CreateLayout(
        string pattern,
        params Mock<ITokenRendererStrategy>[] tokens)
    {
        return new PatternLayout(pattern, CreateRegistry(tokens));
    }

    private static TokenRegistry CreateRegistry(
        params Mock<ITokenRendererStrategy>[] tokens)
    {
        var strategies = tokens.Length == 0
            ? Array.Empty<ITokenRendererStrategy>()
            : tokens.Select(x => x.Object).ToArray();

        return new TokenRegistry(strategies);
    }

    private static Mock<ITokenRendererStrategy> CreateToken(
        string tokenName,
        string renderedValue)
    {
        var token = new Mock<ITokenRendererStrategy>();

        token.SetupGet(x => x.Token).Returns(tokenName);
        token.Setup(x => x.Render(It.IsAny<LogMessage>())).Returns(renderedValue);

        return token;
    }

    private static LogMessage CreateMessage(
        string messageText = "Application started",
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
}