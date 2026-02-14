using SmartLogger.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLogger.Configurations;

public class JsonConfigurationProvider : ILogConfigurationProvider
{
    private readonly string _filePath;

    public JsonConfigurationProvider(string filePath)
    {
        if(string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath)); 
        }

        if (Path.IsPathRooted(filePath))
            _filePath = filePath;
        else
            _filePath = Path.Combine(AppContext.BaseDirectory, filePath);

    }

    public LogConfigurationHolder Load()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"SmartLogger configuration file not found at path: {_filePath}");
        }

        var json = File.ReadAllText(_filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
            }
        };

        var configuration = JsonSerializer.Deserialize<LogConfigurationHolder>(json, options);

        if (configuration is null)
        {
            throw new InvalidOperationException(
                "Failed to deserialize SmartLogger configuration.");
        }

        Validate(configuration);

        return configuration;
    }

    private static void Validate(LogConfigurationHolder config)
    {
        if (config.Appenders is null || !config.Appenders.Any())
        {
            throw new InvalidOperationException(
                "At least one appender must be configured.");
        }

        foreach (var appender in config.Appenders)
        {
            if (string.IsNullOrWhiteSpace(appender.Type))
            {
                throw new InvalidOperationException(
                    "Appender type is required.");
            }
        }
    }
}
