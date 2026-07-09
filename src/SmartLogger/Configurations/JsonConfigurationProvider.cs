using SmartLogger.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLogger.Configurations;

/// <summary>
/// Loads SmartLogger configuration from a JSON file.
/// </summary>
/// <remarks>
/// Features:
/// <list type="bullet">
/// <item><description>Flexible JSON parsing (case-insensitive, comments allowed)</description></item>
/// <item><description>Strong configuration validation (handled by the base class)</description></item>
/// <item><description>Optional automatic reload when the configuration file changes</description></item>
/// </list>
/// </remarks>
public sealed class JsonConfigurationProvider : FileConfigurationProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigurationProvider"/> class.
    /// </summary>
    /// <param name="filePath">
    /// Relative or absolute path to the JSON configuration file.
    /// </param>
    /// <param name="enableAutoReload">
    /// Indicates whether automatic reload on file changes is enabled.
    /// </param>
    public JsonConfigurationProvider(string filePath, bool enableAutoReload = false)
        : base(filePath, enableAutoReload)
    {
    }

    /// <summary>
    /// Deserializes the JSON configuration file into
    /// <see cref="LogConfigurationHolder"/>.
    /// </summary>
    /// <param name="filePath">
    /// Absolute path to the JSON configuration file.
    /// </param>
    /// <returns>
    /// A populated <see cref="LogConfigurationHolder"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configuration cannot be deserialized.
    /// </exception>
    protected override LogConfigurationHolder Deserialize(string filePath)
    {
        var json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(
                    namingPolicy: JsonNamingPolicy.CamelCase,
                    allowIntegerValues: false)
            }
        };

        var configuration = JsonSerializer.Deserialize<LogConfigurationHolder>(json, options);

        if (configuration is null)
        {
            throw new InvalidOperationException("Failed to deserialize SmartLogger configuration.");
        }

        return configuration;
    }
}