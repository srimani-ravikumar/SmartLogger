namespace SmartLogger.Core
{
    public static class LoggerManager
    {
        private static LoggerFactory? _factory;

        public static void Initialize(ILogConfigurationProvider provider)
        {
            _factory = new LoggerFactory(provider);
        }

        public static ISmartLogger GetLogger(string name)
        {
            if (_factory is null)
                throw new InvalidOperationException("LoggerManager is not initialized.");

            return _factory.CreateLogger(name);
        }
    }

}