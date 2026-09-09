using SmartLogger.Core;
using SmartLogger.Formatters;

namespace SmartLogger.Tests.Formatters;

[TestFixture]
public class FormatterFactoryTests
{
    #region PlainText Formatter

    [Test]
    public void Create_WithPlainTextFormat_ShouldReturnPlainTextFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<PlainTextFormatter>());
    }

    [Test]
    public void Create_WithPlainTextAndSimpleLayout_ShouldCreateFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText,
            LogMessageLayoutType.Simple);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<PlainTextFormatter>());
    }

    [Test]
    public void Create_WithPlainTextAndDetailedLayout_ShouldCreateFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText,
            LogMessageLayoutType.Detailed);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<PlainTextFormatter>());
    }

    [Test]
    public void Create_WithPlainTextAndCustomLayout_ShouldCreateFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText,
            LogMessageLayoutType.Custom);

        configuration.Formatter.Pattern =
            "[%LEVEL] %MESSAGE";

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<PlainTextFormatter>());
    }

    #endregion

    #region JSON Formatter

    [Test]
    public void Create_WithJsonFormat_ShouldReturnJsonFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Json);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<JsonFormatter>());
    }

    [Test]
    public void Create_WithJsonFormatAndIncludedFields_ShouldCreateFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Json);

        configuration.Formatter.IncludedJsonFields =
            new()
            {
                "timestamp",
                "level",
                "message"
            };

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<JsonFormatter>());
    }

    [Test]
    public void Create_WithJsonFormatAndFieldMappings_ShouldCreateFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Json);

        configuration.Formatter.JsonFieldMappings =
            new()
            {
                new JsonFieldMappingConfiguration
                {
                    SourceField = "timestamp",
                    TargetField = "ts"
                }
            };

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<JsonFormatter>());
    }

    [Test]
    public void Create_WithJsonFormatAndDefaultConfiguration_ShouldReturnJsonFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Json);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<JsonFormatter>());
    }

    #endregion

    #region XML Formatter

    [Test]
    public void Create_WithXmlFormat_ShouldReturnXmlFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Xml);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<XmlFormatter>());
    }

    [Test]
    public void Create_WithXmlFormat_ShouldReturnFormatterRegardlessOfLayoutType()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Xml,
            LogMessageLayoutType.Detailed);

        // Act
        var result = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(result, Is.TypeOf<XmlFormatter>());
    }

    #endregion

    #region Unsupported Output Format

    [Test]
    public void Create_WithUnsupportedOutputFormat_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = CreateConfiguration(
            (LogOutputFormat)999);

        // Act
        var exception = Assert.Throws<NotSupportedException>(
            () => FormatterFactory.Create(configuration));

        // Assert
        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Create_WithUnsupportedOutputFormat_ShouldIncludeFormatInExceptionMessage()
    {
        // Arrange
        const LogOutputFormat unsupportedFormat = (LogOutputFormat)999;

        var configuration = CreateConfiguration(
            unsupportedFormat);

        // Act
        var exception = Assert.Throws<NotSupportedException>(
            () => FormatterFactory.Create(configuration));

        // Assert
        Assert.That(
            exception!.Message,
            Does.Contain(unsupportedFormat.ToString()));
    }

    #endregion

    #region Unsupported Layout

    [Test]
    public void Create_WithUnsupportedLayoutType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText,
            (LogMessageLayoutType)999);

        // Act & Assert
        Assert.Throws<NotSupportedException>(
            () => FormatterFactory.Create(configuration));
    }

    [Test]
    public void Create_WithUnsupportedLayoutType_ShouldNotReturnFormatter()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.PlainText,
            (LogMessageLayoutType)999);

        // Act
        var exception = Assert.Throws<NotSupportedException>(
            () => FormatterFactory.Create(configuration));

        // Assert
        Assert.That(exception, Is.Not.Null);
    }

    #endregion

    #region Null Configuration

    [Test]
    public void Create_WithNullAppenderConfiguration_ShouldThrowNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => FormatterFactory.Create(null!));
    }

    [Test]
    public void Create_WithNullFormatterConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var configuration = new AppenderConfiguration
        {
            Formatter = null!
        };

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => FormatterFactory.Create(configuration));
    }

    #endregion

    #region Instance Independence

    [Test]
    public void Create_CalledMultipleTimes_ShouldReturnDifferentFormatterInstances()
    {
        // Arrange
        var configuration = CreateConfiguration(
            LogOutputFormat.Json);

        // Act
        var first = FormatterFactory.Create(configuration);
        var second = FormatterFactory.Create(configuration);

        // Assert
        Assert.That(first, Is.Not.SameAs(second));
    }

    [Test]
    public void Create_WithDifferentOutputFormats_ShouldCreateCorrespondingFormatterTypes()
    {
        // Arrange
        var plainTextConfiguration =
            CreateConfiguration(LogOutputFormat.PlainText);

        var jsonConfiguration =
            CreateConfiguration(LogOutputFormat.Json);

        var xmlConfiguration =
            CreateConfiguration(LogOutputFormat.Xml);

        // Act
        var plainTextFormatter =
            FormatterFactory.Create(plainTextConfiguration);

        var jsonFormatter =
            FormatterFactory.Create(jsonConfiguration);

        var xmlFormatter =
            FormatterFactory.Create(xmlConfiguration);

        // Assert
        Assert.That(plainTextFormatter, Is.TypeOf<PlainTextFormatter>());
        Assert.That(jsonFormatter, Is.TypeOf<JsonFormatter>());
        Assert.That(xmlFormatter, Is.TypeOf<XmlFormatter>());
    }

    #endregion

    #region Test Helpers

    private static AppenderConfiguration CreateConfiguration(
        LogOutputFormat outputFormat,
        LogMessageLayoutType layoutType = LogMessageLayoutType.Simple)
    {
        return new AppenderConfiguration
        {
            Formatter = new FormatterConfiguration
            {
                OutputFormat = outputFormat,
                LayoutType = layoutType
            }
        };
    }

    #endregion
}