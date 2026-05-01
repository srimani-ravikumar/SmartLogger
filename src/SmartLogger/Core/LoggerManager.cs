using System;

namespace SmartLogger.Core;

/// <summary>
/// Provides a centralized entry point for initializing,
/// retrieving, and reloading SmartLogger instances.
/// </summary>
public static class LoggerManager
{
    // Static factory instance to manage logger creation and configuration.
    private static LoggerFactory _factory;

    /// <summary>
    /// Initializes the logging system using the specified configuration provider.
    /// Must be called before retrieving any logger instances.
    /// </summary>
    /// <param name="provider">
    /// The configuration provider responsible for supplying logging settings.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provider is null.
    /// </exception>
    public static void Initialize(ILogConfigurationProvider provider)
    {
        _factory = new LoggerFactory(provider);
    }

    /// <summary>
    /// Retrieves a configured logger instance with the specified name.
    /// </summary>
    /// <param name="name">
    /// The name of the logger (typically class or namespace).
    /// </param>
    /// <returns>A configured <see cref="ISmartLogger"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the LoggerFactory has not been initialized.
    /// </exception>
    public static ISmartLogger GetLogger(string name)
    {
        if (_factory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        return _factory.GetOrCreateLogger(name);
    }

    /// <summary>
    /// Retrieves a configured logger instance associated with the specified type.
    /// The logger name is derived from the fully qualified type name (Namespace.ClassName).
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> whose fully qualified name will be used as the logger name.
    /// Typically, this represents the calling class.
    /// </param>
    /// <returns>
    /// A configured <see cref="ISmartLogger"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="type"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the LoggerFactory has not been initialized.
    /// </exception>
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
    /// Newly created loggers will use the updated configuration.
    /// </summary>
    /// <param name="provider">
    /// The configuration provider used to reload settings.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the LoggerFactory has not been initialized.
    /// </exception>
    public static void ReloadConfiguration(ILogConfigurationProvider provider)
    {
        if (_factory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        LogConfigurationHolder newConfig = provider.Load();
        _factory.UpdateConfiguration(newConfig);
    }
}