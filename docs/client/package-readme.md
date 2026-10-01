# SmartLogger

**A lightweight, high-performance logging library for .NET. Observability without the overhead.**

```powershell
Install-Package SmartLogger
```

```csharp
LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

var logger = LoggerManager.GetLogger(typeof(OrderService));
logger.Info("Order created successfully.");
```

That's it. you're logging. Keep reading for the parts that make it worth sticking around.

---

## Why SmartLogger?

* **Fast by default** async pipeline, non-blocking appenders, zero allocations on the hot path
* **Pluggable everywhere** Console, File, or remote LogAggregator; PlainText, JSON, or XML
* **Built for distributed systems** correlation context flows through every log line
* **Fails safe** logging problems never take down your application
* **Reload without restarting** edit `smartlogger.json`, config changes apply live

---

## 30-Second Quick Start

**1. Install**

```powershell
Install-Package SmartLogger
```

**2. Initialize** *(JSON config, hot-reloadable)*

```csharp
var provider = new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true);
LoggerManager.Initialize(provider);
```

```json
{
  "rootLogLevel": "INFO",
  "appenders": [
    { "destination": { "type": "Console" }, "formatter": { "outputFormat": "PlainText" } }
  ]
}
```

**3. Log**

```csharp
var logger = LoggerManager.GetLogger(typeof(OrderService));

logger.Debug("Initializing payment workflow...");
logger.Info("Order created successfully.");
logger.Warning("Inventory running low.");
logger.Error("Payment gateway timeout.");
```

Prefer code-only setup (tests, samples)? Use `InMemoryConfigurationProvider` instead which will use same API, no file required.

---

## Level Up

Once the basics are running, **SmartLogger scales with you:**

| Need | How |
|---|---|
| Write logs to disk with rolling + archiving | Add a `FileSystem` appender |
| Ship logs to a central aggregator | Add a `LogAggregator` appender (JSON only) |
| Trace a request across services | `LogContext.BeginCorrelationScope("REQ-123")` |
| Tune for high throughput | Enable the async logging pipeline |
| Change log levels without redeploying | Edit `smartlogger.json` — reload is automatic |

```csharp
using (LogContext.BeginCorrelationScope("REQ-123"))
{
    logger.Info("Processing request...");
    logger.Error("Request failed");
}
```

For full configuration schemas, file rolling/retention policies, and the LogAggregator setup, see the:

📖 **[Configuration Guide](https://github.com/srimani-ravikumar/SmartLogger/blob/main/docs/client/configuration-guide.md)**

---

## Where It Fits

Monoliths, microservices, web APIs, console apps and  Windows Services. In short, anywhere you need predictable logging that won't become the bottleneck.

---
<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
