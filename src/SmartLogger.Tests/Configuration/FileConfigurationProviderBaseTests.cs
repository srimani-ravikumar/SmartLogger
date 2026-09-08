using SmartLogger.Configurations;
using SmartLogger.Core;

namespace SmartLogger.Tests.Configurations;

[TestFixture]
public class FileConfigurationProviderBaseTests
{
    private string _testDirectory;
    private string _configFilePath;

    [SetUp]
    public void Setup()
    {
        LoggerManager.Reset();

        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);

        _configFilePath = Path.Combine(
            _testDirectory,
            "logger.config");

        File.WriteAllText(_configFilePath, "valid configuration");
    }

    [TearDown]
    public void TearDown()
    {
        LoggerManager.Reset();

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

#pragma warning disable NUnit1001 // The individual arguments provided by a TestCaseAttribute must match the type of the corresponding parameter of the method
    [TestCase(null)]
#pragma warning restore NUnit1001 // The individual arguments provided by a TestCaseAttribute must match the type of the corresponding parameter of the method
    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithInvalidFilePath_ShouldThrowArgumentNullException(string filePath)
    {
        // Arrange

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new TestFileConfigurationProvider(filePath, false));

        Assert.That(ex.ParamName, Is.EqualTo("filePath"));
        Assert.That(
            ex.Message,
            Does.Contain("Invalid configuration file path."));
    }

    [Test]
    public void Constructor_WithAbsolutePath_ShouldUseProvidedPath()
    {
        // Arrange
        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false);

        // Act
        var filePath = provider.GetFilePath();

        // Assert
        Assert.That(filePath, Is.EqualTo(_configFilePath));
    }

    [Test]
    public void Constructor_WithRelativePath_ShouldResolveToAbsolutePath()
    {
        // Arrange
        var relativePath = Path.Combine(
            "SmartLoggerTests",
            "logger.config");

        var expectedPath = Path.Combine(
            AppContext.BaseDirectory,
            relativePath);

        var provider = new TestFileConfigurationProvider(
            relativePath,
            false);

        // Act
        var filePath = provider.GetFilePath();

        // Assert
        Assert.That(filePath, Is.EqualTo(expectedPath));
        Assert.That(Path.IsPathRooted(filePath), Is.True);
    }

    [Test]
    public void Load_WithValidConfiguration_ShouldReturnConfiguration()
    {
        // Arrange
        var expectedConfiguration = new LogConfigurationHolder();

        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false)
        {
            Configuration = expectedConfiguration
        };

        // Act
        var configuration = provider.Load();

        // Assert
        Assert.That(configuration, Is.SameAs(expectedConfiguration));
        Assert.That(provider.DeserializeCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Load_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        File.Delete(_configFilePath);

        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false);

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() =>
            provider.Load());

        Assert.That(
            ex.Message,
            Does.Contain("SmartLogger configuration file not found"));
    }

    [Test]
    public void Load_WhenDeserializeReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false)
        {
            Configuration = null
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            provider.Load());

        Assert.That(
            ex.Message,
            Is.EqualTo("Failed to load SmartLogger configuration."));
    }

    [TestCase(TestInvalidConfiguration.UnknownDestination)]
    [TestCase(TestInvalidConfiguration.DuplicateDestination)]
    [TestCase(TestInvalidConfiguration.InvalidFileConfiguration)]
    [TestCase(TestInvalidConfiguration.InvalidCustomLayout)]
    public void Load_WithInvalidConfiguration_ShouldThrowInvalidOperationException(TestInvalidConfiguration invalidConfiguration)
    {
        // Arrange
        var configuration = CreateInvalidConfiguration(
            invalidConfiguration);

        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false)
        {
            Configuration = configuration
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            provider.Load());
    }

    [Test]
    public void Constructor_WithAutoReloadEnabled_ShouldWatchConfigurationFile()
    {
        // Arrange
        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            true)
        {
            Configuration = new LogConfigurationHolder()
        };

        LoggerManager.Initialize(provider);

        var initialLoadCount = provider.DeserializeCallCount;

        // Act
        File.AppendAllText(
            _configFilePath,
            Environment.NewLine + "configuration changed");

        // Assert
        var reloadDetected = SpinWait.SpinUntil(
            () => provider.DeserializeCallCount > initialLoadCount,
            TimeSpan.FromSeconds(5));

        Assert.That(reloadDetected, Is.True);
        Assert.That(
            provider.DeserializeCallCount,
            Is.GreaterThan(initialLoadCount));
    }

    [Test]
    public void Constructor_WithAutoReloadDisabled_ShouldNotTriggerReload()
    {
        // Arrange
        var provider = new TestFileConfigurationProvider(
            _configFilePath,
            false)
        {
            Configuration = new LogConfigurationHolder()
        };

        LoggerManager.Initialize(provider);

        var initialLoadCount = provider.DeserializeCallCount;

        // Act
        File.AppendAllText(
            _configFilePath,
            Environment.NewLine + "configuration changed");

        // Assert
        Thread.Sleep(500);

        Assert.That(
            provider.DeserializeCallCount,
            Is.EqualTo(initialLoadCount));
    }

    private static LogConfigurationHolder CreateInvalidConfiguration(TestInvalidConfiguration invalidConfiguration)
    {
        var configuration = new LogConfigurationHolder();

        switch (invalidConfiguration)
        {
            case TestInvalidConfiguration.UnknownDestination:
                configuration.Appenders.Add(
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.Unknown
                        }
                    });
                break;

            case TestInvalidConfiguration.DuplicateDestination:
                configuration.Appenders.Add(
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.Console
                        }
                    });

                configuration.Appenders.Add(
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.Console
                        }
                    });
                break;

            case TestInvalidConfiguration.InvalidFileConfiguration:
                configuration.Appenders.Add(
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                FileName = string.Empty
                            }
                        }
                    });
                break;

            case TestInvalidConfiguration.InvalidCustomLayout:
                configuration.Appenders.Add(
                    new AppenderConfiguration
                    {
                        Formatter = new FormatterConfiguration
                        {
                            LayoutType = LogMessageLayoutType.Custom,
                            Pattern = string.Empty
                        }
                    });
                break;
        }

        return configuration;
    }

    public enum TestInvalidConfiguration
    {
        UnknownDestination,
        DuplicateDestination,
        InvalidFileConfiguration,
        InvalidCustomLayout
    }

    private sealed class TestFileConfigurationProvider : FileConfigurationProviderBase
    {
        public TestFileConfigurationProvider(string filePath, bool enableAutoReload) : base(filePath, enableAutoReload)
        { }

        public LogConfigurationHolder? Configuration { get; set; }

        public int DeserializeCallCount { get; private set; }

        public string GetFilePath()
        {
            return FilePath;
        }

        protected override LogConfigurationHolder Deserialize(string filePath)
        {
            DeserializeCallCount++;

            return Configuration!;
        }
    }
}