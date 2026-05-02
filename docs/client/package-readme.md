## SmartLogger: Lightweight Logging for High-Performance Systems

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

* Log levels with priority filtering
* Customizable log layouts (pattern + tokens)
* Multiple output formats (PlainText, JSON, XML)
* Multiple appenders (Console, FileSystem, extensible)
* Async logging support for high throughput
* Runtime configuration reload (hot reload)
* Correlation ID support (AsyncLocal-based)
* Thread-safe logging pipeline
* File rolling strategies (size/time-based)

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

### 1️⃣ Install via NuGet

```powershell
Install-Package SmartLogger
```

---

### 2️⃣ Initialize Logger

#### JSON Configuration

```csharp
var provider = new JsonConfigurationProvider(
    "smartlogger.json",
    enableAutoReload: true);

LoggerManager.Initialize(provider);
```

---

#### In-Memory Configuration

```csharp
var config = new LogConfigurationHolder
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
            AppenderLogLevel = LogLevel.DEBUG
        }
    }
};

LoggerManager.Initialize(new InMemoryConfigurationProvider(config));
```

---

### 3️⃣ Get Logger

```csharp
var logger = LoggerManager.GetLogger("OrderService");
```

---

### 4️⃣ Log Messages

```csharp
logger.Info("Order created successfully.");
logger.Warning("Inventory running low.");
logger.Error("Payment gateway timeout.");
```

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
    }
  ]
}
```

For more information on configuration refer: [SmartLogger_Configuration_Guide](https://github.com/srimani-ravikumar/SmartLogger/blob/main/docs/client/configuration-guide.md)

---

## 🧪 Ideal Use Cases

* Learning system design
* Web APIs
* Background workers
* Microservices
* High-throughput systems

---

## 📌 Summary

SmartLogger provides a clean and extensible logging solution for .NET applications, with support for:

* Structured logging
* Correlation tracking
* Runtime configuration
* High-performance scenarios

> Designed to help you build **observable, maintainable, and resilient systems**.