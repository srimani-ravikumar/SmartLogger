using SmartLogger.Configurations;
using SmartLogger.Core;
using SmartLogger.Filters;

Console.WriteLine("=== Smart Logger Framework Demo ===");
Console.WriteLine();

DemoBasicLoggingWithJsonConfigProvider();

DemoBasicLoggingWithInMemoryConfigProvider();

DemoMultiThreadedLoggingWithJsonConfigProvider();

Console.WriteLine();
Console.WriteLine("=== Demo Completed ===");

void DemoBasicLoggingWithJsonConfigProvider()
{
    Console.WriteLine("1. Basic Logging with JSON Config Provider Demo...");
    Console.WriteLine("-----------------------------------");

    // 1. Create configuration provider
    var path = Path.Combine(AppContext.BaseDirectory, "smartlogger.json");
    ILogConfigurationProvider provider = new JsonConfigurationProvider(filePath: path, enableAutoReload: true);

    // 2. Register logger factory
    LoggerManager.Initialize(provider);

    // 3. Get logger
    ISmartLogger logger = LoggerManager.GetLogger("Program");

    // 2. Set a Correlation ID for the current request context
    // LoggerImplementation.SetCorrelationId("REQ-550E");

    // 3. Log basic levels
    // The source will automatically be captured as "Program.DemoBasicLogging"
    logger.Debug("Initializing system components...");
    logger.Info("User 'JohnDoe' has connected.");
    logger.Warning("Memory usage is reaching 80% threshold.");

    // 4. Simulate an Error
    try
    {
        throw new InvalidOperationException("Database connection failed!");
    }
    catch (Exception ex)
    {
        logger.Error($"Critical Error: {ex.Message}");
    }

    // 5. Demonstrate Filtering
    Console.WriteLine("\n--- Testing Level Filter (Setting to Warning only) ---");
    logger.AddFilter(new LevelFilter(LogLevel.WARNING));

    logger.Info("This info message will NOT be displayed.");
    logger.Fatal("SYSTEM HALTED: Kernel panic detected.");
}

void DemoBasicLoggingWithInMemoryConfigProvider()
{
    Console.WriteLine("1. Basic Logging with In-Memory Config Provider Demo...");
    Console.WriteLine("-----------------------------------");

    // 1. Create configuration provider
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
            Settings = new Dictionary<string, string>
            {
                { "filePath", "logs/app.log" }
            }
        }
    }
    };

    ILogConfigurationProvider provider =
        new InMemoryConfigurationProvider(config);

    // 2. Register logger factory
    LoggerManager.Initialize(provider);

    // 3. Get logger
    ISmartLogger logger = LoggerManager.GetLogger("Program");

    // 2. Set a Correlation ID for the current request context
    // LoggerImplementation.SetCorrelationId("REQ-550E");

    // 3. Log basic levels
    // The source will automatically be captured as "Program.DemoBasicLogging"
    logger.Debug("Initializing system components...");
    logger.Info("User 'JohnDoe' has connected.");
    logger.Warning("Memory usage is reaching 80% threshold.");

    // 4. Simulate an Error
    try
    {
        throw new InvalidOperationException("Database connection failed!");
    }
    catch (Exception ex)
    {
        logger.Error($"Critical Error: {ex.Message}");
    }

    // 5. Demonstrate Filtering
    Console.WriteLine("\n--- Testing Level Filter (Setting to Warning only) ---");
    logger.AddFilter(new LevelFilter(LogLevel.WARNING));

    logger.Info("This info message will NOT be displayed.");
    logger.Fatal("SYSTEM HALTED: Kernel panic detected.");
}

void DemoMultiThreadedLoggingWithJsonConfigProvider()
{
    Console.WriteLine("2. Multi-Threaded Logging Demo (JSON Provider)...");
    Console.WriteLine("--------------------------------------------------");

    // 1. Load JSON Configuration
    var path = Path.Combine(
        AppContext.BaseDirectory,
        "smartlogger.json");

    ILogConfigurationProvider provider =
        new JsonConfigurationProvider(filePath: path, enableAutoReload: true);

    // 2. Initialize Logger
    LoggerManager.Initialize(provider);

    ISmartLogger logger =
        LoggerManager.GetLogger("MultiThreadDemo");

    Console.WriteLine("Starting 5 concurrent threads...\n");

    int threadCount = 5;
    int logsPerThread = 10;

    var tasks = new List<Task>();

    for (int i = 0; i < threadCount; i++)
    {
        int threadId = i;

        tasks.Add(Task.Run(() =>
        {
            for (int j = 0; j < logsPerThread; j++)
            {
                logger.Info(
                    $"Thread-{threadId} | Message-{j} | Executing on ThreadId={Thread.CurrentThread.ManagedThreadId}");

                Thread.Sleep(20); // Simulate work
            }
        }));
    }

    Task.WaitAll(tasks.ToArray());

    Console.WriteLine("\nAll threads completed.");
}
