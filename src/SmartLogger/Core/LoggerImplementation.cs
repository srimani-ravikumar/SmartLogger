using SmartLogger.Appenders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SmartLogger.Core;

/// <summary>
/// Core implementation of the <see cref="ISmartLogger"/> interface.
/// Handles the lifecycle of log messages, filtering, and distribution to appenders.
/// </summary>
internal sealed class LoggerImplementation : ISmartLogger
{
    private readonly string _name;
    private readonly List<ILogAppender> _appenders;
    private readonly List<ILogFilter> _filters;

    /// <summary>
    /// Gets the current global log level for this logger.
    /// </summary>
    internal LogLevel LogLevel { get; private set; }

    #region Telescoping constructors

    internal LoggerImplementation() : this("DefaultLogger") { }

    internal LoggerImplementation(string name) : this(name, LogLevel.INFO) { }

    internal LoggerImplementation(string name, LogLevel loglevel) : this(name, loglevel, true) { }

    internal LoggerImplementation(string name, LogLevel logLevel, bool enableDefaultConsoleAppender)
    {
        _name = name;
        LogLevel = logLevel;
        _appenders = new List<ILogAppender>();
        _filters = new List<ILogFilter>();

        if (enableDefaultConsoleAppender)
        {
            _appenders.Add(new ConsoleAppender());
        }
    }

    #endregion


    /// <summary>
    /// The primary logging facade that constructs the <see cref="LogMessage"/> and notifies appenders.
    /// </summary>
    public void Log(LogLevel logLevel, string message)
    {
        // 1. Minimum Level Check
        if (!logLevel.IsGreaterOrEqual(LogLevel)) return;

        // 2. Build the Message using the Builder (captures ThreadId automatically)
        var logMessage = new LogMessage.Builder().WithLevel(logLevel)
                                                 .WithMessage(message)
                                                 .FromSource(_name)
                                                 .WithCorrelationId(LogContext.CorrelationId) // Injected from the AsyncLocal storage 
                                                 .Build();

        // 3. Apply Filters
        if (_filters.Any(predicate: filter => !filter.ShouldLog(logMessage)))
        {
            return;
        }

        // 4. Distribute to Enabled Appenders
        foreach (var appender in _appenders)
        {
            if (appender.IsEnabled(logLevel))
            {
                appender.Append(logMessage);
            }
        }
    }

    #region Convenience Methods

    /// <inheritdoc/>
    public void Debug(string message) => Log(LogLevel.DEBUG, message);

    /// <inheritdoc/>
    public void Info(string message) => Log(LogLevel.INFO, message);

    /// <inheritdoc/>
    public void Warning(string message) => Log(LogLevel.WARNING, message);

    /// <inheritdoc/>
    public void Error(string message) => Log(LogLevel.ERROR, message);

    /// <inheritdoc/>
    public void Fatal(string message) => Log(LogLevel.FATAL, message);

    #endregion

    #region Management Methods

    /// <inheritdoc/>
    public IList<ILogAppender> GetLogAppenders() => _appenders.AsReadOnly();

    /// <inheritdoc/>
    public void AddAppender(ILogAppender appender) => _appenders.Add(appender);

    /// <inheritdoc/>
    public void RemoveAppender(ILogAppender appender) => _appenders.Remove(appender);

    /// <inheritdoc/>
    public IList<ILogFilter> GetLogFilters() => _filters.AsReadOnly();

    /// <inheritdoc/>
    public void AddFilter(ILogFilter filter) => _filters.Add(filter);

    /// <inheritdoc/>
    public void RemoveFilter(ILogFilter filter) => _filters.Remove(filter);

    /// <inheritdoc/>
    public void SetLogLevel(LogLevel level) => LogLevel = level;

    #endregion
}