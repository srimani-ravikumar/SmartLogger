namespace SmartLogger.Core;

/// <summary>
/// Defines a contract for loading SmartLogger configuration
/// from any underlying source (JSON, memory, database, etc.).
/// </summary>
public interface ILogConfigurationProvider
{
    /// <summary>
    /// Loads and returns a validated <see cref="LogConfigurationHolder"/>.
    /// </summary>
    /// <returns>The loaded logging configuration.</returns>
    LogConfigurationHolder Load();
}