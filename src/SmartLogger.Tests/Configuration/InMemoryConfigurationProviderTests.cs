using SmartLogger.Configurations;
using SmartLogger.Core;
using System;
using System.Collections.Generic;

namespace SmartLogger.Tests.Configuration
{
    [TestFixture]
    public class InMemoryConfigurationProviderTests
    {
        [Test]
        public void Constructor_WithValidConfiguration_ShouldCreateProvider()
        {
            // Arrange
            var configuration = CreateValidConfiguration();

            // Act
            var provider = new InMemoryConfigurationProvider(configuration);

            // Assert
            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            LogConfigurationHolder configuration = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new InMemoryConfigurationProvider(configuration));
            Assert.That(ex.ParamName, Is.EqualTo("configuration"));
        }

        [Test]
        public void Load_WithValidConfiguration_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded, Is.SameAs(configuration));
        }

        [Test]
        public void Load_CalledMultipleTimes_ShouldReturnSameConfigurationInstance()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var first = provider.Load();
            var second = provider.Load();

            // Assert
            Assert.That(first, Is.SameAs(configuration));
            Assert.That(second, Is.SameAs(configuration));
        }

        [Test]
        public void Load_CalledMultipleTimes_ShouldAlwaysReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var first = provider.Load();
            var second = provider.Load();
            var third = provider.Load();

            // Assert
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(third, Is.Not.Null);
        }

        [Test]
        public void CreateDefault_ShouldReturnValidProvider()
        {
            // Arrange
            // Act
            var provider = InMemoryConfigurationProvider.CreateDefault();

            // Assert
            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.Load(), Is.Not.Null);
        }

        [Test]
        public void CreateDefault_ShouldConfigureRootLogLevelAsDebug()
        {
            // Arrange
            var provider = InMemoryConfigurationProvider.CreateDefault();

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(configuration.RootLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void CreateDefault_ShouldCreateSingleAppender()
        {
            // Arrange
            var provider = InMemoryConfigurationProvider.CreateDefault();

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(configuration.Appenders, Has.Count.EqualTo(1));
        }

        [Test]
        public void CreateDefault_ShouldConfigureConsoleAppender()
        {
            // Arrange
            var provider = InMemoryConfigurationProvider.CreateDefault();

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(configuration.Appenders[0].Destination.Type, Is.EqualTo(LogOutputDestination.Console));
        }

        [Test]
        public void CreateDefault_ShouldConfigureAppenderLogLevelAsDebug()
        {
            // Arrange
            var provider = InMemoryConfigurationProvider.CreateDefault();

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(configuration.Appenders[0].AppenderLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void Load_WithNullAppenderCollection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders = null;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.Load());
            Assert.That(ex.Message, Is.EqualTo("At least one appender must be configured."));
        }

        [Test]
        public void Load_WithEmptyAppenderCollection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders = new List<AppenderConfiguration>();
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.Load());
            Assert.That(ex.Message, Is.EqualTo("At least one appender must be configured."));
        }

        [Test]
        public void Load_WithUnknownAppenderDestination_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Destination.Type = LogOutputDestination.Unknown;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.Load());
            Assert.That(ex.Message, Is.EqualTo("Appender destination must be specified."));
        }

        [Test]
        public void Load_WithMultipleValidAppenders_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders.Add(new AppenderConfiguration
            {
                Destination = new DestinationConfiguration { Type = LogOutputDestination.FileSystem }
            });
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders, Has.Count.EqualTo(2));
        }

        [Test]
        public void Load_WithConsoleAppender_ShouldSucceed()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Destination.Type = LogOutputDestination.Console;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders[0].Destination.Type, Is.EqualTo(LogOutputDestination.Console));
        }

        [Test]
        public void Load_WithFileSystemAppender_ShouldSucceed()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Destination.Type = LogOutputDestination.FileSystem;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders[0].Destination.Type, Is.EqualTo(LogOutputDestination.FileSystem));
        }

        [Test]
        public void Load_WithDatabaseAppender_ShouldSucceed()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Destination.Type = LogOutputDestination.DatabaseSystem;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders[0].Destination.Type, Is.EqualTo(LogOutputDestination.DatabaseSystem));
        }

        [TestCase(LogLevel.DEBUG)]
        [TestCase(LogLevel.INFO)]
        [TestCase(LogLevel.WARNING)]
        [TestCase(LogLevel.ERROR)]
        [TestCase(LogLevel.FATAL)]
        public void Load_WithRootLogLevelDebug_ShouldSucceed(LogLevel logLevel)
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.RootLogLevel = logLevel;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.RootLogLevel, Is.EqualTo(logLevel));
        }

        [Test]
        public void Load_WithLoggerOverrides_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.LoggerOverrides.Add(new LoggerOverrideConfiguration { LoggerName = "My.Logger", LogLevel = LogLevel.DEBUG });
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.LoggerOverrides, Has.Count.EqualTo(1));
        }

        [Test]
        public void Load_WithFormatterConfiguration_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Formatter = new FormatterConfiguration { OutputFormat = LogOutputFormat.Json };
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders[0].Formatter.OutputFormat, Is.EqualTo(LogOutputFormat.Json));
        }

        [Test]
        public void Load_WithFileDestinationConfiguration_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.Appenders[0].Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.FileSystem,
                File = new FileConfiguration { FileName = "logs/app", Extension = "log" }
            };
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders[0].Destination.File.FileName, Is.EqualTo("logs/app"));
        }

        [Test]
        public void Load_WithAsyncLoggingEnabled_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            configuration.EnableAsyncLoggingProcess = true;
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.EnableAsyncLoggingProcess, Is.True);
        }

        [Test]
        public void Load_WithManyAppenders_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            for (var i = 0; i < 100; i++)
            {
                configuration.Appenders.Add(new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration { Type = LogOutputDestination.Console }
                });
            }
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.Appenders, Has.Count.EqualTo(101));
        }

        [Test]
        public void Load_WithManyLoggerOverrides_ShouldReturnConfiguration()
        {
            // Arrange
            var configuration = CreateValidConfiguration();
            for (var i = 0; i < 100; i++)
            {
                configuration.LoggerOverrides.Add(new LoggerOverrideConfiguration
                {
                    LoggerName = $"Logger{i}",
                    LogLevel = LogLevel.INFO
                });
            }
            var provider = new InMemoryConfigurationProvider(configuration);

            // Act
            var loaded = provider.Load();

            // Assert
            Assert.That(loaded.LoggerOverrides, Has.Count.EqualTo(100));
        }

        private static LogConfigurationHolder CreateValidConfiguration()
        {
            return new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
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
        }
    }
}
