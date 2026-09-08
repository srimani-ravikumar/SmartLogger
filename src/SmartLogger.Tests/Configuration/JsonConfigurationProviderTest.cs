using SmartLogger.Configurations;
using SmartLogger.Core;
using System.Text.Json;

namespace SmartLogger.Tests.Configurations;

[TestFixture]
public class JsonConfigurationProviderTests
{
    private string _testDirectory;
    private string _configFilePath;

    [SetUp]
    public void Setup()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);

        _configFilePath = Path.Combine(
            _testDirectory,
            "logger.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public void Load_WithValidJson_ShouldReturnConfiguration()
    {
        // Arrange
        var json = """
                   {
                       "RootLogLevel": "DEBUG",
                       "EnableAsyncLoggingProcess": true
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.IsNotNull(configuration);
        Assert.That(configuration.RootLogLevel, Is.EqualTo(LogLevel.DEBUG));
        Assert.That(
            configuration.EnableAsyncLoggingProcess,
            Is.True);
    }

    [Test]
    public void Load_WithCaseInsensitiveProperties_ShouldDeserializeConfiguration()
    {
        // Arrange
        var json = """
                   {
                       "rootloglevel": "WARNING",
                       "enableasyncloggingprocess": true
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.That(
            configuration.RootLogLevel,
            Is.EqualTo(LogLevel.WARNING));

        Assert.That(
            configuration.EnableAsyncLoggingProcess,
            Is.True);
    }

    [Test]
    public void Load_WithJsonComments_ShouldDeserializeConfiguration()
    {
        // Arrange
        var json = """
                   {
                       // Root logging level
                       "RootLogLevel": "ERROR",

                       /*
                        * Enable asynchronous logging.
                        */
                       "EnableAsyncLoggingProcess": true
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.That(
            configuration.RootLogLevel,
            Is.EqualTo(LogLevel.ERROR));

        Assert.That(
            configuration.EnableAsyncLoggingProcess,
            Is.True);
    }

    [Test]
    public void Load_WithTrailingCommas_ShouldDeserializeConfiguration()
    {
        // Arrange
        var json = """
                   {
                       "RootLogLevel": "INFO",
                       "EnableAsyncLoggingProcess": true,
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.That(
            configuration.RootLogLevel,
            Is.EqualTo(LogLevel.INFO));

        Assert.That(
            configuration.EnableAsyncLoggingProcess,
            Is.True);
    }

    [TestCase("DEBUG", LogLevel.DEBUG)]
    [TestCase("INFO", LogLevel.INFO)]
    [TestCase("WARNING", LogLevel.WARNING)]
    [TestCase("ERROR", LogLevel.ERROR)]
    [TestCase("FATAL", LogLevel.FATAL)]
    public void Load_WithStringEnumValues_ShouldDeserializeConfiguration(
        string logLevel,
        LogLevel expected)
    {
        // Arrange
        var json = $$"""
                   {
                       "RootLogLevel": "{{logLevel}}"
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.That(configuration.RootLogLevel, Is.EqualTo(expected));
    }

    [Test]
    public void Load_WithIntegerEnumValue_ShouldThrowJsonException()
    {
        // Arrange
        var json = """
                   {
                       "RootLogLevel": 2
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            provider.Load());
    }

    [Test]
    public void Load_WithMalformedJson_ShouldThrowJsonException()
    {
        // Arrange
        var json = """
                   {
                       "RootLogLevel": "INFO"
                       "EnableAsyncLoggingProcess": true
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            provider.Load());
    }

    [Test]
    public void Load_WithInvalidJsonStructure_ShouldThrowJsonException()
    {
        // Arrange
        var json = """
                   {
                       "RootLogLevel": {
                           "Value": "INFO"
                       }
                   }
                   """;

        WriteConfiguration(json);

        var provider = new JsonConfigurationProvider(
            _configFilePath);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            provider.Load());
    }

    private void WriteConfiguration(string json)
    {
        File.WriteAllText(_configFilePath, json);
    }
}