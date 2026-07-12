## SmartLogger: Lightweight Logging for High-Performance Systems

## Document Information

| Project | Version | Date | Author | Status | Description |
|---------|---------|------------|---------|-------------|-------------|
| SmartLogger | 1.0.0 | 2026-07-12 | Srimani | Final | Provides an overview of SmartLogger, its architecture, key features, quick start guide, and configuration resources for developers. |

> **Build observability without sacrificing performance or simplicity.**

SmartLogger is a lightweight, extensible logging framework for .NET applications, designed to provide **structured, reliable, and configurable logging** without unnecessary complexity.

---

## Design Philosophy

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
Log Call
   ↓
Logger
   ↓
Layout (Pattern + Tokens)
   ↓
Formatter (PlainText / JSON / XML)
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
* Multiple output formats (PlainText, JSON, XML)
* Multiple appenders (Console, FileSystem, extensible)
* Offers both Synchronous and asynchronous logging pipeline
* Runtime configuration reload with zero downtime
* Correlation context for distributed systems
* Overload protection and logging health monitoring
* Configurable file rolling and simple & intuitive configurations

---

## 💡 Why SmartLogger?

SmartLogger helps you:

* Maintain consistent logging standards
* Trace requests across execution flows using correlation IDs
* Handle high log volumes safely
* Dynamically update logging without restarting apps
* Build observable systems with minimal setup

---

## Quick Start

Get SmartLogger up and running in just a few steps.

### 1️⃣ Install via NuGet

```powershell
Install-Package SmartLogger
```

### 2️⃣ Configure & Initialize SmartLogger

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

LoggerManager.Initialize(
    new InMemoryConfigurationProvider(configuration));
```

Ideal for unit tests, sample applications, or scenarios where the logging configuration is created programmatically.

### 3️⃣ Retrieve a Logger

Using the current class (recommended)

```csharp
var logger = LoggerManager.GetLogger(typeof(OrderService));
```

Or using a custom logger name

```csharp
var logger = LoggerManager.GetLogger("OrderService");
```

### 4️⃣ Write Log Messages

```csharp
logger.Debug("Initializing payment workflow...");
logger.Info("Order created successfully.");
logger.Warning("Inventory running low.");
logger.Error("Payment gateway timeout.");
```

### 5️⃣ Enable File Logging *(Optional)*

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

LoggerManager.Initialize(
    new InMemoryConfigurationProvider(configuration));
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
* Web APIs
* Background workers
* Microservices
* High-throughput systems

---

## Summary

SmartLogger provides a clean and extensible logging solution for .NET applications, with support for:

* Structured logging
* Correlation tracking
* Runtime configuration
* High-performance scenarios

> Designed to help you build **observable, maintainable, and resilient systems**.

---

<p align="center">
<strong>© 2026 Srimani. All rights reserved.</strong><br/>
<em>SmartLogger — Lightweight Logging for High-Performance .NET Applications</em>
</p>