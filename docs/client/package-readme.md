## SmartLogger: Lightweight Logging for High-Performance Systems

> **Build observability without sacrificing performance or simplicity.**

## Why SmartLogger? - Design Philosophy

> **Logging must never compromise application stability.**

SmartLogger is built with a strong focus on:

* Predictable behavior under load
* Minimal runtime overhead
* Clear and flexible configuration
* Extensibility without complexity

---

## Architecture Overview

SmartLogger follows a clean, layered pipeline:

```
LoggerManager.Initialize
   ↓
LoggerManager.GetLogger
   ↓
logger.Info("This is your first log message!")
   ↓
Layout (Pattern + Tokens)
   ↓
Formatter (PlainText / JSON / etc.)
   ↓
Appender (Console / File / etc.)
```

This design ensures:

* Separation of concerns
* Easy extensibility
* Maintainable logging pipeline

---

## Key Features

* Priority-based log level management and filtering
* Multiple output formats (PlainText, JSON, extensible)
* Multiple appenders (Console, FileSystem, extensible)
* Offers both Synchronous and asynchronous logging pipeline
* Runtime configuration reload with zero downtime
* Correlation context for distributed systems
* Overload protection
* Configurable file rolling and simple & intuitive configurations

---

## Quick Start

Get SmartLogger up and running in just a few steps.

### 1️. Install via NuGet

```powershell
Install-Package SmartLogger
```

### 2️. Configure & Initialize SmartLogger

#### Option A – JSON Configuration *(Recommended)*

```csharp
var provider = new JsonConfigurationProvider(
    "smartlogger.json",
    enableAutoReload: true);

LoggerManager.Initialize(provider);
```

This approach is recommended for most applications as it supports **configuration hot reload** without restarting the application.

#### Option B – In-Memory Configuration

```csharp
var configuration = new LogConfigurationHolder
{
    RootLogLevel = LogLevel.INFO,

    Appenders = new List<AppenderConfiguration>
    {
        new AppenderConfiguration
        {
            Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.Console
            },

            Formatter = new FormatterConfiguration
            {
                OutputFormat = LogOutputFormat.PlainText,
                LayoutType = LogMessageLayoutType.Simple
            },

            AppenderLogLevel = LogLevel.INFO
        }
    }
};

LoggerManager.Initialize(new InMemoryConfigurationProvider(configuration));
```

Ideal for unit tests, sample applications, or scenarios where the logging configuration is created programmatically.

### 3️. Retrieve a Logger

Using the current class (recommended)

```csharp
var logger = LoggerManager.GetLogger(typeof(OrderService));
```

Or using a custom logger name

```csharp
var logger = LoggerManager.GetLogger("OrderService");
```

### 4️. Write Log Messages

```csharp
logger.Debug("Initializing payment workflow...");
logger.Info("Order created successfully.");
logger.Warning("Inventory running low.");
logger.Error("Payment gateway timeout.");
```

### 5️. Enable File Logging *(Optional)*

```csharp
var configuration = new LogConfigurationHolder
{
    RootLogLevel = LogLevel.INFO,

    Appenders = new List<AppenderConfiguration>
    {
        new AppenderConfiguration
        {
            Destination = new DestinationConfiguration
            {
                Type = LogOutputDestination.FileSystem,

                File = new FileConfiguration
                {
                    Directory = "Logs",
                    FileName = "Application",
                    Extension = "log",

                    Naming = new FileNamingConfiguration
                    {
                        Strategy = FileNamingStrategyType.Date
                    },

                    Rolling = new FileRollingConfiguration
                    {
                        Strategy = RollingStrategyType.Daily
                    },

                    Archive = new ArchiveConfiguration
                    {
                        Enabled = true,
                        Directory = "Logs\\Archive",
                        Compress = true
                    },

                    Retention = new RetentionConfiguration
                    {
                        RetentionDays = 30
                    }
                }
            },

            Formatter = new FormatterConfiguration
            {
                OutputFormat = LogOutputFormat.PlainText,
                LayoutType = LogMessageLayoutType.Detailed
            }
        }
    }
};

LoggerManager.Initialize(new InMemoryConfigurationProvider(configuration));
```

The default file logging behavior includes:

- Daily rolling
- Date-based file naming
- Automatic archive creation
- ZIP compression
- 30-day retention policy

---

## Correlation Logging Example

```csharp
using (LogContext.BeginCorrelationScope("REQ-123"))
{
    logger.Info("Processing request...");
    logger.Error("Request failed");
}
```

---

## Example JSON Configuration

```json
{
  "rootLogLevel": "DEBUG",
  "appenders": [
    {
      "destination": {
        "type": "Console"
      },
      "appenderLogLevel": "DEBUG",
      "formatter": {
        "outputFormat": "PlainText",
        "layoutType": "Detailed"
      }
    },
    {
      "destination": {
        "type": "FileSystem",
        "file": {
          "directory": "Logs",
          "fileName": "Application",
          "extension": "log",
          "naming": {
            "strategy": "Date"
          },
          "rolling": {
            "strategy": "Daily"
          },
          "archive": {
            "enabled": true,
            "directory": "Logs\\Archive",
            "compress": true
          },
          "retention": {
            "retentionDays": 30
          }
        }
      },
      "formatter": {
        "outputFormat": "Json"
      }
    }
  ]
}
```

The above example demonstrates a common production setup with:

- Console logging using a detailed plain text layout.
- File logging with JSON output.
- Daily file rolling.
- Automatic archive creation.
- ZIP compression.
- 30-day archive retention.

For a complete list of supported configuration options, examples, and best practices, refer to the **Configuration Guide**:

📖 **Configuration Guide**  [SmartLogger_Configuration_Guide](https://github.com/srimani-ravikumar/SmartLogger/blob/main/docs/client/configuration-guide.md)

---

## Ideal Use Cases

* Learning system design
* Monolith Web APIs
* Background workers
* High-throughput systems

---

<center><b>© 2026 Srimani. All rights reserved.</b></center>
</br>
<center><i>SmartLogger — Lightweight Logging for High-Performance .NET Applications</i></center>