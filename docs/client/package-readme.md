# SmartLogger - Build observability with less complexity

> ***Primary design goal is to come up with lightweight, simple and robust logging framework for high performance systems.***

SmartLogger is an extensible logging framework designed to provide structured, reliable, and configurable logging for modern .NET applications.

## Design Philosophy

SmartLogger is built with this principle:

> Logging must never compromise application stability.

It prioritizes:

* Safe execution
* Clear configuration
* Predictable behavior
* Extensibility

---

## Key Features

It is built to support:

* Log Levels with Priority System
* Customizable message structure
* Runtime configuration updates
* Multiple output destinations (Console, FileSystem, Database etc.)
* Multiple output format (plain text, Json etc.)
* Multi-threaded environments
* Correlation-based request tracing
* Resilient logging under load

---

## Why Use SmartLogger?

SmartLogger helps teams:

* Maintain consistent logging standards
* Trace requests across services using correlation IDs
* Dynamically update logging rules without restarting applications
* Protect applications during high log volume scenarios
* Detect logging pipeline failures early

It is designed to be **simple to integrate** and **safe & lightweight to use in production systems.**

---

## Quick Start

### 1️⃣ Install via NuGet

```powershell
Install-Package SmartLogger
```

---

### 2️⃣ Initialize Logger

```csharp
ILogConfigurationProvider provider = new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true);

LoggerManager.Initialize(provider);
```
or 

```charp
    var config = new LogConfigurationHolder
    {
        RootLogLevel = LogLevel.INFO,
        Appenders = new List<AppenderConfiguration>
    {
        new AppenderConfiguration
        {
            Destination = LogOutputDestination.Console,
            Threshold = LogLevel.DEBUG
        },
        new AppenderConfiguration
        {
            Destination = LogOutputDestination.FileSystem,
            Threshold = LogLevel.INFO,
            File = new FileConfiguration
            {
                BasePath = "logs/app",
                Extension = "log",
                Naming = new FileNamingConfiguration
                {
                    DateFormat = "yyyy-MM-dd",
                    IncludeDate = true,
                    IncludeIndex = true,
                    Separator = "_"
                }
            }
        }
    }
    };

    ILogConfigurationProvider provider = new nMemoryConfigurationProvider(config);

    LoggerManager.Initialize(provider);
```

---

### 3️⃣ Get a Logger Instance

```csharp
ISmartLogger logger = LoggerManager.GetLogger("OrderService");
```

---

### 4️⃣ Log Messages

```csharp
logger.Info("Order created successfully.");
logger.Warning("Inventory running low.");
logger.Error("Payment gateway timeout.");
```

---

## Example Configuration (JSON)

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": "Console",
      "threshold": "DEBUG",
      "outputFormat": "PlainText",
      "layoutType": "Detailed"
    }
  ]
}
```

For more information on configuration refer: [SmartLogger_Configuration_Guide](https://github.com/srimani-ravikumar/SmartLogger/blob/main/docs/configuration-guide.md)
---

## Ideal Use Cases

SmartLogger is suitable for:

* Web APIs
* Microservices
* Background processing systems
* Internal enterprise tools
* Learning and system design exploration

---

## Summary

SmartLogger provides a clean, extensible logging solution for .NET applications, with built-in support for correlation, runtime configuration, and multi-threaded safety.

It enables teams to **build observable and maintainable systems without complex setup.**

---