using System;

namespace SmartLogger.Core
{
    /// <summary>
    /// Provides a centralized entry point for initializing,
    /// retrieving, and reloading SmartLogger instances.
    /// </summary>
    public static class LoggerManager
    {
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
        /// Thrown when the LoggerManager has not been initialized.
        /// </exception>
        public static ISmartLogger GetLogger(string name)
        {
            if (_factory is null)
                throw new InvalidOperationException("LoggerManager is not initialized.");

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
        /// Thrown when the LoggerManager has not been initialized.
        /// </exception>
        public static void Reload(ILogConfigurationProvider provider)
        {
            if (_factory is null)
                throw new InvalidOperationException("LoggerManager is not initialized.");

            LogConfigurationHolder newConfig = provider.Load();
            _factory.UpdateConfiguration(newConfig);
        }
    }
}