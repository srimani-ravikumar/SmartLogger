namespace SmartLogger.Core;

public interface ILogConfigurationProvider
{
    LogConfigurationHolder Load();
}

// TODO: Implementations:

// JsonConfigurationProvider

// EnvironmentConfigurationProvider

// InMemoryConfigurationProvider

// FluentConfigurationProvider