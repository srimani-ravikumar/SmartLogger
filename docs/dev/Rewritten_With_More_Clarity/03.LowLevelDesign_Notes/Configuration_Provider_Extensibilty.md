# Design Note: Configuration Provider Extensibility

## Problem Statement

SmartLogger currently provides a `JsonConfigurationProvider`. Future configuration providers such as XML, YAML, Environment Variables, Azure App Configuration, or a custom configuration source will require the same infrastructure for file handling, validation, and runtime configuration reload.

Duplicating this logic across providers would increase maintenance effort and introduce inconsistencies.

## Proposed Design

Extract the common responsibilities into a reusable abstract base class.

```text
                ILogConfigurationProvider
                         ▲
                         │
        FileConfigurationProviderBase
               ▲                  ▲
               │                  │
 JsonConfigurationProvider   XmlConfigurationProvider
```

The base class will be responsible for:

* Resolving relative and absolute file paths.
* Loading the configuration file.
* Runtime configuration reload using `FileSystemWatcher`.
* Synchronization during configuration reload.
* Invoking configuration validation.
* Reloading the active configuration through `LoggerManager`.
* Providing common exception handling and resilience.

Each provider will only be responsible for deserializing its own configuration format into `LogConfigurationHolder`.

## Benefits

* Eliminates duplicate implementation across configuration providers.
* Ensures consistent behavior regardless of the configuration format.
* Makes it easier to introduce new providers in the future.
* Improves maintainability by centralizing the shared runtime reload logic.
* Keeps each provider focused on a single responsibility.

## Design Principle

> **Common infrastructure should be implemented once. Each configuration provider should only be responsible for converting its input format into `LogConfigurationHolder`.**
