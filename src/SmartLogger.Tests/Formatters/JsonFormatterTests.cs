using SmartLogger.Core;
using System.Text.Json;

namespace SmartLogger.Tests.Formatters;

[TestFixture]
public class JsonFormatterTests
{
    #region Basic Serialization

    [Test]
    public void Format_WithDefaultConfiguration_ShouldSerializeAllFields()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("timestamp", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("level", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("source", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("thread", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("correlation", out _), Is.True);
    }

    [Test]
    public void Format_WithLogLevel_ShouldSerializeLevelAsString()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.GetProperty("level").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(
            json.RootElement.GetProperty("level").GetString(),
            Is.EqualTo(message.LogLevel.ToString()));
    }

    [Test]
    public void Format_WithMessage_ShouldSerializeMessageCorrectly()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("message").GetString(),
            Is.EqualTo(message.Message));
    }

    [Test]
    public void Format_WithSource_ShouldSerializeSourceCorrectly()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("source").GetString(),
            Is.EqualTo(message.Source));
    }

    [Test]
    public void Format_WithThreadId_ShouldSerializeThreadCorrectly()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("thread").GetInt32(),
            Is.EqualTo(message.ThreadId));
    }

    [Test]
    public void Format_WithCorrelationId_ShouldSerializeCorrelationCorrectly()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("correlation").GetString(),
            Is.EqualTo(message.CorrelationId));
    }

    [Test]
    public void Format_WithTimestamp_ShouldSerializeTimestampCorrectly()
    {
        // Arrange
        var message = CreateMessage();
        var formatter = CreateFormatter();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        var serializedTimestamp =
            json.RootElement.GetProperty("timestamp").GetDateTime();

        Assert.That(serializedTimestamp, Is.EqualTo(message.Timestamp));
    }

    #endregion

    #region JSON Output Validity

    [Test]
    public void Format_WithValidMessage_ShouldReturnValidJson()
    {
        // Arrange
        var formatter = CreateFormatter();
        var message = CreateMessage();

        // Act
        var output = formatter.Format(message);

        // Assert
        Assert.DoesNotThrow(() =>
        {
            using var document = JsonDocument.Parse(output);
        });
    }

    [Test]
    public void Format_WithValidMessage_ShouldReturnJsonObject()
    {
        // Arrange
        var formatter = CreateFormatter();
        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
    }

    [Test]
    public void Format_WithSpecialCharacters_ShouldProduceValidJson()
    {
        // Arrange
        var formatter = CreateFormatter();
        var message = CreateMessage(
            messageText: "Message with \"quotes\", slash \\, newline\nand\ttab.");

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("message").GetString(),
            Is.EqualTo(message.Message));
    }

    [Test]
    public void Format_WithUnicodeContent_ShouldPreserveUnicodeCharacters()
    {
        // Arrange
        var formatter = CreateFormatter();
        var message = CreateMessage(
            messageText: "தமிழ் 日本語 Ελληνικά 🚀");

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("message").GetString(),
            Is.EqualTo(message.Message));
    }

    #endregion

    #region Field Filtering

    [Test]
    public void Format_WithIncludedFields_ShouldSerializeOnlyConfiguredFields()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "message", "level" });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("level", out _), Is.True);

        Assert.That(json.RootElement.TryGetProperty("timestamp", out _), Is.False);
        Assert.That(json.RootElement.TryGetProperty("source", out _), Is.False);
        Assert.That(json.RootElement.TryGetProperty("thread", out _), Is.False);
        Assert.That(json.RootElement.TryGetProperty("correlation", out _), Is.False);
    }

    [Test]
    public void Format_WithSingleIncludedField_ShouldSerializeOnlyThatField()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "message" });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
        Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo(message.Message));
    }

    [Test]
    public void Format_WithMultipleIncludedFields_ShouldSerializeConfiguredFields()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "message", "source", "level" });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("source", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("level", out _), Is.True);
    }

    [Test]
    public void Format_WithUnknownIncludedField_ShouldReturnEmptyJsonObject()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "unknown" });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(0));
    }

    [Test]
    public void Format_WithEmptyIncludedFields_ShouldIncludeAllFields()
    {
        // Arrange
        var formatter = CreateFormatter(fields: new List<string>());
        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(6));
    }

    [Test]
    public void Format_WithNullIncludedFields_ShouldIncludeAllFields()
    {
        // Arrange
        var formatter = CreateFormatter(fields: null);
        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(6));
    }

    [Test]
    public void Format_WithDuplicateIncludedFields_ShouldSerializeEachFieldOnce()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string>
            {
                "message",
                "message",
                "level",
                "level"
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
    }

    #endregion

    #region Field Mapping

    [Test]
    public void Format_WithFieldMapping_ShouldUseMappedFieldName()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new()
                {
                    SourceField = "timestamp",
                    TargetField = "ts"
                }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("ts", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("timestamp", out _), Is.False);
    }

    [Test]
    public void Format_WithMultipleFieldMappings_ShouldMapAllConfiguredFields()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new() { SourceField = "timestamp", TargetField = "ts" },
                new() { SourceField = "correlation", TargetField = "cid" },
                new() { SourceField = "message", TargetField = "msg" }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("ts", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("cid", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("msg", out _), Is.True);
    }

    [Test]
    public void Format_WithPartialFieldMapping_ShouldPreserveUnmappedFields()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new()
                {
                    SourceField = "message",
                    TargetField = "msg"
                }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("msg", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("source", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("level", out _), Is.True);
    }

    [Test]
    public void Format_WithFilteringAndMapping_ShouldFilterThenMapFields()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "message", "level" },
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new() { SourceField = "message", TargetField = "msg" },
                new() { SourceField = "level", TargetField = "severity" }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
        Assert.That(json.RootElement.TryGetProperty("msg", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("severity", out _), Is.True);

        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.False);
        Assert.That(json.RootElement.TryGetProperty("level", out _), Is.False);
        Assert.That(json.RootElement.TryGetProperty("source", out _), Is.False);
    }

    [Test]
    public void Format_WithEmptyMappings_ShouldUseOriginalFieldNames()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>());

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
    }

    [Test]
    public void Format_WithNullMappings_ShouldUseOriginalFieldNames()
    {
        // Arrange
        var formatter = CreateFormatter(mappings: null);
        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
    }

    #endregion

    #region Mapping Validation

    [Test]
    public void Constructor_WithDuplicateSourceMappings_ShouldThrowArgumentException()
    {
        // Arrange
        var mappings = new List<JsonFieldMappingConfiguration>
        {
            new() { SourceField = "message", TargetField = "msg" },
            new() { SourceField = "message", TargetField = "text" }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateFormatter(mappings: mappings));
    }

    [Test]
    public void Constructor_WithMultipleSourcesMappedToSameTarget_ShouldCreateFormatter()
    {
        // Arrange
        var mappings = new List<JsonFieldMappingConfiguration>
        {
            new() { SourceField = "message", TargetField = "value" },
            new() { SourceField = "source", TargetField = "value" }
        };

        // Act
        var formatter = CreateFormatter(mappings: mappings);

        // Assert
        Assert.That(formatter, Is.Not.Null);
    }

    [Test]
    public void Format_WithEmptyTargetField_ShouldUseEmptyJsonPropertyName()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new()
                {
                    SourceField = "message",
                    TargetField = string.Empty
                }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty(string.Empty, out _), Is.True);
    }

    #endregion

    #region Null Values

    [Test]
    public void Format_WithNullOptionalFields_ShouldSerializeNullValues()
    {
        // Arrange
        var formatter = CreateFormatter();

        var message = CreateMessage(
            source: null,
            correlationId: null);

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(
            json.RootElement.GetProperty("source").ValueKind,
            Is.EqualTo(JsonValueKind.Null));

        Assert.That(
            json.RootElement.GetProperty("correlation").ValueKind,
            Is.EqualTo(JsonValueKind.Null));
    }

    [Test]
    public void Format_WithNullMessage_ShouldThrowNullReferenceException()
    {
        // Arrange
        var formatter = CreateFormatter();

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => formatter.Format(null!));
    }

    #endregion

    #region Pretty Print

    [Test]
    public void Format_WithDefaultPrettyPrint_ShouldReturnCompactJson()
    {
        // Arrange
        var formatter = CreateFormatter();
        var message = CreateMessage();

        // Act
        var output = formatter.Format(message);

        // Assert
        Assert.That(output, Does.Not.Contain(Environment.NewLine));
    }

    [Test]
    public void Format_WithPrettyPrintEnabled_ShouldReturnIndentedJson()
    {
        // Arrange
        var formatter = new JsonFormatter(
            fields: null,
            mappings: null,
            prettyPrint: true);

        var message = CreateMessage();

        // Act
        var output = formatter.Format(message);

        // Assert
        Assert.That(output, Does.Contain(Environment.NewLine));
    }

    [Test]
    public void Format_WithPrettyPrintEnabled_ShouldPreserveJsonContent()
    {
        // Arrange
        var compactFormatter = CreateFormatter();
        var prettyFormatter = new JsonFormatter(
            fields: null,
            mappings: null,
            prettyPrint: true);

        var message = CreateMessage();

        // Act
        using var compactJson = ParseJson(compactFormatter.Format(message));
        using var prettyJson = ParseJson(prettyFormatter.Format(message));

        // Assert
        Assert.That(
            prettyJson.RootElement.GetProperty("message").GetString(),
            Is.EqualTo(compactJson.RootElement.GetProperty("message").GetString()));

        Assert.That(
            prettyJson.RootElement.GetProperty("source").GetString(),
            Is.EqualTo(compactJson.RootElement.GetProperty("source").GetString()));
    }

    #endregion

    #region Case Sensitivity

    [Test]
    public void Format_WithIncorrectFieldNameCasing_ShouldExcludeField()
    {
        // Arrange
        var formatter = CreateFormatter(
            fields: new List<string> { "Message" });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(0));
    }

    [Test]
    public void Format_WithIncorrectMappingSourceCasing_ShouldNotApplyMapping()
    {
        // Arrange
        var formatter = CreateFormatter(
            mappings: new List<JsonFieldMappingConfiguration>
            {
                new()
                {
                    SourceField = "Message",
                    TargetField = "msg"
                }
            });

        var message = CreateMessage();

        // Act
        using var json = ParseJson(formatter.Format(message));

        // Assert
        Assert.That(json.RootElement.TryGetProperty("message", out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("msg", out _), Is.False);
    }

    #endregion

    #region Configuration Independence

    [Test]
    public void Constructor_ShouldNotModifyIncludedFields()
    {
        // Arrange
        var fields = new List<string>
        {
            "message",
            "level",
            "message"
        };

        var original = new List<string>(fields);

        // Act
        _ = CreateFormatter(fields: fields);

        // Assert
        Assert.That(fields, Is.EqualTo(original));
    }

    [Test]
    public void Constructor_ShouldNotModifyMappings()
    {
        // Arrange
        var mappings = new List<JsonFieldMappingConfiguration>
        {
            new()
            {
                SourceField = "message",
                TargetField = "msg"
            }
        };

        var originalSource = mappings[0].SourceField;
        var originalTarget = mappings[0].TargetField;

        // Act
        _ = CreateFormatter(mappings: mappings);

        // Assert
        Assert.That(mappings[0].SourceField, Is.EqualTo(originalSource));
        Assert.That(mappings[0].TargetField, Is.EqualTo(originalTarget));
    }

    [Test]
    public void MultipleFormatterInstances_ShouldMaintainIndependentConfiguration()
    {
        // Arrange
        var message = CreateMessage();

        var messageFormatter = CreateFormatter(
            fields: new List<string> { "message" });

        var sourceFormatter = CreateFormatter(
            fields: new List<string> { "source" });

        // Act
        using var messageJson = ParseJson(messageFormatter.Format(message));
        using var sourceJson = ParseJson(sourceFormatter.Format(message));

        // Assert
        Assert.That(messageJson.RootElement.TryGetProperty("message", out _), Is.True);
        Assert.That(messageJson.RootElement.TryGetProperty("source", out _), Is.False);

        Assert.That(sourceJson.RootElement.TryGetProperty("source", out _), Is.True);
        Assert.That(sourceJson.RootElement.TryGetProperty("message", out _), Is.False);
    }

    #endregion

    #region Test Helpers

    private static JsonFormatter CreateFormatter(
        List<string>? fields = null,
        List<JsonFieldMappingConfiguration>? mappings = null)
    {
        return new JsonFormatter(fields, mappings);
    }

    private static JsonDocument ParseJson(string output)
    {
        return JsonDocument.Parse(output);
    }

    private static LogMessage CreateMessage(
    string messageText = "Test message",
    string source = "TestSource",
    string correlationId = "correlation-123")
    {
        return new LogMessage.Builder()
            .WithLevel(LogLevel.INFO)
            .WithMessage(messageText)
            .FromSource(source)
            .WithCorrelationId(correlationId)
            .Build();
    }

    #endregion
}