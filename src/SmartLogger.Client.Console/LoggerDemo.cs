using SmartLogger.Appenders.Aggregation;
using SmartLogger.Configurations;
using SmartLogger.Core;
using System.Threading;

internal class LoggerDemo
{
    public static int Main(string[] args)
    {
        Console.WriteLine("=== SmartLogger Framework Demo ===\n");
        Console.WriteLine("Each demo below mirrors a recipe from docs/client/configuration-guide.md\n");

        //DemoLocalDevelopmentConsole();
        //DemoMinimalZeroConfig();
        //DemoProductionFileLogging();
        //DemoConsolePlusFile();
        //DemoRemoteLogAggregatorUseDefault(); TODO: enfore string config validation before launching
        //DemoRemoteLogAggregatorCustomSinkViaCode();
        //DemoRemoteLogAggregatorCustomSinkViaTypeName();
        //DemoHighThroughputAsync();
        //DemoLongRunningCompliance();
        //DemoPerComponentOverrides();
        //DemoCustomConsolePattern();
        //DemoJsonTrimmedFields();
        DemoAutoReloadBattleTest();

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
    // Use Case: Remote Log Aggregator - useDefault: true (built-in HttpLogAggregatorSink)
    // --------------------------------------------------------

    private static void DemoRemoteLogAggregatorUseDefault()
    {
        Console.WriteLine("\n5a. Remote Log Aggregator - useDefault: true");
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

        // Requires a listener at the configured endpoint - spin up SmartLogger.Aggregator.Demo first.
        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("RemoteAggregatorUseDefault");

        logger.Info("Shipped via the built-in HttpLogAggregatorSink");
    }

    // --------------------------------------------------------
    // Use Case: Remote Log Aggregator - useDefault: false, custom sink wired in code (sinkKey)
    // --------------------------------------------------------

    private static void DemoRemoteLogAggregatorCustomSinkViaCode()
    {
        Console.WriteLine("\n5b. Remote Log Aggregator - useDefault: false (sinkKey, code wiring)");
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = false,
                            SinkKey = "console-demo-sink"
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        var customSinks = new Dictionary<string, ILogAggregatorSink>
        {
            ["console-demo-sink"] = new ConsoleLogAggregatorSink()
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config), customSinks);

        var logger = LoggerManager.GetLogger("RemoteAggregatorCustomSinkViaCode");

        logger.Info("Shipped via a custom sink instance registered at Initialize() time");
    }

    // --------------------------------------------------------
    // Use Case: Remote Log Aggregator - useDefault: false, custom sink resolved from config (sinkTypeName)
    // --------------------------------------------------------

    private static void DemoRemoteLogAggregatorCustomSinkViaTypeName()
    {
        Console.WriteLine("\n5c. Remote Log Aggregator - useDefault: false (sinkTypeName, config wiring)");
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
                        Type = LogOutputDestination.LogAggregator,
                        LogAggregator = new LogAggregatorConfiguration
                        {
                            UseDefault = false,
                            // Assembly-qualified name so SmartLogger can Activator.CreateInstance it - no code wiring needed.
                            SinkTypeName = typeof(ConsoleLogAggregatorSink).AssemblyQualifiedName
                        }
                    },
                    Formatter = new FormatterConfiguration
                    {
                        OutputFormat = LogOutputFormat.Json
                    }
                }
            }
        };

        // No customSinks dictionary - the sink is instantiated purely from configuration.
        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("RemoteAggregatorCustomSinkViaTypeName");

        logger.Info("Shipped via a custom sink resolved by assembly-qualified type name");
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

    // --------------------------------------------------------
    // Use Case: Live Auto-Reload Battle Test
    // --------------------------------------------------------
    // smartlogger.json (copied next to the build output) is the file under test.
    // Each scenario below is independent - uncomment ONE at a time and run the
    // matching PowerShell script from scripts/ in a separate terminal while it loops.
    // Run scripts/restore-baseline.ps1 between scenarios to reset to a clean state.

    private static void DemoAutoReloadBattleTest()
    {
        Console.WriteLine("\n11. Live Auto-Reload Battle Test");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Edit scripts/*.ps1 usage notes in each scenario method.\n");

        Scenario_BasicLiveReload_LogLevelChange();
        //Scenario_InvalidJsonDuringEdit_ShouldKeepOldConfig();
        //Scenario_SemanticallyInvalidConfig_ShouldKeepOldConfig();
        //Scenario_RapidSaveStorm_Debounce();
        //Scenario_AppenderDestinationSwitch_RuntimeRewire();
        //Scenario_FileDeletedWhileWatched();
    }

    /// <summary>
    /// Scenario 1: rootLogLevel flips INFO -> DEBUG -> INFO while logging continuously.
    /// Expected: Debug lines appear/disappear live, no restart needed.
    /// </summary>
    private static void Scenario_BasicLiveReload_LogLevelChange()
    {
        Console.WriteLine("Scenario 1: Basic live reload (rootLogLevel INFO <-> DEBUG)");
        Console.WriteLine("Run: scripts/toggle-loglevel.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.BasicLevel"), TimeSpan.FromSeconds(35));
    }

    /// <summary>
    /// Scenario 2: config file is briefly replaced with malformed JSON, then restored.
    /// Expected: reload attempt fails silently, previous configuration keeps running.
    /// </summary>
    private static void Scenario_InvalidJsonDuringEdit_ShouldKeepOldConfig()
    {
        Console.WriteLine("Scenario 2: Malformed JSON mid-edit (must keep last good config)");
        Console.WriteLine("Run: scripts/corrupt-json.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.CorruptJson"), TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Scenario 3: config file is briefly replaced with structurally valid but semantically
    /// invalid JSON (FileSystem destination without a "file" block), then restored.
    /// Expected: ConfigurationValidator rejects the reload, previous configuration keeps running.
    /// </summary>
    private static void Scenario_SemanticallyInvalidConfig_ShouldKeepOldConfig()
    {
        Console.WriteLine("Scenario 3: Semantically invalid config (must keep last good config)");
        Console.WriteLine("Run: scripts/invalid-semantic-config.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.InvalidSemantics"), TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Scenario 4: many rapid writes simulate a FileSystemWatcher event storm.
    /// Expected: reload lock + 100ms debounce absorb the storm, no crash, final write wins.
    /// </summary>
    private static void Scenario_RapidSaveStorm_Debounce()
    {
        Console.WriteLine("Scenario 4: Rapid save storm (debounce under FileSystemWatcher flood)");
        Console.WriteLine("Run: scripts/reload-storm.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.Storm"), TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Scenario 5: appender destination flips between Console and FileSystem at runtime.
    /// Expected: LoggerFactory soft-reload rewires appenders live; check Logs/ReloadTest*.log
    /// for leaked/duplicate file handles after the run.
    /// </summary>
    private static void Scenario_AppenderDestinationSwitch_RuntimeRewire()
    {
        Console.WriteLine("Scenario 5: Appender destination switch (Console <-> FileSystem)");
        Console.WriteLine("Run: scripts/switch-destination.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.DestinationSwitch"), TimeSpan.FromSeconds(25));
    }

    /// <summary>
    /// Scenario 6: watched config file is deleted, then recreated with a different rootLogLevel.
    /// Expected: Created/Deleted watcher events (added alongside Changed) pick up the recreate,
    /// triggering a reload with no crash in between.
    /// </summary>
    private static void Scenario_FileDeletedWhileWatched()
    {
        Console.WriteLine("Scenario 6: Config file deleted and recreated while watched");
        Console.WriteLine("Run: scripts/delete-and-recreate.ps1\n");

        LoggerManager.Initialize(new JsonConfigurationProvider("smartlogger.json", enableAutoReload: true));

        RunObservationLoop(LoggerManager.GetLogger("ReloadTest.DeleteRecreate"), TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Logs a Debug/Info tick every 500ms for the given duration so reload effects are visible live.
    /// </summary>
    private static void RunObservationLoop(ISmartLogger logger, TimeSpan duration)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (sw.Elapsed < duration)
        {
            logger.Debug($"[{DateTime.Now:HH:mm:ss}] debug tick");
            logger.Info($"[{DateTime.Now:HH:mm:ss}] info tick");
            Thread.Sleep(500);
        }
    }
}

/// <summary>
/// Minimal <see cref="ILogAggregatorSink"/> used to demo custom sink wiring (code and config based).
/// </summary>
/// <remarks>
/// Requires a public parameterless constructor so it can also be resolved via <c>SinkTypeName</c> reflection.
/// </remarks>
public sealed class ConsoleLogAggregatorSink : ILogAggregatorSink
{
    /// <inheritdoc/>
    public void Send(LogMessage message) =>
        Console.WriteLine($"[CustomSink] {message.LogLevel} | {message.Message}");
}
