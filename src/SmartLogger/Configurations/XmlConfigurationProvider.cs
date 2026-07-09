using SmartLogger.Core;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SmartLogger.Configurations;

/// <summary>
/// Loads SmartLogger configuration from an XML file.
/// </summary>
/// <remarks>
/// Features:
/// <list type="bullet">
/// <item><description>Strongly typed XML deserialization</description></item>
/// <item><description>Strong configuration validation (handled by the base class)</description></item>
/// <item><description>Optional automatic reload when the configuration file changes</description></item>
/// </list>
/// </remarks>
public sealed class XmlConfigurationProvider : FileConfigurationProviderBase
{
    /// <summary>
    /// XML serializer reused across all loads.
    /// </summary>
    private static readonly XmlSerializer XmlSerializer = new(typeof(LogConfigurationHolder));

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlConfigurationProvider"/> class.
    /// </summary>
    /// <param name="filePath">
    /// Relative or absolute path to the XML configuration file.
    /// </param>
    /// <param name="enableAutoReload">
    /// Indicates whether automatic reload on file changes is enabled.
    /// </param>
    public XmlConfigurationProvider(string filePath, bool enableAutoReload = false)
        : base(filePath, enableAutoReload)
    {
    }

    /// <summary>
    /// Deserializes the XML configuration file into <see cref="LogConfigurationHolder"/>.
    /// </summary>
    /// <param name="filePath">
    /// Absolute path to the XML configuration file.
    /// </param>
    /// <returns>
    /// A populated <see cref="LogConfigurationHolder"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configuration cannot be deserialized.
    /// </exception>
    protected override LogConfigurationHolder Deserialize(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        if (XmlSerializer.Deserialize(stream) is not LogConfigurationHolder configuration)
        {
            throw new InvalidOperationException(
                "Failed to deserialize SmartLogger configuration.");
        }

        return configuration;
    }
}