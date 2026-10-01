# SmartLogger

A logging framework I built from scratch to understand how production logging systems actually work under the hood, *not to replace Serilog or NLog.*

If you're reviewing this as a hiring manager, architect, or senior engineer: this README is for you. I'll keep it short.

## Why I Built This..

I'd used logging libraries for years without asking the harder questions: 
* How do they stay thread-safe under concurrent writes? 
* What happens when the logging pipeline itself fails.. does it take the app down with it?
* How does single log message got published to multiple destinations?
* How does the configuration layer works internally?

I wanted real answers, not documentation. So I built one.

## What It Pays Me Back..

Shipping this moved me from **using** logging frameworks to **reasoning about** the tradeoffs behind them, concurrency control, graceful degradation, and designing configuration surfaces that don't leak internal complexity to the caller.

## What I Was Solving For..

* **Thread safety** concurrent writes from multiple threads without corruption or blocking
* **Context propagation** correlation IDs flowing correctly across async boundaries
* **Backpressure handling** high-throughput logging without degrading the host application
* **Fault isolation** a failing appender (disk full, aggregator down) never crashes the caller
* **Live configuration reload** changing log levels/appenders without restarting the process

## What's in the Codebase..

* Pluggable appenders (Console, FileSystem, remote LogAggregator) behind a common interface
* Sync and async logging pipelines, selectable per deployment
* JSON/XML/PlainText formatters with a layout + token pipeline
* File rolling, archiving, and retention policies
* Hot-reloadable configuration via `JsonConfigurationProvider`
* Full test coverage under `SmartLogger.Tests`

See [docs/client/package-readme.md](docs/client/package-readme.md) for the user-facing quick start, and [docs/client/configuration-guide.md](docs/client/configuration-guide.md) for the full configuration reference.

## "You might think this was just AI-generated?"

* Fair question given the era. Short answer: no, AI was used to write only test cases `SmartLogger.Tests`, nothing else.

* Every design decision here, log configuration model, the appender/formatter separation, the thread-safety model, the concurrency model, what gets retried vs. what fails loudly, where configuration hot-reload is safe to apply, came from me actually hitting these problems and mastering

> one of the most valuable engineering skills: knowing **what not to build** something that AI was struggling to do so... :)

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>

