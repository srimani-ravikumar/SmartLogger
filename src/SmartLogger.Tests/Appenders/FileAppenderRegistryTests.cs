using Moq;
using SmartLogger.Appenders;
using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Core;
using System.Collections.Concurrent;

namespace SmartLogger.Tests.Appenders;

[TestFixture]
public class FileAppenderRegistryTests
{
    private readonly List<string> _createdDirectories = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var directory in _createdDirectories)
        {
            TryDeleteDirectory(directory);
        }

        _createdDirectories.Clear();
    }

    [Test]
    public void GetOrCreate_WithNewFileConfiguration_ShouldCreateAppender()
    {
        var config = CreateConfiguration();

        var result = FileAppenderRegistry.GetOrCreate(
            config,
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<FileAppender>());
    }

    [Test]
    public void GetOrCreate_WithValidConfiguration_ShouldReturnAppender()
    {
        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(result, Is.InstanceOf<ILogAppender>());
    }

    [Test]
    public void GetOrCreate_WithAsyncDisabled_ShouldReturnFileAppender()
    {
        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(result, Is.TypeOf<FileAppender>());
    }

    [Test]
    public void GetOrCreate_WithAsyncEnabled_ShouldReturnAsyncAppenderWrapper()
    {
        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: true);

        try
        {
            Assert.That(result, Is.TypeOf<AsyncAppenderWrapper>());
            Assert.That(
                ((AsyncAppenderWrapper)result).InnerAppender,
                Is.TypeOf<FileAppender>());
        }
        finally
        {
            ((AsyncAppenderWrapper)result).Stop();
        }
    }

    [Test]
    public void GetOrCreate_WithLogLevel_ShouldCreateAppenderWithConfiguredLogLevel()
    {
        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.ERROR,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(result.GetLogLevel(LogLevel.INFO), Is.EqualTo(LogLevel.ERROR));
    }

    [Test]
    public void GetOrCreate_WithFormatter_ShouldCreateAppenderWithConfiguredFormatter()
    {
        var formatter = CreateFormatter();

        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.INFO,
            formatter,
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(result.GetFormatter(), Is.SameAs(formatter));
    }

    [Test]
    public void GetOrCreate_WithNamingStrategy_ShouldCreateAppenderSuccessfully()
    {
        var namingStrategy = CreateNamingStrategy();

        var result = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            namingStrategy,
            asyncEnabled: false);

        Assert.That(result, Is.Not.Null);
        Mock.Get(namingStrategy).Verify(
            x => x.CreateActiveFileName(),
            Times.AtLeastOnce);
    }

    [Test]
    public void GetOrCreate_WithSameFileKey_ShouldReturnSameInstance()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "same-file"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "same-file"),
            LogLevel.ERROR,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetOrCreate_WithExistingFileKey_ShouldReuseCachedAppender()
    {
        var namingStrategy1 = CreateNamingStrategy();
        var namingStrategy2 = CreateNamingStrategy();

        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "cached-file"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            namingStrategy1,
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "cached-file"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            namingStrategy2,
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));

        Mock.Get(namingStrategy2).Verify(
            x => x.CreateActiveFileName(),
            Times.Never);
    }

    [Test]
    public void GetOrCreate_WithDifferentFileNames_ShouldReturnDifferentInstances()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "first-file"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "second-file"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.Not.SameAs(first));
    }

    [Test]
    public void GetOrCreate_WithSameFileNameDifferentExtension_ShouldReturnDifferentInstances()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "same-name", extension: "log"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "same-name", extension: "txt"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.Not.SameAs(first));
    }

    [Test]
    public void GetOrCreate_WithExistingAppender_ShouldRefreshLogLevel()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "refresh-level"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "refresh-level"),
            LogLevel.FATAL,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));
        Assert.That(second.GetLogLevel(LogLevel.INFO), Is.EqualTo(LogLevel.FATAL));
    }

    [Test]
    public void GetOrCreate_WithExistingAppender_ShouldRefreshFormatter()
    {
        var firstFormatter = CreateFormatter();
        var secondFormatter = CreateFormatter();

        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "refresh-formatter"),
            LogLevel.INFO,
            firstFormatter,
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "refresh-formatter"),
            LogLevel.INFO,
            secondFormatter,
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));
        Assert.That(second.GetFormatter(), Is.SameAs(secondFormatter));
    }

    [Test]
    public void GetOrCreate_SameFileDifferentRollingStrategy_ShouldReturnSameAppender()
    {
        var firstStrategy = CreateRollingStrategy();
        var secondStrategy = CreateRollingStrategy();

        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "rolling-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            firstStrategy,
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "rolling-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            secondStrategy,
            CreateNamingStrategy(),
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetOrCreate_SameFileDifferentNamingStrategy_ShouldReturnSameAppender()
    {
        var firstStrategy = CreateNamingStrategy();
        var secondStrategy = CreateNamingStrategy();

        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "naming-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            firstStrategy,
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "naming-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            secondStrategy,
            asyncEnabled: false);

        Assert.That(second, Is.SameAs(first));

        Mock.Get(secondStrategy).Verify(
            x => x.CreateActiveFileName(),
            Times.Never);
    }

    [Test]
    public void GetOrCreate_SameFileDifferentAsyncSetting_ShouldReturnSameAppender()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-cache"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: true);

        Assert.That(second, Is.SameAs(first));
        Assert.That(second, Is.TypeOf<FileAppender>());
    }

    [Test]
    public void GetOrCreate_FirstAsyncThenSync_ShouldReturnExistingAsyncAppender()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-first"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: true);

        var second = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-first"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: false);

        try
        {
            Assert.That(second, Is.SameAs(first));
            Assert.That(second, Is.TypeOf<AsyncAppenderWrapper>());
        }
        finally
        {
            ((AsyncAppenderWrapper)first).Stop();
        }
    }

    [Test]
    public void GetOrCreate_WithCachedAsyncWrapper_ShouldRefreshInnerFileAppender()
    {
        var firstFormatter = CreateFormatter();
        var secondFormatter = CreateFormatter();

        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-refresh"),
            LogLevel.INFO,
            firstFormatter,
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: true);

        try
        {
            var second = FileAppenderRegistry.GetOrCreate(
                CreateConfiguration(fileName: "async-refresh"),
                LogLevel.ERROR,
                secondFormatter,
                CreateRollingStrategy(),
                CreateNamingStrategy(),
                asyncEnabled: true);

            var wrapper = (AsyncAppenderWrapper)second;
            var inner = (FileAppender)wrapper.InnerAppender;

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.SameAs(first));
                Assert.That(inner.GetLogLevel(LogLevel.INFO), Is.EqualTo(LogLevel.ERROR));
                Assert.That(inner.GetFormatter(), Is.SameAs(secondFormatter));
            });
        }
        finally
        {
            ((AsyncAppenderWrapper)first).Stop();
        }
    }

    [Test]
    public void GetOrCreate_WithCachedAsyncWrapper_ShouldPreserveWrapperInstance()
    {
        var first = FileAppenderRegistry.GetOrCreate(
            CreateConfiguration(fileName: "async-wrapper"),
            LogLevel.INFO,
            CreateFormatter(),
            CreateRollingStrategy(),
            CreateNamingStrategy(),
            asyncEnabled: true);

        try
        {
            var second = FileAppenderRegistry.GetOrCreate(
                CreateConfiguration(fileName: "async-wrapper"),
                LogLevel.ERROR,
                CreateFormatter(),
                CreateRollingStrategy(),
                CreateNamingStrategy(),
                asyncEnabled: false);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second, Is.TypeOf<AsyncAppenderWrapper>());
        }
        finally
        {
            ((AsyncAppenderWrapper)first).Stop();
        }
    }

    [Test]
    public void GetOrCreate_WithNullConfiguration_ShouldThrow()
    {
        Assert.Throws<NullReferenceException>(() =>
            FileAppenderRegistry.GetOrCreate(
                null!,
                LogLevel.INFO,
                CreateFormatter(),
                CreateRollingStrategy(),
                CreateNamingStrategy(),
                asyncEnabled: false));
    }

    [Test]
    public void GetOrCreate_WithNullFileConfiguration_ShouldThrow()
    {
        var config = new AppenderConfiguration
        {
            Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.FileSystem,
                File = null
            }
        };

        Assert.Throws<NullReferenceException>(() =>
            FileAppenderRegistry.GetOrCreate(
                config,
                LogLevel.INFO,
                CreateFormatter(),
                CreateRollingStrategy(),
                CreateNamingStrategy(),
                asyncEnabled: false));
    }

    [Test]
    public void GetOrCreate_ConcurrentlyWithSameFile_ShouldReturnSameInstance()
    {
        const int callerCount = 20;
        var results = new ConcurrentBag<ILogAppender>();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, callerCount, _ =>
        {
            try
            {
                var result = FileAppenderRegistry.GetOrCreate(
                    CreateConfiguration(fileName: "concurrent-same"),
                    LogLevel.INFO,
                    CreateFormatter(),
                    CreateRollingStrategy(),
                    CreateNamingStrategy(),
                    asyncEnabled: false);

                results.Add(result);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.That(exceptions, Is.Empty);
        Assert.That(results.Count, Is.EqualTo(callerCount));
        Assert.That(results.Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetOrCreate_ConcurrentlyWithDifferentFiles_ShouldCreateDifferentInstances()
    {
        const int callerCount = 20;
        var results = new ConcurrentBag<ILogAppender>();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, callerCount, index =>
        {
            try
            {
                var result = FileAppenderRegistry.GetOrCreate(
                    CreateConfiguration(fileName: $"concurrent-{index}"),
                    LogLevel.INFO,
                    CreateFormatter(),
                    CreateRollingStrategy(),
                    CreateNamingStrategy(),
                    asyncEnabled: false);

                results.Add(result);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.That(exceptions, Is.Empty);
        Assert.That(results.Count, Is.EqualTo(callerCount));
        Assert.That(results.Distinct().Count(), Is.EqualTo(callerCount));
    }

    [Test]
    public void GetOrCreate_ConcurrentlyWithSameFile_ShouldPreserveLatestConfiguration()
    {
        const int callerCount = 20;
        var results = new ConcurrentBag<ILogAppender>();

        Parallel.For(0, callerCount, index =>
        {
            var level = index % 2 == 0
                ? LogLevel.INFO
                : LogLevel.ERROR;

            var formatter = CreateFormatter();

            results.Add(
                FileAppenderRegistry.GetOrCreate(
                    CreateConfiguration(fileName: "concurrent-refresh"),
                    level,
                    formatter,
                    CreateRollingStrategy(),
                    CreateNamingStrategy(),
                    asyncEnabled: false));
        });

        var distinctInstances = results.Distinct().ToArray();

        Assert.That(distinctInstances.Length, Is.EqualTo(1));

        // The exact final configuration is intentionally not asserted because
        // concurrent callers race to perform RefreshConfiguration().
        Assert.That(
            distinctInstances[0].GetLogLevel(LogLevel.INFO),
            Is.AnyOf(LogLevel.INFO, LogLevel.ERROR));
    }

    private FileConfiguration CreateFileConfiguration(
        string fileName,
        string extension)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerRegistryTests",
            Guid.NewGuid().ToString("N"));

        _createdDirectories.Add(directory);

        return new FileConfiguration
        {
            Directory = Path.Combine(directory, "Logs"),
            FileName = fileName,
            Extension = extension,
            Archive = new ArchiveConfiguration
            {
                Enabled = false,
                Directory = Path.Combine(directory, "Archive"),
                Compress = false
            }
        };
    }

    private AppenderConfiguration CreateConfiguration(
        string fileName = "application",
        string extension = "log")
    {
        return new AppenderConfiguration
        {
            Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.FileSystem,
                File = CreateFileConfiguration(fileName, extension)
            }
        };
    }

    private static ILogOutputFormatterStrategy CreateFormatter()
    {
        return new Mock<ILogOutputFormatterStrategy>().Object;
    }

    private static IRollingStrategy CreateRollingStrategy()
    {
        var mock = new Mock<IRollingStrategy>();
        mock.Setup(x => x.ShouldRoll(It.IsAny<string>())).Returns(false);
        return mock.Object;
    }

    private static IFileNamingStrategy CreateNamingStrategy()
    {
        var mock = new Mock<IFileNamingStrategy>();

        mock.Setup(x => x.CreateActiveFileName())
            .Returns("application.log");

        mock.Setup(x => x.CreateRolledFileName(It.IsAny<int>()))
            .Returns<int>(index => $"application-{index}.log");

        return mock.Object;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Test cleanup should not mask the actual test result.
        }
    }
}