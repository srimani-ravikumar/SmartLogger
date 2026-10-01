using SmartLogger.Configurations;
using SmartLogger.Core;

internal class LoggerDemo
{
    public static int Main(string[] args)
    {
        Console.WriteLine("=== SmartLogger Framework Demo ===\n");
        Console.WriteLine("Each demo below mirrors a recipe from docs/client/configuration-guide.md\n");

        DemoLocalDevelopmentConsole();
        //DemoMinimalZeroConfig();
        //DemoProductionFileLogging();
        //DemoConsolePlusFile();
        //DemoRemoteLogAggregator();
        //DemoHighThroughputAsync();
        //DemoLongRunningCompliance();
        //DemoPerComponentOverrides();
        //DemoCustomConsolePattern();
        //DemoJsonTrimmedFields();

        Console.WriteLine("\n=== Demo Completed ===");
        Console.ReadKey();

        return 1;
    }

    // --------------------------------------------------------
    // Use Case: Local Development - Console, Verbose
    // --------------------------------------------------------

    private static void DemoLocalDevelopmentConsole()
    {
        Console.WriteLine("1. Local Development - Console, Verbose");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
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
                        LayoutType = LogMessageLayoutType.Detailed
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("LocalDev");

        logger.Debug("Loading configuration files...");
        logger.Info("Application bootstrap started");
        logger.Warning("Cache service running in degraded mode");
        logger.Error("Simulated startup error for visibility");
    }

    // --------------------------------------------------------
    // Use Case: Minimal / Zero Config
    // --------------------------------------------------------

    private static void DemoMinimalZeroConfig()
    {
        Console.WriteLine("\n2. Minimal / Zero Config");
        Console.WriteLine("-----------------------------------");

        // No appenders configured -> SmartLogger auto-enables a default Console Appender.
        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.INFO
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("MinimalConfig");

        logger.Info("Running with zero appenders configured");
        logger.Warning("Still logs to console via the default appender");
    }

    // --------------------------------------------------------
    // Use Case: Production - File Logging, JSON, Rolling
    // --------------------------------------------------------

    private static void DemoProductionFileLogging()
    {
        Console.WriteLine("\n3. Production - File Logging, JSON, Rolling");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
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
                            Rolling = new FileRollingConfiguration { Strategy = RollingStrategyType.Daily },
                            Archive = new ArchiveConfiguration { Enabled = true, Compress = true },
                            Retention = new RetentionConfiguration { RetentionDays = 30 }
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("ProductionFileLogging");

        logger.Info("Order created successfully");
        logger.Warning("Inventory running low");
        logger.Error("Payment gateway timeout");
    }

    // --------------------------------------------------------
    // Use Case: Console + File (Common Combo)
    // --------------------------------------------------------

    private static void DemoConsolePlusFile()
    {
        Console.WriteLine("\n4. Console + File (Common Combo)");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
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
                    }
                },
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
                            Rolling = new FileRollingConfiguration { Strategy = RollingStrategyType.Daily },
                            Archive = new ArchiveConfiguration { Enabled = true, Compress = true }
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("ConsolePlusFile");

        logger.Info("Application event triggered");
        logger.Debug("Writing logs to multiple destinations...");
        logger.Warning("Disk usage nearing threshold");
    }

    // --------------------------------------------------------
    // Use Case: Remote Log Aggregator (Centralized Sink)
    // --------------------------------------------------------

    private static void DemoRemoteLogAggregator()
    {
        Console.WriteLine("\n5. Remote Log Aggregator (Centralized Sink)");
        Console.WriteLine("-----------------------------------");

        // Mirrors the configuration-guide.md recipe. The LogAggregator destination
        // is not yet wired into LoggerFactory, so this method only builds the
        // configuration shape for reference - see SmartLogger.Aggregator.Demo
        // to try the sink end-to-end via HttpLogAggregatorSink directly.
        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.INFO,
            Appenders = new List<AppenderConfiguration>
            {
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = true,
                            Endpoint = new Uri("http://localhost:5290/api/logs")
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        Console.WriteLine($"Configured endpoint: {config.Appenders[0].Destination.LogAggregator!.Endpoint}");
        Console.WriteLine("Note: not initialized - runtime wiring for this destination is pending.");
    }

    // --------------------------------------------------------
    // Use Case: High-Throughput APIs / Workers
    // --------------------------------------------------------

    private static void DemoHighThroughputAsync()
    {
        Console.WriteLine("\n6. High-Throughput APIs / Workers (Stress Test)");
        Console.WriteLine("--------------------------------------------------");

        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.INFO,
            EnableAsyncLoggingProcess = true,
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
                            Extension = "json",
                            Rolling = new FileRollingConfiguration
                            {
                                Strategy = RollingStrategyType.Size,
                                MaxFileSizeMB = 10
                            }
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("HighThroughput");

        int threadCount = Environment.ProcessorCount;
        int logsPerThread = 1000;

        Console.WriteLine($"Running with {threadCount} threads, {logsPerThread} logs per thread...\n");

        var sw = System.Diagnostics.Stopwatch.StartNew();

        Parallel.For(0, threadCount, threadId =>
        {
            using (LogContext.BeginCorrelationScope($"PERF-{threadId}"))
            {
                for (int i = 0; i < logsPerThread; i++)
                {
                    logger.Debug($"[T{threadId}] Processing item {i}");

                    if (i % 100 == 0)
                        logger.Info($"[T{threadId}] Checkpoint at {i}");

                    if (i % 250 == 0)
                        logger.Warning($"[T{threadId}] High load detected at {i}");

                    if (i % 400 == 0)
                    {
                        try
                        {
                            throw new Exception("Simulated processing failure");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"[T{threadId}] Error: {ex.Message}");
                        }
                    }
                }
            }
        });

        sw.Stop();

        Console.WriteLine("\n--- Performance Summary ---");
        Console.WriteLine($"Total logs: {threadCount * logsPerThread}");
        Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Logs/sec: {(threadCount * logsPerThread) / sw.Elapsed.TotalSeconds:F2}");

        logger.Info("High throughput logging test completed");
    }

    // --------------------------------------------------------
    // Use Case: Long-Running / Compliance Services
    // --------------------------------------------------------

    private static void DemoLongRunningCompliance()
    {
        Console.WriteLine("\n7. Long-Running / Compliance Services");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
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
                            Archive = new ArchiveConfiguration { Enabled = true, Compress = true },
                            Retention = new RetentionConfiguration { RetentionDays = 90 }
                        }
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("ComplianceService");

        logger.Info("Scheduled job started");
        logger.Info("Audit record written with 90 day retention");
    }

    // --------------------------------------------------------
    // Use Case: Per-Component Log Levels (Overrides)
    // --------------------------------------------------------

    private static void DemoPerComponentOverrides()
    {
        Console.WriteLine("\n8. Per-Component Log Levels (Overrides)");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.INFO,
            LoggerOverrides = new List<LoggerOverrideConfiguration>
            {
                new LoggerOverrideConfiguration { LoggerName = "SmartLogger.PaymentService", LogLevel = LogLevel.DEBUG },
                new LoggerOverrideConfiguration { LoggerName = "SmartLogger.Database", LogLevel = LogLevel.ERROR }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var paymentLogger = LoggerManager.GetLogger("SmartLogger.PaymentService");
        var databaseLogger = LoggerManager.GetLogger("SmartLogger.Database");

        paymentLogger.Debug("Verbose payment diagnostics (visible due to override)");
        databaseLogger.Warning("Suppressed - Database override only allows ERROR and above");
        databaseLogger.Error("Connection pool exhausted");
    }

    // --------------------------------------------------------
    // Use Case: Custom Console Pattern
    // --------------------------------------------------------

    private static void DemoCustomConsolePattern()
    {
        Console.WriteLine("\n9. Custom Console Pattern");
        Console.WriteLine("-----------------------------------");

        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
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
                        LayoutType = LogMessageLayoutType.Custom,
                        Pattern = "[%LEVEL] [%THREAD] [%CORRELATION] >> %MESSAGE"
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("CustomLayout");

        using (LogContext.BeginCorrelationScope("REQ-1023"))
        {
            logger.Info("Payment processed successfully");
            logger.Warning("Minor inconsistency detected");
        }
    }

    // --------------------------------------------------------
    // Use Case: JSON With Trimmed / Renamed Fields
    // --------------------------------------------------------

    private static void DemoJsonTrimmedFields()
    {
        Console.WriteLine("\n10. JSON With Trimmed / Renamed Fields");
        Console.WriteLine("-----------------------------------");

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
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json,
                        IncludedJsonFields = new List<string> { "timestamp", "level", "message" },
                        JsonFieldMappings = new List<JsonFieldMappingConfiguration>
                        {
                            new JsonFieldMappingConfiguration { SourceField = "timestamp", TargetField = "@timestamp" },
                            new JsonFieldMappingConfiguration { SourceField = "level", TargetField = "severity" },
                            new JsonFieldMappingConfiguration { SourceField = "message", TargetField = "msg" }
                        }
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("TrimmedJsonFields");

        logger.Info("Order processed successfully.");
    }
}
