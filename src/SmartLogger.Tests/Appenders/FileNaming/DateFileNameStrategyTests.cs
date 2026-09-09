using SmartLogger.Appenders.FileNaming;
using SmartLogger.Core;

namespace SmartLogger.Tests.Appenders.FileNaming;

[TestFixture]
public class DateFileNamingStrategyTests
{
    // -------------------------------------------------------------------------
    // Constructor Tests
    // -------------------------------------------------------------------------

    [Test]
    public void Constructor_WithValidConfiguration_ShouldCreateStrategy()
    {
        var configuration = CreateConfiguration();

        Assert.DoesNotThrow(() =>
        {
            _ = new DateFileNamingStrategy(configuration);
        });
    }

    [Test]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DateFileNamingStrategy(null!));
    }

    // -------------------------------------------------------------------------
    // Active File Name Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateActiveFileName_WithValidConfiguration_ShouldReturnExpectedFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(result, Is.EqualTo("Application.log"));
    }

    [Test]
    public void CreateActiveFileName_WithCustomFileName_ShouldUseConfiguredFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "MyApplication",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(result, Is.EqualTo("MyApplication.log"));
    }

    [Test]
    public void CreateActiveFileName_WithCustomExtension_ShouldUseConfiguredExtension()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "txt");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(result, Is.EqualTo("Application.txt"));
    }

    [Test]
    public void CreateActiveFileName_WithExtensionWithoutDot_ShouldReturnExpectedFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "json");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(result, Is.EqualTo("Application.json"));
    }

    [Test]
    public void CreateActiveFileName_WithDefaultConfiguration_ShouldReturnDefaultFileName()
    {
        var configuration = new FileConfiguration();

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(result, Is.EqualTo("Application.log"));
    }

    // -------------------------------------------------------------------------
    // Rolled File Name Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateRolledFileName_WithDefaultIndex_ShouldIncludeCurrentDate()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var before = DateTime.Now;
        var result = strategy.CreateRolledFileName();
        var after = DateTime.Now;

        var possibleResults = new[]
        {
            $"Application_{before.ToString("yyyy-MM-dd")}.log",
            $"Application_{after.ToString("yyyy-MM-dd")}.log"
        };

        Assert.That(result, Is.AnyOf(possibleResults));
    }

    [Test]
    public void CreateRolledFileName_WithZeroIndex_ShouldNotIncludeIndex()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        var result = strategy.CreateRolledFileName(0);

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}.log"));
    }

    [Test]
    public void CreateRolledFileName_WithPositiveIndex_ShouldIncludeIndex()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        var result = strategy.CreateRolledFileName(1);

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}_1.log"));
    }

    [Test]
    public void CreateRolledFileName_WithMultipleIndexes_ShouldIncludeExactIndex()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        Assert.Multiple(() =>
        {
            Assert.That(
                strategy.CreateRolledFileName(1),
                Is.EqualTo($"Application_{date}_1.log"));

            Assert.That(
                strategy.CreateRolledFileName(2),
                Is.EqualTo($"Application_{date}_2.log"));

            Assert.That(
                strategy.CreateRolledFileName(10),
                Is.EqualTo($"Application_{date}_10.log"));

            Assert.That(
                strategy.CreateRolledFileName(100),
                Is.EqualTo($"Application_{date}_100.log"));
        });
    }

    [Test]
    public void CreateRolledFileName_CalledMultipleTimesWithinSameDate_ShouldReturnSameDateComponent()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var firstResult = strategy.CreateRolledFileName();
        var secondResult = strategy.CreateRolledFileName();

        Assert.That(secondResult, Is.EqualTo(firstResult));
    }

    // -------------------------------------------------------------------------
    // Date Format Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateRolledFileName_WithCustomDateFormat_ShouldUseConfiguredFormat()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyyMMdd");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString("yyyyMMdd");

        var result = strategy.CreateRolledFileName();

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}.log"));
    }

    [Test]
    public void CreateRolledFileName_WithDateFormatContainingSeparators_ShouldPreserveFormat()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        var result = strategy.CreateRolledFileName();

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}.log"));
    }

    [Test]
    public void CreateRolledFileName_WithDateAndTimeFormat_ShouldUseConfiguredFormat()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy-MM-dd_HH-mm");

        var strategy = new DateFileNamingStrategy(configuration);

        var before = DateTime.Now;
        var result = strategy.CreateRolledFileName();
        var after = DateTime.Now;

        var expectedBefore =
            $"Application_{before.ToString("yyyy-MM-dd_HH-mm")}.log";

        var expectedAfter =
            $"Application_{after.ToString("yyyy-MM-dd_HH-mm")}.log";

        Assert.That(
            result,
            Is.AnyOf(expectedBefore, expectedAfter));
    }

    [Test]
    public void CreateRolledFileName_WithLiteralDateFormatText_ShouldPreserveLiteralText()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyy'year'MM'month'dd'day'");

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString(
            "yyyy'year'MM'month'dd'day'");

        var result = strategy.CreateRolledFileName();

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}.log"));
    }

    // -------------------------------------------------------------------------
    // Configuration Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateActiveFileName_AfterConfigurationChange_ShouldUseUpdatedValues()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        configuration.FileName = "UpdatedApplication";
        configuration.Extension = "txt";

        var result = strategy.CreateActiveFileName();

        Assert.That(
            result,
            Is.EqualTo("UpdatedApplication.txt"));
    }

    [Test]
    public void CreateRolledFileName_AfterConfigurationChange_ShouldUseUpdatedValues()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "log",
            dateFormat: "yyyyMMdd");

        var strategy = new DateFileNamingStrategy(configuration);

        configuration.FileName = "UpdatedApplication";
        configuration.Extension = "txt";
        configuration.Naming.DateFormat = "yyyy-MM-dd";

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        var result = strategy.CreateRolledFileName(2);

        Assert.That(
            result,
            Is.EqualTo($"UpdatedApplication_{date}_2.txt"));
    }

    [Test]
    public void CreateActiveFileName_AndCreateRolledFileName_ShouldUseConfiguredFileNameAndExtension()
    {
        var configuration = CreateConfiguration(
            fileName: "Service",
            extension: "json",
            dateFormat: "yyyy-MM-dd");

        var strategy = new DateFileNamingStrategy(configuration);

        var activeResult = strategy.CreateActiveFileName();
        var rolledResult = strategy.CreateRolledFileName();

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        Assert.Multiple(() =>
        {
            Assert.That(
                activeResult,
                Is.EqualTo("Service.json"));

            Assert.That(
                rolledResult,
                Is.EqualTo($"Service_{date}.json"));
        });
    }

    // -------------------------------------------------------------------------
    // Rolling Index Boundary Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateRolledFileName_WithIndexOne_ShouldAppendOne()
    {
        var configuration = CreateConfiguration();

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString(configuration.Naming.DateFormat);

        var result = strategy.CreateRolledFileName(1);

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}_1.log"));
    }

    [Test]
    public void CreateRolledFileName_WithLargePositiveIndex_ShouldReturnExpectedFileName()
    {
        var configuration = CreateConfiguration();

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString(configuration.Naming.DateFormat);

        var result = strategy.CreateRolledFileName(int.MaxValue);

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}_{int.MaxValue}.log"));
    }

    [Test]
    public void CreateRolledFileName_WithNegativeIndex_ShouldNotIncludeIndex()
    {
        var configuration = CreateConfiguration();

        var strategy = new DateFileNamingStrategy(configuration);

        var date = DateTime.Now.ToString(configuration.Naming.DateFormat);

        var result = strategy.CreateRolledFileName(-1);

        Assert.That(
            result,
            Is.EqualTo($"Application_{date}.log"));
    }

    // -------------------------------------------------------------------------
    // Special Configuration Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateActiveFileName_WithFileNameContainingSpaces_ShouldPreserveFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "My Application",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(
            result,
            Is.EqualTo("My Application.log"));
    }

    [Test]
    public void CreateActiveFileName_WithSpecialCharacters_ShouldPreserveFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "My-App_01",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(
            result,
            Is.EqualTo("My-App_01.log"));
    }

    [Test]
    public void CreateActiveFileName_WithUnicodeFileName_ShouldPreserveFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "Applicationதமிழ்",
            extension: "log");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(
            result,
            Is.EqualTo("Applicationதமிழ்.log"));
    }

    [Test]
    public void CreateActiveFileName_WithJsonExtension_ShouldReturnJsonFileName()
    {
        var configuration = CreateConfiguration(
            fileName: "Application",
            extension: "json");

        var strategy = new DateFileNamingStrategy(configuration);

        var result = strategy.CreateActiveFileName();

        Assert.That(
            result,
            Is.EqualTo("Application.json"));
    }

    // -------------------------------------------------------------------------
    // Invalid Configuration Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CreateRolledFileName_WithInvalidDateFormat_ShouldPropagateFormatException()
    {
        var configuration = CreateConfiguration(
            dateFormat: "invalid format [");

        var strategy = new DateFileNamingStrategy(configuration);

        Assert.Throws<FormatException>(() =>
            strategy.CreateRolledFileName());
    }

    [Test]
    public void CreateRolledFileName_WithNullNamingConfiguration_ShouldThrowNullReferenceException()
    {
        var configuration = CreateConfiguration();

        configuration.Naming = null!;

        var strategy = new DateFileNamingStrategy(configuration);

        Assert.Throws<NullReferenceException>(() =>
            strategy.CreateRolledFileName());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static FileConfiguration CreateConfiguration(
        string fileName = "Application",
        string extension = "log",
        string dateFormat = "yyyy-MM-dd")
    {
        return new FileConfiguration
        {
            FileName = fileName,
            Extension = extension,
            Naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Date,
                DateFormat = dateFormat
            }
        };
    }
}