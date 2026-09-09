using Moq;
using SmartLogger.Core;
using SmartLogger.Formatters.Tokens;

namespace SmartLogger.Tests.Formatters.Tokens;

[TestFixture]
public class TokenRegistryTests
{
    [Test]
    public void Constructor_WithSingleToken_ShouldRegisterToken()
    {
        // Arrange
        var renderer = CreateRenderer("%LEVEL");

        // Act
        var sut = new TokenRegistry(new[] { renderer });

        // Assert
        Assert.That(sut.Tokens, Has.Count.EqualTo(1));
        Assert.That(sut.Tokens.ContainsKey("%LEVEL"), Is.True);
        Assert.That(sut.Tokens["%LEVEL"], Is.SameAs(renderer));
    }

    [Test]
    public void Constructor_WithMultipleTokens_ShouldRegisterAllTokens()
    {
        // Arrange
        var levelRenderer = CreateRenderer("%LEVEL");
        var messageRenderer = CreateRenderer("%MESSAGE");
        var sourceRenderer = CreateRenderer("%SOURCE");

        // Act
        var sut = new TokenRegistry(
            new[]
            {
                levelRenderer,
                messageRenderer,
                sourceRenderer
            });

        // Assert
        Assert.That(sut.Tokens, Has.Count.EqualTo(3));
        Assert.That(sut.Tokens.ContainsKey("%LEVEL"), Is.True);
        Assert.That(sut.Tokens.ContainsKey("%MESSAGE"), Is.True);
        Assert.That(sut.Tokens.ContainsKey("%SOURCE"), Is.True);
    }

    [Test]
    public void Tokens_WithRegisteredToken_ShouldReturnSameRendererInstance()
    {
        // Arrange
        var renderer = CreateRenderer("%LEVEL");
        var sut = new TokenRegistry(new[] { renderer });

        // Act
        var result = sut.Tokens["%LEVEL"];

        // Assert
        Assert.That(result, Is.SameAs(renderer));
    }

    [Test]
    public void Tokens_WithMultipleTokens_ShouldMapEachTokenToCorrectRenderer()
    {
        // Arrange
        var levelRenderer = CreateRenderer("%LEVEL");
        var messageRenderer = CreateRenderer("%MESSAGE");

        var sut = new TokenRegistry(
            new[]
            {
                levelRenderer,
                messageRenderer
            });

        // Act
        var levelResult = sut.Tokens["%LEVEL"];
        var messageResult = sut.Tokens["%MESSAGE"];

        // Assert
        Assert.That(levelResult, Is.SameAs(levelRenderer));
        Assert.That(messageResult, Is.SameAs(messageRenderer));
    }

    [Test]
    public void Tokens_WithMultipleTokens_ShouldContainExpectedCount()
    {
        // Arrange
        var tokens = new[]
        {
            CreateRenderer("%LEVEL"),
            CreateRenderer("%MESSAGE"),
            CreateRenderer("%SOURCE")
        };

        // Act
        var sut = new TokenRegistry(tokens);

        // Assert
        Assert.That(sut.Tokens, Has.Count.EqualTo(3));
    }

    [Test]
    public void Tokens_WithRegisteredToken_ShouldSupportDictionaryLookup()
    {
        // Arrange
        var renderer = CreateRenderer("%LEVEL");
        var sut = new TokenRegistry(new[] { renderer });

        // Act
        var found = sut.Tokens.TryGetValue("%LEVEL", out var result);

        // Assert
        Assert.That(found, Is.True);
        Assert.That(result, Is.SameAs(renderer));
    }

    [Test]
    public void Constructor_WithEmptyTokenCollection_ShouldCreateEmptyRegistry()
    {
        // Arrange
        var tokens = Array.Empty<ITokenRendererStrategy>();

        // Act
        var sut = new TokenRegistry(tokens);

        // Assert
        Assert.That(sut.Tokens, Is.Empty);
    }

    [Test]
    public void Constructor_WithDuplicateTokens_ShouldThrowArgumentException()
    {
        // Arrange
        var firstRenderer = CreateRenderer("%LEVEL");
        var secondRenderer = CreateRenderer("%LEVEL");

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new TokenRegistry(
                new[]
                {
                    firstRenderer,
                    secondRenderer
                }));
    }

    [Test]
    public void Constructor_WithDifferentRenderersUsingSameToken_ShouldThrowArgumentException()
    {
        // Arrange
        var firstRenderer = CreateRenderer("%CUSTOM");
        var secondRenderer = CreateRenderer("%CUSTOM");

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new TokenRegistry(
                new[]
                {
                    firstRenderer,
                    secondRenderer
                }));
    }

    [Test]
    public void Constructor_WithNullTokens_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TokenRegistry(null!));
    }

    [Test]
    public void Constructor_WithValidTokenStrategy_ShouldCreateRegistry()
    {
        // Arrange
        var renderer = CreateRenderer("%CUSTOM");

        // Act
        var sut = new TokenRegistry(new[] { renderer });

        // Assert
        Assert.That(sut.Tokens, Is.Not.Null);
        Assert.That(sut.Tokens, Has.Count.EqualTo(1));
    }

    [Test]
    public void Tokens_ShouldPreserveTokenIdentifierExactly()
    {
        // Arrange
        const string token = "  %CUSTOM  ";
        var renderer = CreateRenderer(token);

        // Act
        var sut = new TokenRegistry(new[] { renderer });

        // Assert
        Assert.That(sut.Tokens.ContainsKey(token), Is.True);
        Assert.That(sut.Tokens.ContainsKey("%CUSTOM"), Is.False);
    }

    [Test]
    public void Constructor_WithDifferentCaseTokenIdentifiers_ShouldRegisterAccordingToDictionarySemantics()
    {
        // Arrange
        var upperCaseRenderer = CreateRenderer("%LEVEL");
        var lowerCaseRenderer = CreateRenderer("%level");

        // Act
        var sut = new TokenRegistry(
            new[]
            {
                upperCaseRenderer,
                lowerCaseRenderer
            });

        // Assert
        Assert.That(sut.Tokens, Has.Count.EqualTo(2));
        Assert.That(sut.Tokens["%LEVEL"], Is.SameAs(upperCaseRenderer));
        Assert.That(sut.Tokens["%level"], Is.SameAs(lowerCaseRenderer));
    }

    private static ITokenRendererStrategy CreateRenderer(string token)
    {
        var mock = new Mock<ITokenRendererStrategy>();

        mock.SetupGet(x => x.Token)
            .Returns(token);

        return mock.Object;
    }
}