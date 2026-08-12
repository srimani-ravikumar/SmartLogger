using Moq;
using SmartLogger.Appenders;
using SmartLogger.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LoggerFactoryTests
    {
        private Mock<ILogConfigurationProvider> _provider;

        [SetUp]
        public void Setup()
        {
            ResetFileAppenderRegistry();
            _provider = new Mock<ILogConfigurationProvider>();

            // Ensure logs directory exists for file appender tests
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            // Ensure default Logs directory exists
            var defaultLogsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(defaultLogsDir))
            {
                Directory.CreateDirectory(defaultLogsDir);
            }
        }

        private static AppenderConfiguration CreateConsoleAppenderConfiguration(LogLevel? appenderLevel = null)
        {
            return new AppenderConfiguration
            {
                AppenderLogLevel = appenderLevel,
                Destination = new DestinationConfiguration
                {
                    Type = LogOutputDestination.Console
                },
                Formatter = new FormatterConfiguration
                {
                    OutputFormat = LogOutputFormat.PlainText,
                    LayoutType = LogMessageLayoutType.Simple
                }
            };
        }

        private static AppenderConfiguration CreateFileAppenderConfiguration(string basePath, LogLevel? appenderLevel = null, LogOutputFormat outputFormat = LogOutputFormat.PlainText)
        {
            return new AppenderConfiguration
            {
                AppenderLogLevel = appenderLevel,
                Destination = new DestinationConfiguration
                {
                    Type = LogOutputDestination.FileSystem,
                    File = new FileConfiguration
                    {
                        Directory = "logs",
                        FileName = Path.GetFileName(basePath),
                        Extension = "log"
                    }
                },
                Formatter = new FormatterConfiguration
                {
                    OutputFormat = outputFormat,
                    LayoutType = LogMessageLayoutType.Simple
                }
            };
        }

        private static LoggerFactory CreateFactory(LogConfigurationHolder configuration)
        {
            var provider = new Mock<ILogConfigurationProvider>();
            provider.Setup(x => x.Load()).Returns(configuration);
            return new LoggerFactory(provider.Object);
        }

        private static void ResetFileAppenderRegistry()
        {
            var cacheField = typeof(FileAppenderRegistry).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Static);
            var cache = cacheField?.GetValue(null) as ConcurrentDictionary<string, ILogAppender>;
            cache?.Clear();
        }

        [Test]
        public void Constructor_WithValidProvider_ShouldLoadConfiguration()
        {
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            var factory = new LoggerFactory(_provider.Object);

            Assert.That(factory, Is.Not.Null);
            _provider.Verify(x => x.Load(), Times.Once);
        }

        [Test]
        public void Constructor_WithNullProvider_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new LoggerFactory(null!));
            Assert.That(ex!.ParamName, Is.EqualTo("provider"));
        }

        [Test]
        public void Constructor_WhenProviderLoadThrows_ShouldPropagateException()
        {
            var expected = new InvalidOperationException("load failed");
            _provider.Setup(x => x.Load()).Throws(expected);

            var ex = Assert.Throws<InvalidOperationException>(() => new LoggerFactory(_provider.Object));
            Assert.That(ex, Is.SameAs(expected));
        }

        [Test]
        public void GetOrCreateLogger_WithValidName_ShouldReturnLogger()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var logger = factory.GetOrCreateLogger("MyLogger");

            Assert.That(logger, Is.Not.Null);
            Assert.That(logger, Is.InstanceOf<ISmartLogger>());
        }

        [Test]
        public void GetOrCreateLogger_WithSameName_ShouldReturnCachedInstance()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var first = factory.GetOrCreateLogger("MyLogger");
            var second = factory.GetOrCreateLogger("MyLogger");

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void GetOrCreateLogger_WithDifferentNames_ShouldReturnDifferentInstances()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var first = factory.GetOrCreateLogger("LoggerA");
            var second = factory.GetOrCreateLogger("LoggerB");

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void GetOrCreateLogger_WithVeryLongName_ShouldReturnLogger()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var logger = factory.GetOrCreateLogger(new string('A', 5000));

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WithUnicodeName_ShouldReturnLogger()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var logger = factory.GetOrCreateLogger("ஸ்மார்ட்Logger_日本語_日志");

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WithEmptyName_ShouldReturnLogger()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var logger = factory.GetOrCreateLogger(string.Empty);

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WithNullName_ShouldThrowArgumentNullException()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            Assert.Throws<ArgumentNullException>(() => factory.GetOrCreateLogger(null!));
        }

        [Test]
        public void GetOrCreateLogger_WithRepeatedRequests_ShouldReturnSameInstance()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var first = factory.GetOrCreateLogger("Repeated");
            var second = factory.GetOrCreateLogger("Repeated");
            var third = factory.GetOrCreateLogger("Repeated");

            Assert.That(first, Is.SameAs(second));
            Assert.That(first, Is.SameAs(third));
        }

        [Test]
        public void UpdateConfiguration_ShouldReuseExistingLoggerInstances()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var existing = factory.GetOrCreateLogger("CacheAware");

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.ERROR,
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var updated = factory.GetOrCreateLogger("CacheAware");

            Assert.That(updated, Is.SameAs(existing));
        }

        [Test]
        public void GetOrCreateLogger_WithoutAppenders_ShouldUseRootLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder { RootLogLevel = LogLevel.DEBUG });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("NoAppenders");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void GetOrCreateLogger_WithMultipleAppenders_ShouldUseLowestAppenderLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    CreateConsoleAppenderConfiguration(LogLevel.WARNING),
                    CreateConsoleAppenderConfiguration(LogLevel.INFO)
                }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("MultiAppender");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.INFO));
        }

        [Test]
        public void GetOrCreateLogger_WithAppenderWithoutLevel_ShouldUseRootLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.ERROR,
                Appenders = { CreateConsoleAppenderConfiguration(null) }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("AppenderWithoutLevel");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void GetOrCreateLogger_WithLoggerOverride_ShouldUseOverrideLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
                LoggerOverrides =
                {
                    new LoggerOverrideConfiguration
                    {
                        LoggerName = "OverrideLogger",
                        LogLevel = LogLevel.DEBUG
                    }
                }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("OverrideLogger");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void GetOrCreateLogger_WithMultipleOverrides_ShouldUseLastMatchingOverride()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
                LoggerOverrides =
                {
                    new LoggerOverrideConfiguration { LoggerName = "MultiOverride", LogLevel = LogLevel.DEBUG },
                    new LoggerOverrideConfiguration { LoggerName = "MultiOverride", LogLevel = LogLevel.WARNING }
                }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("MultiOverride");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.WARNING));
        }

        [Test]
        public void GetOrCreateLogger_WithConsoleAppender_ShouldAttachConsoleAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("ConsoleLogger");

            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateLogger_WithConfiguredAppender_ShouldDisableDefaultConsoleAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("ConfiguredLogger");

            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateLogger_WithoutConfiguredAppenders_ShouldEnableDefaultConsoleAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("DefaultConsoleLogger");

            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateLogger_WithFileAppender_ShouldAttachFileAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("FileLogger");

            Assert.That(logger.GetLogAppenders().OfType<FileAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateLogger_WithDifferentFileTargets_ShouldCreateDifferentFileAppenders()
        {
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            var firstLogger = (LoggerImplementation)firstFactory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)secondFactory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.Not.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithSharedFileConfiguration_ShouldReuseCachedAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });

            var firstLogger = (LoggerImplementation)firstFactory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)secondFactory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithSameFileButDifferentFormatter_ShouldReuseCachedAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget, outputFormat: LogOutputFormat.Json) }
            });

            var firstLogger = (LoggerImplementation)firstFactory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)secondFactory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithSameFileButDifferentLogLevel_ShouldReuseCachedAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget, LogLevel.INFO) }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget, LogLevel.WARNING) }
            });

            var firstLogger = (LoggerImplementation)firstFactory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)secondFactory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithSameFileButDifferentRollingStrategy_ShouldReuseCachedAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });

            firstFactory.GetOrCreateLogger("A");
            secondFactory.GetOrCreateLogger("B");

            var secondConfig = new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            };
            secondConfig.Appenders[0].Destination.File!.Rolling.Strategy = RollingStrategyType.Size;
            var thirdFactory = CreateFactory(secondConfig);
            var thirdLogger = (LoggerImplementation)thirdFactory.GetOrCreateLogger("C");

            Assert.That(thirdLogger.GetLogAppenders().Single(), Is.SameAs(((LoggerImplementation)firstFactory.GetOrCreateLogger("A")).GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithAsyncEnabled_ShouldWrapConsoleAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                EnableAsyncLoggingProcess = true,
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("AsyncConsoleLogger");

            Assert.That(logger.GetLogAppenders().Single(), Is.InstanceOf<AsyncAppenderWrapper>());
        }

        [Test]
        public void GetOrCreateLogger_WithAsyncDisabled_ShouldNotWrapAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                EnableAsyncLoggingProcess = false,
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("SyncConsoleLogger");

            Assert.That(logger.GetLogAppenders().Single(), Is.Not.InstanceOf<AsyncAppenderWrapper>());
        }

        [Test]
        public void GetOrCreateLogger_WithAsyncFileAppender_ShouldNotDoubleWrapAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var factory = CreateFactory(new LogConfigurationHolder
            {
                EnableAsyncLoggingProcess = true,
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });

            var firstLogger = (LoggerImplementation)factory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)factory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.InstanceOf<AsyncAppenderWrapper>());
            Assert.That(firstLogger.GetLogAppenders().Single(), Is.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void UpdateConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            var factory = CreateFactory(new LogConfigurationHolder());

            Assert.Throws<ArgumentNullException>(() => factory.UpdateConfiguration(null!));
        }

        [Test]
        public void UpdateConfiguration_ShouldRefreshLoggerLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder { RootLogLevel = LogLevel.INFO });
            var logger = (LoggerImplementation)factory.GetOrCreateLogger("RefreshLevel");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.INFO));

            factory.UpdateConfiguration(new LogConfigurationHolder { RootLogLevel = LogLevel.ERROR });

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void UpdateConfiguration_ShouldReplaceExistingAppenders()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("ReplaceAppenders");
            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(1));

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders =
                {
                    CreateConsoleAppenderConfiguration(LogLevel.ERROR),
                    CreateConsoleAppenderConfiguration(LogLevel.WARNING)
                }
            });

            Assert.That(logger.GetLogAppenders().Count, Is.EqualTo(2));
        }

        [Test]
        public void UpdateConfiguration_ShouldRetainLoggerCache()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var first = factory.GetOrCreateLogger("RetainLogger");

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var second = factory.GetOrCreateLogger("RetainLogger");

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void UpdateConfiguration_WithUpdatedOverride_ShouldRefreshEffectiveLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
                LoggerOverrides = { new LoggerOverrideConfiguration { LoggerName = "OverrideRefresh", LogLevel = LogLevel.INFO } }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("OverrideRefresh");
            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.INFO));

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.ERROR,
                LoggerOverrides = { new LoggerOverrideConfiguration { LoggerName = "OverrideRefresh", LogLevel = LogLevel.DEBUG } }
            });

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void UpdateConfiguration_ShouldRebuildAppenderCollection()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateConsoleAppenderConfiguration() }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("RebuildAppenders");
            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(1));

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            Assert.That(logger.GetLogAppenders().OfType<ConsoleAppender>().Count, Is.EqualTo(0));
            Assert.That(logger.GetLogAppenders().OfType<FileAppender>().Count, Is.EqualTo(1));
        }

        [Test]
        public void UpdateConfiguration_ShouldCreateNewFileAppendersInsteadOfRegistryInstances()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("RegistryReload");
            var originalAppender = logger.GetLogAppenders().Single();

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            var updatedAppender = logger.GetLogAppenders().Single();

            Assert.That(updatedAppender, Is.Not.SameAs(originalAppender));
        }

        [Test]
        public void UpdateConfiguration_WithMultipleCachedLoggers_ShouldRefreshAllLoggers()
        {
            var factory = CreateFactory(new LogConfigurationHolder { RootLogLevel = LogLevel.INFO });

            var first = (LoggerImplementation)factory.GetOrCreateLogger("A");
            var second = (LoggerImplementation)factory.GetOrCreateLogger("B");

            factory.UpdateConfiguration(new LogConfigurationHolder { RootLogLevel = LogLevel.WARNING });

            Assert.That(first.EffectiveMinLogLevel, Is.EqualTo(LogLevel.WARNING));
            Assert.That(second.EffectiveMinLogLevel, Is.EqualTo(LogLevel.WARNING));
        }

        [Test]
        public void UpdateConfiguration_WithNonLoggerImplementation_ShouldSkipUpdate()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var fakeLogger = new Mock<ISmartLogger>().Object;
            var loggersField = typeof(LoggerFactory).GetField("_loggers", BindingFlags.Instance | BindingFlags.NonPublic);
            var loggers = (ConcurrentDictionary<string, ISmartLogger>)loggersField!.GetValue(factory)!;
            loggers.TryAdd("Fake", fakeLogger);

            Assert.DoesNotThrow(() => factory.UpdateConfiguration(new LogConfigurationHolder()));
        }

        [Test]
        public void GetOrCreateLogger_WithUnsupportedDestination_ShouldThrowNotSupportedException()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration { Type = (LogOutputDestination)999 },
                        Formatter = new FormatterConfiguration()
                    }
                }
            });

            Assert.Throws<NotSupportedException>(() => factory.GetOrCreateLogger("UnsupportedDestination"));
        }

        [Test]
        public void GetOrCreateLogger_WithUnsupportedFormatter_ShouldThrowNotSupportedException()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration { Type = LogOutputDestination.Console },
                        Formatter = new FormatterConfiguration { OutputFormat = (LogOutputFormat)2 }
                    }
                }
            });

            Assert.Throws<NotSupportedException>(() => factory.GetOrCreateLogger("UnsupportedFormatter"));
        }

        [Test]
        public void GetOrCreateLogger_WithNoRollingStrategy_ShouldCreateFileAppender()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration($"logs/{Guid.NewGuid():N}") }
            });

            var logger = factory.GetOrCreateLogger("NoRollingStrategy");

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WhenAppenderCreationFails_ShouldPropagateException()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration { Type = (LogOutputDestination)999 },
                        Formatter = new FormatterConfiguration()
                    }
                }
            });

            Assert.Throws<NotSupportedException>(() => factory.GetOrCreateLogger("Boom"));
        }

        [Test]
        public void UpdateConfiguration_WhenAppenderCreationFails_ShouldPropagateException()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            factory.GetOrCreateLogger("Existing");

            Assert.Throws<NotSupportedException>(() => factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration { Type = (LogOutputDestination)999 },
                        Formatter = new FormatterConfiguration()
                    }
                }
            }));
        }

        [Test]
        public void UpdateConfiguration_WithUnsupportedFormatter_ShouldThrowNotSupportedException()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            factory.GetOrCreateLogger("Existing");

            Assert.Throws<NotSupportedException>(() => factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration { Type = LogOutputDestination.Console },
                        Formatter = new FormatterConfiguration { OutputFormat = (LogOutputFormat)2 }
                    }
                }
            }));
        }

        [Test]
        public void GetOrCreateLogger_WhenCalledConcurrently_ShouldCreateSingleLoggerInstance()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var results = new ConcurrentBag<ISmartLogger>();

            Parallel.For(0, 50, _ => results.Add(factory.GetOrCreateLogger("ConcurrentLogger")));

            Assert.That(results.Distinct().Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateLogger_WithConcurrentDifferentNames_ShouldCreateIndependentLoggers()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var results = new ConcurrentBag<ISmartLogger>();

            Parallel.For(0, 20, index => results.Add(factory.GetOrCreateLogger($"Logger-{index % 5}")));

            Assert.That(results.Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public void UpdateConfiguration_DuringConcurrentLoggerAccess_ShouldRemainConsistent()
        {
            var factory = CreateFactory(new LogConfigurationHolder());
            var start = new System.Threading.ManualResetEventSlim(false);
            var tasks = new ConcurrentBag<Task>();

            for (var i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    start.Wait();
                    factory.GetOrCreateLogger("Shared");
                    factory.GetOrCreateLogger($"Shared-{i}");
                }));
            }

            start.Set();

            Assert.DoesNotThrow(() => Task.WaitAll(tasks.ToArray()));
        }

        [Test]
        public void GetOrCreateLogger_WithOverrideAndAppenders_ShouldPreferOverrideLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
                Appenders = { CreateConsoleAppenderConfiguration(LogLevel.WARNING) },
                LoggerOverrides =
                {
                    new LoggerOverrideConfiguration
                    {
                        LoggerName = "PreferredLogger",
                        LogLevel = LogLevel.DEBUG
                    }
                }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("PreferredLogger");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void GetOrCreateLogger_WithNonMatchingOverride_ShouldUseResolvedLogLevel()
        {
            var factory = CreateFactory(new LogConfigurationHolder
            {
                RootLogLevel = LogLevel.INFO,
                Appenders = { CreateConsoleAppenderConfiguration(LogLevel.WARNING) },
                LoggerOverrides =
                {
                    new LoggerOverrideConfiguration
                    {
                        LoggerName = "OtherLogger",
                        LogLevel = LogLevel.DEBUG
                    }
                }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("UnmatchedLogger");

            Assert.That(logger.EffectiveMinLogLevel, Is.EqualTo(LogLevel.WARNING));
        }

        [Test]
        public void GetOrCreateLogger_WithSameFileNameInDifferentDirectories_ShouldReuseCachedAppender()
        {
            var fileName = "test";
            var firstFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = fileName,
                                Extension = "log"
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            });
            var secondFactory = CreateFactory(new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = fileName,
                                Extension = "log"
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            });

            var firstLogger = (LoggerImplementation)firstFactory.GetOrCreateLogger("A");
            var secondLogger = (LoggerImplementation)secondFactory.GetOrCreateLogger("B");

            Assert.That(firstLogger.GetLogAppenders().Single(), Is.SameAs(secondLogger.GetLogAppenders().Single()));
        }

        [Test]
        public void GetOrCreateLogger_WithDateNamingStrategy_ShouldCreateFileAppender()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = Guid.NewGuid().ToString("N"),
                                Extension = "log",
                                Naming = new FileNamingConfiguration
                                {
                                    Strategy = FileNamingStrategyType.Date
                                }
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);
            var logger = factory.GetOrCreateLogger("DateNamingLogger");

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WithDailyRollingStrategy_ShouldCreateFileAppender()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = Guid.NewGuid().ToString("N"),
                                Extension = "log",
                                Rolling = new FileRollingConfiguration
                                {
                                    Strategy = RollingStrategyType.Daily
                                }
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);
            var logger = factory.GetOrCreateLogger("DailyRollingLogger");

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void GetOrCreateLogger_WithSizeRollingStrategy_ShouldCreateFileAppender()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = Guid.NewGuid().ToString("N"),
                                Extension = "log",
                                Rolling = new FileRollingConfiguration
                                {
                                    Strategy = RollingStrategyType.Size,
                                    MaxFileSizeMB = 1
                                }
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);
            var logger = factory.GetOrCreateLogger("SizeRollingLogger");

            Assert.That(logger, Is.Not.Null);
        }

        [Test]
        public void UpdateConfiguration_ShouldRefreshCachedFileAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var firstConfig = new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget, LogLevel.INFO) }
            };

            var factory = CreateFactory(firstConfig);
            var logger = (LoggerImplementation)factory.GetOrCreateLogger("RefreshAppender");
            var originalAppender = logger.GetLogAppenders().Single();

            var secondConfig = new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget, LogLevel.DEBUG) }
            };
            factory.UpdateConfiguration(secondConfig);

            var updatedAppender = logger.GetLogAppenders().Single();

            Assert.That(updatedAppender, Is.SameAs(originalAppender));
        }

        [Test]
        public void UpdateConfiguration_ShouldReuseExistingFileAppender()
        {
            var fileTarget = $"logs/{Guid.NewGuid():N}";
            var factory = CreateFactory(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });

            var logger = (LoggerImplementation)factory.GetOrCreateLogger("ReuseAppender");
            var originalAppender = logger.GetLogAppenders().Single();

            factory.UpdateConfiguration(new LogConfigurationHolder
            {
                Appenders = { CreateFileAppenderConfiguration(fileTarget) }
            });

            var reusedAppender = logger.GetLogAppenders().Single();

            Assert.That(reusedAppender, Is.SameAs(originalAppender));
        }

        [Test]
        public void GetOrCreateLogger_WithUnsupportedNamingStrategy_ShouldThrowNotSupportedException()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = Guid.NewGuid().ToString("N"),
                                Extension = "log",
                                Naming = new FileNamingConfiguration
                                {
                                    Strategy = (FileNamingStrategyType)999
                                }
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);

            Assert.Throws<NotSupportedException>(() => factory.GetOrCreateLogger("UnsupportedNamingStrategy"));
        }

        [Test]
        public void GetOrCreateLogger_WithUnsupportedRollingStrategy_ShouldThrowNotSupportedException()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = new FileConfiguration
                            {
                                Directory = "logs",
                                FileName = Guid.NewGuid().ToString("N"),
                                Extension = "log",
                                Rolling = new FileRollingConfiguration
                                {
                                    Strategy = (RollingStrategyType)999
                                }
                            }
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);

            Assert.Throws<NotSupportedException>(() => factory.GetOrCreateLogger("UnsupportedRollingStrategy"));
        }

        [Test]
        public void GetOrCreateLogger_WithFileDestinationAndMissingFileConfiguration_ShouldThrow()
        {
            var config = new LogConfigurationHolder
            {
                Appenders =
                {
                    new AppenderConfiguration
                    {
                        Destination = new DestinationConfiguration
                        {
                            Type = LogOutputDestination.FileSystem,
                            File = null
                        },
                        Formatter = new FormatterConfiguration
                        {
                            OutputFormat = LogOutputFormat.PlainText,
                            LayoutType = LogMessageLayoutType.Simple
                        }
                    }
                }
            };

            var factory = CreateFactory(config);

            Assert.Throws<NullReferenceException>(() => factory.GetOrCreateLogger("MissingFileConfig"));
        }
    }
}
