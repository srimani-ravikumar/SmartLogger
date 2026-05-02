using SmartLogger.Configurations;
using SmartLogger.Core;

internal class LoggerDemo
{
    public static int Main(string[] args)
    {
        Console.WriteLine("=== SmartLogger Framework Demo ===\n");

        //DemoProductionBootstrap();
        //DemoStructuredLogging();
        //DemoAsyncCorrelationFlow().Wait();
        //DemoCustomPatternLayout();
        //DemoMultiAppender();
        DemoHighThroughputLogging();
        //DemoFailureScenario();

        Console.WriteLine("\n=== Demo Completed ===");
        Console.ReadKey();

        return 1;
    }

    // --------------------------------------------------------

    private static void DemoProductionBootstrap()
    {
        Console.WriteLine("1. Production Bootstrap Demo");
        Console.WriteLine("-----------------------------------");

        var provider = new JsonConfigurationProvider(
            Path.Combine(AppContext.BaseDirectory, "smartlogger.json"),
            enableAutoReload: true);

        LoggerManager.Initialize(provider);

        var logger = LoggerManager.GetLogger(typeof(LoggerDemo));

        logger.Info("Application bootstrap started");
        logger.Debug("Loading configuration files...");
        logger.Debug("Initializing dependency graph...");
        logger.Info("All services initialized successfully");

        logger.Warning("Cache service running in degraded mode");
        logger.Info("Application started successfully and ready to accept traffic");
    }

    // --------------------------------------------------------

    private static void DemoStructuredLogging()
    {
        Console.WriteLine("\n2. Structured Logging Demo");
        Console.WriteLine("-----------------------------------");

        var provider = new JsonConfigurationProvider(
            Path.Combine(AppContext.BaseDirectory, "smartlogger.json"),
            enableAutoReload: false);

        LoggerManager.Initialize(provider);

        var logger = LoggerManager.GetLogger("StructuredDemo");

        logger.Info("Incoming request received");
        logger.Debug("Validating user credentials...");
        logger.Info("User 'john_doe' authenticated successfully");

        logger.Warning("Suspicious login detected from new device");
        logger.Info("User preferences loaded");

        logger.Debug("Fetching dashboard data...");
        logger.Info("Dashboard rendered successfully");

        logger.Error("Failed to fetch analytics module due to timeout");
    }

    // --------------------------------------------------------

    private static async Task DemoAsyncCorrelationFlow()
    {
        Console.WriteLine("\n3. Async Correlation Flow Demo");
        Console.WriteLine("-----------------------------------");

        var provider = new JsonConfigurationProvider(
            Path.Combine(AppContext.BaseDirectory, "smartlogger.json"),
            enableAutoReload: false);

        LoggerManager.Initialize(provider);

        var logger = LoggerManager.GetLogger("AsyncFlow");

        using (LogContext.BeginCorrelationScope("REQ-789"))
        {
            logger.Info("Request started");
            logger.Debug("Parsing request headers...");
            logger.Debug("Validating authentication token...");

            await Task.Delay(100);

            await ProcessAsync(logger);

            logger.Info("Aggregating response...");
            logger.Info("Request completed successfully");
        }
    }

    private static async Task ProcessAsync(ISmartLogger logger)
    {
        logger.Debug("Entering business layer...");
        await Task.Delay(50);

        logger.Info("Processing order...");
        logger.Debug("Checking inventory...");
        logger.Info("Inventory confirmed");

        logger.Debug("Calling payment service...");
        await Task.Delay(50);

        logger.Info("Payment processed successfully");
    }

    // --------------------------------------------------------

    private static void DemoCustomPatternLayout()
    {
        Console.WriteLine("\n4. Custom Pattern Layout Demo");
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

        using (LogContext.BeginCorrelationScope("CTX-999"))
        {
            logger.Info("Custom layout initialized");
            logger.Debug("Thread-specific execution started");
            logger.Warning("Minor inconsistency detected");
            logger.Error("Simulated error in custom layout pipeline");
        }
    }

    // --------------------------------------------------------

    private static void DemoMultiAppender()
    {
        Console.WriteLine("\n5. Multi-Appender Demo");
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
                    }
                },
                new AppenderConfiguration
                {
                    Destination = new DestinationConfiguration
                    {
                        Type = LogOutputDestination.FileSystem,
                        File = new FileConfiguration
                        {
                            BasePath = "logs/app",
                            Extension = "log"
                        }
                    }
                }
            }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("MultiAppender");

        logger.Info("Application event triggered");
        logger.Debug("Writing logs to multiple destinations...");
        logger.Info("User session created");
        logger.Warning("Disk usage nearing threshold");
        logger.Error("Simulated file write delay detected");
    }

    // --------------------------------------------------------

    private static void DemoHighThroughputLogging()
    {
        Console.WriteLine("\n6. High Throughput Logging Demo (Stress Test)");
        Console.WriteLine("--------------------------------------------------");

        // In-memory config with async logging + file + console
        var config = new LogConfigurationHolder
        {
            RootLogLevel = LogLevel.DEBUG,
            EnableAsyncLoggingProcess = true,
            Appenders = new List<AppenderConfiguration>
        {
            new AppenderConfiguration
            {
                Destination = new DestinationConfiguration
                {
                    Type = LogOutputDestination.Console
                },
                AppenderLogLevel = LogLevel.INFO
            },
            new AppenderConfiguration
            {
                Destination = new DestinationConfiguration
                {
                    Type = LogOutputDestination.FileSystem,
                    File = new FileConfiguration
                    {
                        BasePath = "logs/perf",
                        Extension = "log",
                        Naming = new FileNamingConfiguration
                        {
                            IncludeDate = true,
                            IncludeIndex = true
                        },
                        RollingPolicy = new RollingPolicyConfiguration
                        {
                            RollingType = RollingType.Size,
                            MaxFileSizeMB = 1 // force rolling quickly
                        }
                    }
                },
                AppenderLogLevel = LogLevel.DEBUG
            }
        }
        };

        LoggerManager.Initialize(new InMemoryConfigurationProvider(config));

        var logger = LoggerManager.GetLogger("PerformanceTest");

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
                    // Mix of levels
                    logger.Debug($"[T{threadId}] Processing item {i}");

                    if (i % 100 == 0)
                    {
                        logger.Info($"[T{threadId}] Checkpoint at {i}");
                    }

                    if (i % 250 == 0)
                    {
                        logger.Warning($"[T{threadId}] High load detected at {i}");
                    }

                    // Simulate occasional failure
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
        Console.WriteLine($"Logs/sec: {(threadCount * logsPerThread) / (sw.Elapsed.TotalSeconds):F2}");

        logger.Info("High throughput logging test completed");
    }

    // --------------------------------------------------------

    private static void DemoFailureScenario()
    {
        Console.WriteLine("\n7. Failure Scenario Demo");
        Console.WriteLine("-----------------------------------");

        var provider = new JsonConfigurationProvider(
            Path.Combine(AppContext.BaseDirectory, "smartlogger.json"),
            enableAutoReload: false);

        LoggerManager.Initialize(provider);

        var logger = LoggerManager.GetLogger("FailureDemo");

        logger.Info("Initiating payment transaction...");
        logger.Debug("Validating card details...");
        logger.Info("Connecting to payment gateway...");

        try
        {
            throw new Exception("Payment gateway timeout");
        }
        catch (Exception ex)
        {
            logger.Error($"Transaction failed: {ex.Message}");
            logger.Warning("Retry mechanism will be triggered");
        }

        logger.Info("Transaction marked as failed in system");
    }
}