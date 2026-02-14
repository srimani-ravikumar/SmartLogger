using SmartLogger.Configurations;
using SmartLogger.Core;
using SmartLogger.Filters;

Console.WriteLine("=== Smart Logger Framework Demo ===");
Console.WriteLine();

DemoBasicLogging();

Console.WriteLine();
Console.WriteLine("=== Demo Completed ===");

void DemoBasicLogging()
{
    Console.WriteLine("1. Basic Logging & Context Demo...");
    Console.WriteLine("-----------------------------------");

    // 1. Create configuration provider
    var path = Path.Combine(
    AppContext.BaseDirectory,
    "smartlogger.json");
    ILogConfigurationProvider provider =
        new JsonConfigurationProvider(path);


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