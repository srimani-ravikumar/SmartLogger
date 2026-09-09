using Moq;
using SmartLogger.Appenders.FileNaming;
using SmartLogger.Appenders.FileRolling;
using SmartLogger.Appenders.FileSystem;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileSystem;

[TestFixture]
internal sealed class FileLifecycleManagerTests
{
    private string _testDirectory = null!;
    private string _activeDirectory = null!;
    private string _archiveDirectory = null!;

    private Mock<IRollingStrategy> _rollingStrategy = null!;
    private Mock<IFileNamingStrategy> _namingStrategy = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "SmartLoggerTests",
            Guid.NewGuid().ToString("N"));

        _activeDirectory = Path.Combine(_testDirectory, "Active");
        _archiveDirectory = Path.Combine(_testDirectory, "Archive");

        _rollingStrategy = new Mock<IRollingStrategy>();
        _namingStrategy = new Mock<IFileNamingStrategy>();

        _namingStrategy
            .Setup(x => x.CreateActiveFileName())
            .Returns("application.log");

        _namingStrategy
            .Setup(x => x.CreateRolledFileName(It.IsAny<int>()))
            .Returns((int index) => $"application-{index}.log");
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
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new FileLifecycleManager(
                null!,
                _rollingStrategy.Object,
                _namingStrategy.Object),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithNullNamingStrategy_ThrowsArgumentNullException()
    {
        var configuration = CreateConfiguration();

        Assert.That(
            () => new FileLifecycleManager(
                configuration,
                _rollingStrategy.Object,
                null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithNullRollingStrategy_AllowsConstruction()
    {
        var configuration = CreateConfiguration();

        Assert.That(
            () => CreateManager(configuration, null),
            Throws.Nothing);
    }

    [Test]
    public void Constructor_CreatesActiveDirectory()
    {
        var configuration = CreateConfiguration();

        CreateManager(configuration);

        Assert.That(Directory.Exists(_activeDirectory), Is.True);
    }

    [Test]
    public void Constructor_WithArchiveEnabled_CreatesArchiveDirectory()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;

        CreateManager(configuration);

        Assert.That(Directory.Exists(_archiveDirectory), Is.True);
    }

    [Test]
    public void Constructor_WithArchiveDisabled_DoesNotCreateArchiveDirectory()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = false;

        CreateManager(configuration);

        Assert.That(Directory.Exists(_archiveDirectory), Is.False);
    }

    [Test]
    public void Constructor_CreatesInitialActiveFile()
    {
        var configuration = CreateConfiguration();

        CreateManager(configuration);

        var activeFile = Path.Combine(_activeDirectory, "application.log");

        Assert.That(File.Exists(activeFile), Is.True);
    }

    [Test]
    public void Constructor_CreatesActiveFileUsingNamingStrategy()
    {
        var configuration = CreateConfiguration();

        CreateManager(configuration);

        _namingStrategy.Verify(
            x => x.CreateActiveFileName(),
            Times.Once);
    }

    [Test]
    public void Constructor_WhenActiveFileAlreadyExists_PreservesExistingContent()
    {
        Directory.CreateDirectory(_activeDirectory);

        var activeFile = Path.Combine(_activeDirectory, "application.log");
        File.WriteAllText(activeFile, "existing content");

        var configuration = CreateConfiguration();

        CreateManager(configuration);

        Assert.That(
            File.ReadAllText(activeFile),
            Is.EqualTo("existing content"));
    }

    [Test]
    public void Write_WithNullMessage_DoesNotWrite()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        var activeFile = GetActiveFilePath();

        manager.Write(null!);

        Assert.That(File.ReadAllText(activeFile), Is.Empty);
    }

    [Test]
    public void Write_WithEmptyMessage_DoesNotWrite()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        var activeFile = GetActiveFilePath();

        manager.Write(string.Empty);

        Assert.That(File.ReadAllText(activeFile), Is.Empty);
    }

    [Test]
    public void Write_WithWhitespaceMessage_DoesNotWrite()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        var activeFile = GetActiveFilePath();

        manager.Write("   ");

        Assert.That(File.ReadAllText(activeFile), Is.Empty);
    }

    [Test]
    public void Write_WithValidMessage_AppendsMessageToActiveFile()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        manager.Write("Hello");

        Assert.That(
            File.ReadAllText(GetActiveFilePath()),
            Is.EqualTo("Hello" + Environment.NewLine));
    }

    [Test]
    public void Write_MultipleMessages_AppendsAllMessagesInOrder()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        manager.Write("First");
        manager.Write("Second");
        manager.Write("Third");

        Assert.That(
            File.ReadAllText(GetActiveFilePath()),
            Is.EqualTo(
                "First" + Environment.NewLine +
                "Second" + Environment.NewLine +
                "Third" + Environment.NewLine));
    }

    [Test]
    public void Write_WhenActiveFileIsMissing_RecreatesActiveFileAndWritesMessage()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration);

        File.Delete(GetActiveFilePath());

        manager.Write("Recovered");

        Assert.That(File.Exists(GetActiveFilePath()), Is.True);
        Assert.That(
            File.ReadAllText(GetActiveFilePath()),
            Is.EqualTo("Recovered" + Environment.NewLine));
    }

    [Test]
    public void Write_WithNullRollingStrategy_DoesNotAttemptRolling()
    {
        var configuration = CreateConfiguration();
        var manager = CreateManager(configuration, null);

        manager.Write("Message");

        _rollingStrategy.Verify(
            x => x.ShouldRoll(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void Write_WhenRollingStrategyReturnsFalse_DoesNotArchive()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(false);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(
            Directory.GetFiles(_archiveDirectory),
            Is.Empty);

        Assert.That(
            File.ReadAllText(GetActiveFilePath()),
            Is.EqualTo("Message" + Environment.NewLine));
    }

    [Test]
    public void Write_WhenRollingStrategyReturnsFalse_CallsStrategyWithActiveFilePath()
    {
        var configuration = CreateConfiguration();

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(false);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        _rollingStrategy.Verify(
            x => x.ShouldRoll(GetActiveFilePath()),
            Times.Once);
    }

    [Test]
    public void Write_WhenRollingStrategyReturnsTrue_MovesActiveFileToArchive()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Before roll");

        var archiveFile = Path.Combine(_archiveDirectory, "application-0.log");

        Assert.That(File.Exists(archiveFile), Is.True);
        Assert.That(File.Exists(GetActiveFilePath()), Is.True);
        Assert.That(
            File.ReadAllText(archiveFile),
            Is.EqualTo("Before roll" + Environment.NewLine));
    }

    [Test]
    public void Write_WhenRollingStrategyReturnsTrue_CreatesFreshActiveFile()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Before roll");
        manager.Write("After roll");

        Assert.That(
            File.ReadAllText(GetActiveFilePath()),
            Is.EqualTo("After roll" + Environment.NewLine));
    }

    [Test]
    public void Write_WhenRollingStrategyReturnsTrue_UsesRolledFileName()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        _namingStrategy.Verify(
            x => x.CreateRolledFileName(0),
            Times.Once);
    }

    [Test]
    public void Write_WhenArchiveNameExists_UsesNextAvailableIndex()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        Directory.CreateDirectory(_archiveDirectory);

        File.WriteAllText(
            Path.Combine(_archiveDirectory, "application-0.log"),
            "existing");

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(
            File.Exists(
                Path.Combine(_archiveDirectory, "application-1.log")),
            Is.True);

        _namingStrategy.Verify(
            x => x.CreateRolledFileName(0),
            Times.Once);

        _namingStrategy.Verify(
            x => x.CreateRolledFileName(1),
            Times.Once);
    }

    [Test]
    public void Write_WhenMultipleArchiveNamesExist_ContinuesUntilAvailable()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        Directory.CreateDirectory(_archiveDirectory);

        File.WriteAllText(
            Path.Combine(_archiveDirectory, "application-0.log"),
            "existing 0");

        File.WriteAllText(
            Path.Combine(_archiveDirectory, "application-1.log"),
            "existing 1");

        File.WriteAllText(
            Path.Combine(_archiveDirectory, "application-2.log"),
            "existing 2");

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(
            File.Exists(
                Path.Combine(_archiveDirectory, "application-3.log")),
            Is.True);
    }

    [Test]
    public void Write_WhenCompressionEnabled_CreatesZipArchive()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = true;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        var zipPath = Path.Combine(_archiveDirectory, "application-0.zip");

        Assert.That(File.Exists(zipPath), Is.True);
        Assert.That(
            File.Exists(
                Path.Combine(_archiveDirectory, "application-0.log")),
            Is.False);
    }

    [Test]
    public void Write_WhenCompressionDisabled_PreservesUncompressedArchive()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(
            File.Exists(
                Path.Combine(_archiveDirectory, "application-0.log")),
            Is.True);

        Assert.That(
            File.Exists(
                Path.Combine(_archiveDirectory, "application-0.zip")),
            Is.False);
    }

    [Test]
    public void Write_WhenArchiveEnabled_CleansUpExpiredArchivesAfterRoll()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;
        configuration.Retention.RetentionDays = 1;

        Directory.CreateDirectory(_archiveDirectory);

        var expiredArchive = Path.Combine(
            _archiveDirectory,
            "expired.log");

        File.WriteAllText(expiredArchive, "old");
        File.SetLastWriteTime(
            expiredArchive,
            DateTime.Now.AddDays(-2));

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(File.Exists(expiredArchive), Is.False);
    }

    [Test]
    public void Write_WhenNoRollOccurs_DoesNotModifyExistingArchives()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;
        configuration.Retention.RetentionDays = 1;

        Directory.CreateDirectory(_archiveDirectory);

        var archive = Path.Combine(
            _archiveDirectory,
            "existing.log");

        File.WriteAllText(archive, "existing");

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(false);

        var manager = CreateManager(configuration);

        manager.Write("Message");

        Assert.That(File.Exists(archive), Is.True);
        Assert.That(File.ReadAllText(archive), Is.EqualTo("existing"));
    }

    [Test]
    public void MultipleRolls_CreateSeparateArchives()
    {
        var configuration = CreateConfiguration();
        configuration.Archive.Enabled = true;
        configuration.Archive.Compress = false;

        _rollingStrategy
            .Setup(x => x.ShouldRoll(It.IsAny<string>()))
            .Returns(true);

        var manager = CreateManager(configuration);

        manager.Write("First");
        manager.Write("Second");

        var archives = Directory
            .GetFiles(_archiveDirectory, "*.log")
            .OrderBy(x => x)
            .ToArray();

        Assert.That(archives.Length, Is.EqualTo(2));

        Assert.That(
            File.ReadAllText(
                Path.Combine(_archiveDirectory, "application-0.log")),
            Is.EqualTo("First" + Environment.NewLine));

        Assert.That(
            File.ReadAllText(
                Path.Combine(_archiveDirectory, "application-1.log")),
            Is.EqualTo("Second" + Environment.NewLine));
    }

    [Test]
    public async Task ConcurrentWrites_PreserveAllMessages()
    {
        var configuration = CreateConfiguration();

        var manager = CreateManager(configuration, null);

        const int messageCount = 100;

        var messages = Enumerable
            .Range(0, messageCount)
            .Select(i => $"Message-{i}")
            .ToArray();

        await Task.WhenAll(
            messages.Select(
                message => Task.Run(() => manager.Write(message))));

        var content = File.ReadAllText(GetActiveFilePath());

        foreach (var message in messages)
        {
            Assert.That(
                content,
                Does.Contain(message + Environment.NewLine));
        }

        var lineCount = File.ReadAllLines(GetActiveFilePath()).Length;

        Assert.That(lineCount, Is.EqualTo(messageCount));
    }

    [Test]
    public void ConcurrentWrites_DoNotCreateMultipleActiveFiles()
    {
        var configuration = CreateConfiguration();

        var manager = CreateManager(configuration, null);

        Parallel.For(
            0,
            50,
            i => manager.Write($"Message-{i}"));

        var activeFiles = Directory
            .GetFiles(_activeDirectory)
            .Where(x => Path.GetFileName(x) == "application.log")
            .ToArray();

        Assert.That(activeFiles.Length, Is.EqualTo(1));
    }

    private FileLifecycleManager CreateManager(
        FileConfiguration configuration,
        IRollingStrategy? rollingStrategy = null)
    {
        return new FileLifecycleManager(
            configuration,
            rollingStrategy ?? _rollingStrategy.Object,
            _namingStrategy.Object);
    }

    private FileConfiguration CreateConfiguration()
    {
        return new FileConfiguration
        {
            Directory = _activeDirectory,
            Archive = new ArchiveConfiguration
            {
                Enabled = false,
                Directory = _archiveDirectory,
                Compress = false
            },
            Retention = new RetentionConfiguration
            {
                RetentionDays = 30
            }
        };
    }

    private string GetActiveFilePath()
    {
        return Path.Combine(
            _activeDirectory,
            "application.log");
    }
}
