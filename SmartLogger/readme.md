# SmartLogger

### A Lightweight, Production-Ready Logging Framework for .NET Applications

SmartLogger is an extensible logging framework designed to provide structured, reliable, and configurable logging for modern .NET applications.

It is built to support:

* Multi-threaded environments
* Runtime configuration updates
* Correlation-based request tracing
* Multiple output destinations
* Resilient logging under load

---

## Why Use SmartLogger?

SmartLogger helps teams:

* Maintain consistent logging standards
* Trace requests across services using correlation IDs
* Dynamically update logging rules without restarting applications
* Protect applications during high log volume scenarios
* Detect logging pipeline failures early

It is designed to be simple to integrate and safe to use in production systems.

---

## Key Features

### 1. Structured Logging

Each log entry includes:

* Timestamp
* Log level
* Message
* Source (class/method)
* Correlation ID

---

### 2. Multiple Output Destinations

Logs can be written to:

* Console
* File system
* (Extensible to database or other destinations)

Each destination operates independently.

---

### 3. Correlation ID Support

SmartLogger allows request-level tracing:

```csharp
LoggerImplementation.SetCorrelationId("REQ-12345");
```

All logs generated within that execution flow automatically include the correlation ID, making troubleshooting significantly easier.

---

### 4. Runtime Configuration Reload

Logging configuration can be updated without restarting the application.

When enabled, SmartLogger monitors configuration changes and applies updates dynamically.

---

### 5. Thread-Safe Logging

SmartLogger is designed to safely handle concurrent log requests across multiple threads without data corruption.

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
  "rootLogLevel": "info",
  "appenders": [
    {
      "destination": "console",
      "threshold": "debug"
    },
    {
      "destination": "fileSystem",
      "threshold": "info",
      "settings": {
        "filePath": "logs/app.log"
      }
    }
  ]
}
```

---

## Ideal Use Cases

SmartLogger is suitable for:

* Web APIs
* Microservices
* Background processing systems
* Internal enterprise tools
* Learning and system design exploration

---

## Design Philosophy

SmartLogger is built with the principle:

> Logging must never compromise application stability.

It prioritizes:

* Safe execution
* Clear configuration
* Predictable behavior
* Extensibility

---

## Summary

SmartLogger provides a clean, extensible logging solution for .NET applications, with built-in support for correlation, runtime configuration, and multi-threaded safety.

It enables teams to build observable and maintainable systems without complex setup.

---