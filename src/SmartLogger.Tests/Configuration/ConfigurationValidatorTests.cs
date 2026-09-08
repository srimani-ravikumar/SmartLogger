using SmartLogger.Core;
using SmartLogger.Configurations;

namespace SmartLogger.Tests.Configurations;

[TestFixture]
public class ConfigurationValidatorTests
{
    [Test]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithEmptyAppenders_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder();

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ConfigurationValidator.Validate(null));
    }

    [TestCase(LogOutputDestination.Console)]
    [TestCase(LogOutputDestination.FileSystem)]
    public void Validate_WithDuplicateDestination_ShouldThrowInvalidOperationException(
        LogOutputDestination destination)
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                CreateAppender(destination),
                CreateAppender(destination)
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithDifferentDestinations_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                CreateAppender(LogOutputDestination.Console),
                CreateAppender(LogOutputDestination.FileSystem)
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithUnknownDestination_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Unknown
                    }
                }
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithMultipleUnknownDestinations_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                CreateAppender(LogOutputDestination.Unknown),
                CreateAppender(LogOutputDestination.Unknown)
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithFileSystemAppenderWithoutFile_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.FileSystem,
                        File = null
                    }
                }
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithFileSystemAppenderWithoutFileName_ShouldThrowInvalidOperationException(
        string? fileName)
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.FileSystem,
                        File = new FileConfiguration
                        {
                            FileName = fileName
                        }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithValidFileSystemConfiguration_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                CreateAppender(LogOutputDestination.FileSystem)
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithCustomLayoutWithoutPattern_ShouldThrowInvalidOperationException(
        string? pattern)
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    },
                    Formatter = new FormatterConfiguration
                    {
                        LayoutType = LogMessageLayoutType.Custom,
                        Pattern = pattern
                    }
                }
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithCustomLayoutWithPattern_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    },
                    Formatter = new FormatterConfiguration
                    {
                        LayoutType = LogMessageLayoutType.Custom,
                        Pattern = "[%LEVEL] %MESSAGE"
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [TestCase(LogMessageLayoutType.Simple)]
    [TestCase(LogMessageLayoutType.Detailed)]
    public void Validate_WithNonCustomLayoutWithoutPattern_ShouldNotThrow(
        LogMessageLayoutType layoutType)
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            Appenders =
            [
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    },
                    Formatter = new FormatterConfiguration
                    {
                        LayoutType = layoutType,
                        Pattern = string.Empty
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    private static AppenderConfiguration CreateAppender(
        LogOutputDestination destination)
    {
        return new AppenderConfiguration
        {
            Destination = new DestinationConfiguration
            {
                Type = destination,
                File = destination == LogOutputDestination.FileSystem
                    ? new FileConfiguration
                    {
                        FileName = "Application"
                    }
                    : null
            }
        };
    }
}