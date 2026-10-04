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

    #region File Directory Validation

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithFileSystemAppenderWithoutDirectory_ShouldThrowInvalidOperationException(
        string? directory)
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
                            Directory = directory,
                            FileName = "Application"
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("directory"));
    }

    #endregion

    #region File Extension Validation

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithFileSystemAppenderWithoutExtension_ShouldThrowInvalidOperationException(
        string? extension)
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = extension
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("extension"));
    }

    #endregion

    #region File Naming Configuration Validation

    [Test]
    public void Validate_WithDateNamingWithoutDateFormat_ShouldThrowInvalidOperationException()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Naming = new FileNamingConfiguration
                            {
                                Strategy = FileNamingStrategyType.Date,
                                DateFormat = ""
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("DateFormat"));
    }

    [Test]
    public void Validate_WithTimestampNamingWithoutDateFormat_ShouldThrowInvalidOperationException()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Naming = new FileNamingConfiguration
                            {
                                Strategy = FileNamingStrategyType.Timestamp,
                                DateFormat = null
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("DateFormat"));
    }

    [Test]
    public void Validate_WithValidDateNamingConfiguration_ShouldNotThrow()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Naming = new FileNamingConfiguration
                            {
                                Strategy = FileNamingStrategyType.Date,
                                DateFormat = "yyyy-MM-dd"
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region File Rolling Configuration Validation

    [Test]
    public void Validate_WithSizeBasedRollingWithZeroMaxSize_ShouldThrowInvalidOperationException()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Rolling = new FileRollingConfiguration
                            {
                                Strategy = RollingStrategyType.Size,
                                MaxFileSizeMB = 0
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("MaxFileSizeMB"));
    }

    [Test]
    public void Validate_WithSizeBasedRollingWithNegativeMaxSize_ShouldThrowInvalidOperationException()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Rolling = new FileRollingConfiguration
                            {
                                Strategy = RollingStrategyType.Size,
                                MaxFileSizeMB = -10
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("MaxFileSizeMB"));
    }

    [Test]
    public void Validate_WithValidSizeBasedRolling_ShouldNotThrow()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Rolling = new FileRollingConfiguration
                            {
                                Strategy = RollingStrategyType.Size,
                                MaxFileSizeMB = 50
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region Archive Configuration Validation

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithEnabledArchiveWithoutDirectory_ShouldThrowInvalidOperationException(
        string? archiveDir)
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Archive = new ArchiveConfiguration
                            {
                                Enabled = true,
                                Directory = archiveDir
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("Archive directory"));
    }

    [Test]
    public void Validate_WithValidArchiveConfiguration_ShouldNotThrow()
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Archive = new ArchiveConfiguration
                            {
                                Enabled = true,
                                Directory = "Archive"
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region Retention Configuration Validation

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void Validate_WithRetentionDaysZeroOrNegative_ShouldThrowInvalidOperationException(
        int retentionDays)
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Retention = new RetentionConfiguration
                            {
                                RetentionDays = retentionDays
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("Retention days"));
    }

    [TestCase(1)]
    [TestCase(30)]
    [TestCase(365)]
    public void Validate_WithValidRetentionDays_ShouldNotThrow(int retentionDays)
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
                            Directory = "Logs",
                            FileName = "Application",
                            Extension = "log",
                            Retention = new RetentionConfiguration
                            {
                                RetentionDays = retentionDays
                            }
                        }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region LogAggregator Validation

    [Test]
    public void Validate_WithLogAggregatorWithoutConfiguration_ShouldThrowInvalidOperationException()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = null
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("LogAggregator destination requires configuration"));
    }

    [Test]
    public void Validate_WithLogAggregatorNonJsonFormat_ShouldThrowInvalidOperationException()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration()
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.PlainText
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("LogAggregator requires JSON"));
    }

    [Test]
    public void Validate_WithLogAggregatorDefaultSinkWithoutEndpoint_ShouldThrowInvalidOperationException()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = true,
                            Endpoint = null
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("Endpoint"));
    }

    [Test]
    public void Validate_WithLogAggregatorDefaultSinkWithValidEndpoint_ShouldNotThrow()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = true,
                            Endpoint = new Uri("http://localhost:8080/logs")
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithLogAggregatorCustomSinkWithoutKeyOrTypeName_ShouldThrowInvalidOperationException()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = false,
                            SinkKey = null,
                            SinkTypeName = null
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("SinkKey").Or.Contain("SinkTypeName"));
    }

    [Test]
    public void Validate_WithLogAggregatorCustomSinkWithKey_ShouldNotThrow()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = false,
                            SinkKey = "MyCustomSink"
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithLogAggregatorCustomSinkWithTypeName_ShouldNotThrow()
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = false,
                            SinkTypeName = "MyApp.CustomSink, MyApp"
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region Logger Override Validation

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithEmptyLoggerOverrideName_ShouldThrowInvalidOperationException(
        string? loggerName)
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            LoggerOverrides =
            [
                new LoggerOverrideConfiguration
                {
                    LoggerName = loggerName,
                    LogLevel = LogLevel.DEBUG
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("Logger name"));
    }

    [Test]
    public void Validate_WithValidLoggerOverride_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            LoggerOverrides =
            [
                new LoggerOverrideConfiguration
                {
                    LoggerName = "MyApp.Services",
                    LogLevel = LogLevel.DEBUG
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region JSON Formatter Validation

    [Test]
    public void Validate_WithJsonFormatWithoutIncludedFields_ShouldThrowInvalidOperationException()
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
                        OutputFormat = LogOutputFormat.Json,
                        IncludedJsonFields = new List<string>()
                    }
                }
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.Validate(configuration));
        Assert.That(ex?.Message, Does.Contain("at least one"));
    }

    [Test]
    public void Validate_WithJsonFormatWithIncludedFields_ShouldNotThrow()
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
                        OutputFormat = LogOutputFormat.Json,
                        IncludedJsonFields = new List<string> { "timestamp", "level", "message" }
                    }
                }
            ]
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

    #region Async Logging Configuration Validation

    [Test]
    public void Validate_WithAsyncLoggingEnabledButNoAppenders_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            EnableAsyncLoggingProcess = true,
            Appenders = new List<AppenderConfiguration>()
        };

        // Act & Assert
        // Note: When no appenders are configured, the default console appender is automatically enabled
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    [Test]
    public void Validate_WithAsyncLoggingEnabledWithAppenders_ShouldNotThrow()
    {
        // Arrange
        var configuration = new LogConfigurationHolder
        {
            EnableAsyncLoggingProcess = true,
            Appenders = new List<AppenderConfiguration>
            {
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.Console
                    }
                }
            }
        };

        // Act & Assert
        Assert.DoesNotThrow(() =>
            ConfigurationValidator.Validate(configuration));
    }

    #endregion

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