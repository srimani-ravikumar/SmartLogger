using System;

namespace SmartLogger.Core;

/// <summary>
/// Provides a centralized entry point for initializing,
/// retrieving, and reloading <see cref="ISmartLogger"/> instances.
/// </summary>
/// <remarks>
/// Acts as the global access layer for the logging system:
/// <list type="bullet">
/// <item><description>Initializes the logging infrastructure</description></item>
/// <item><description>Provides logger instances on demand</description></item>
/// <item><description>Supports runtime configuration updates</description></item>
/// </list>
/// 
/// <para>
/// This class maintains a single <see cref="LoggerFactory"/> instance
/// responsible for logger lifecycle management.
/// </para>
/// </remarks>
public static class LoggerManager
{
    /// <summary>
    /// Internal factory responsible for logger creation and configuration.
    /// </summary>
    private static LoggerFactory _factory;

    /// <summary>
    /// Initializes the logging system using the specified configuration provider.
    /// </summary>
    /// <param name="provider">
    /// The configuration provider responsible for supplying logging settings.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> is null.
    /// </exception>
    /// <remarks>
    /// Must be called before any logger is requested.
    /// Subsequent calls will overwrite the existing factory.
    /// </remarks>
    public static void Initialize(ILogConfigurationProvider provider)
    {
        _factory = new LoggerFactory(provider ?? throw new ArgumentNullException(nameof(provider)));
    }

    /// <summary>
    /// Retrieves a configured logger instance with the specified name.
    /// </summary>
    /// <param name="name">
    /// The logical name of the logger (typically class or namespace).
    /// </param>
    /// <returns>A configured <see cref="ISmartLogger"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the logging system has not been initialized.
    /// </exception>
    public static ISmartLogger GetLogger(string name)
    {
        if (_factory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        return _factory.GetOrCreateLogger(name);
    }

    /// <summary>
    /// Retrieves a configured logger instance associated with the specified type.
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> whose fully qualified name will be used as the logger name.
    /// </param>
    /// <returns>A configured <see cref="ISmartLogger"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="type"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the logging system has not been initialized.
    /// </exception>
    /// <remarks>
    /// Uses <c>Type.FullName</c> when available; otherwise falls back to <c>Type.Name</c>.
    /// </remarks>
    public static ISmartLogger GetLogger(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (_factory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        var name = type.FullName ?? type.Name;
        return _factory.GetOrCreateLogger(name);
    }

    /// <summary>
    /// Reloads the logging configuration using the specified provider.
    /// </summary>
    /// <param name="provider">
    /// The configuration provider used to reload settings.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the logging system has not been initialized.
    /// </exception>
    /// <remarks>
    /// Applies the new configuration to:
    /// <list type="bullet">
    /// <item><description>All newly created loggers</description></item>
    /// <item><description>Existing loggers via soft update</description></item>
    /// </list>
    /// </remarks>
    public static void ReloadConfiguration(ILogConfigurationProvider provider)
    {
        if (_factory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        var newConfig = provider.Load();
        _factory.UpdateConfiguration(newConfig);
    }
}