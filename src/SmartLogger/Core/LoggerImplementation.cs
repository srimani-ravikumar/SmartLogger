using SmartLogger.Appenders;
using System.Collections.Generic;
using System.Linq;

namespace SmartLogger.Core;

/// <summary>
/// Core implementation of the <see cref="ISmartLogger"/> interface.
/// </summary>
/// <remarks>
/// Responsible for:
/// <list type="bullet">
/// <item><description>Constructing <see cref="LogMessage"/> instances</description></item>
/// <item><description>Applying filters</description></item>
/// <item><description>Dispatching logs to appenders</description></item>
/// </list>
/// 
/// Threading Model:
/// <list type="bullet">
/// <item><description>Appender list is snapshotted during logging to avoid concurrent modification issues</description></item>
/// <item><description>Configuration updates replace appenders under lock</description></item>
/// </list>
/// </remarks>
internal sealed class LoggerImplementation : ISmartLogger
{
    /// <summary>
    /// Logical name of the logger (typically class or namespace).
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Collection of appenders responsible for output.
    /// </summary>
    private readonly List<ILogAppender> _appenders;

    /// <summary>
    /// Collection of filters applied before dispatching logs.
    /// </summary>
    private readonly List<ILogFilter> _filters;

    /// <summary>
    /// Minimum effective log level across all configurations.
    /// </summary>
    /// <remarks>
    /// Derived from root level and appender-specific levels.
    /// </remarks>
    internal LogLevel EffectiveMinLogLevel { get; private set; }

    #region Telescoping constructors

    internal LoggerImplementation() : this("DefaultLogger") { }

    internal LoggerImplementation(string name) : this(name, LogLevel.INFO) { }

    internal LoggerImplementation(string name, LogLevel loglevel) : this(name, loglevel, true) { }

    internal LoggerImplementation(string name, LogLevel effectiveMinLogLevel, bool enableDefaultConsoleAppender)
    {
        _name = name;
        EffectiveMinLogLevel = effectiveMinLogLevel;
        _appenders = new List<ILogAppender>();
        _filters = new List<ILogFilter>();

        // Optional default appender for quick usability
        if (enableDefaultConsoleAppender)
        {
            _appenders.Add(new ConsoleAppender());
        }
    }

    #endregion

    /// <summary>
    /// Core logging pipeline entry point.
    /// </summary>
    /// <param name="logLevel">Severity of the log.</param>
    /// <param name="message">Log message content.</param>
    /// <remarks>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>Level filtering (fast-fail)</description></item>
    /// <item><description>Message construction</description></item>
    /// <item><description>Filter evaluation</description></item>
    /// <item><description>Appender dispatch</description></item>
    /// </list>
    /// </remarks>
    public void Log(LogLevel logLevel, string message)
    {
        // 1. Minimum Level Check (fast path)
        if (!logLevel.IsGreaterOrEqual(EffectiveMinLogLevel)) return;

        // 2. Build the message (captures thread + correlation context)
        var logMessage = new LogMessage.Builder()
            .WithLevel(logLevel)
            .WithMessage(message)
            .FromSource(_name)
            .WithCorrelationId(LogContext.CorrelationId) // AsyncLocal-based propagation
            .Build();

        // 3. Apply filters (short-circuit if any filter rejects)
        if (_filters.Any(filter => !filter.ShouldLog(logMessage)))
        {
            return;
        }

        // 4. Snapshot appenders to avoid concurrent modification issues
        var appendersSnapshot = _appenders.ToArray();

        // 5. Dispatch to enabled appenders
        foreach (ILogAppender appender in appendersSnapshot)
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

    /// <summary>
    /// Retrieves a read-only list of all currently attached appenders.
    /// </summary>
    /// <returns>A collection of <see cref="ILogAppender"/> instances.</returns>
    internal IReadOnlyList<ILogAppender> GetLogAppenders() => _appenders.AsReadOnly();

    /// <summary>
    /// Attaches an output destination (appender) to the logger.
    /// </summary>
    /// <param name="appender">The implementation of <see cref="ILogAppender"/> to add.</param>
    internal void AddAppender(ILogAppender appender) => _appenders.Add(appender);

    /// <summary>
    /// Detaches an output destination (appender) from the logger.
    /// </summary>
    /// <param name="appender">The appender to remove.</param>
    internal void RemoveAppender(ILogAppender appender) => _appenders.Remove(appender);

    /// <summary>
    /// Retrieves a read-only list of all currently active filters.
    /// </summary>
    /// <returns>A collection of <see cref="ILogFilter"/> instances.</returns>
    internal IReadOnlyList<ILogFilter> GetLogFilters() => _filters.AsReadOnly();

    /// <summary>
    /// Adds a logic-based filter to the logging pipeline.
    /// </summary>
    /// <param name="filter">The <see cref="ILogFilter"/> to evaluate before logging.</param>
    internal void AddFilter(ILogFilter filter) => _filters.Add(filter);

    /// <summary>
    /// Removes a logic-based filter from the logging pipeline.
    /// </summary>
    /// <param name="filter">The filter to remove.</param>
    internal void RemoveFilter(ILogFilter filter) => _filters.Remove(filter);

    /// <summary>
    /// Updates the minimum <see cref="LogLevel"/> required for messages to be processed.
    /// </summary>
    /// <param name="logLevel">The new minimum threshold level.</param>
    internal void SetLogLevel(LogLevel logLevel) => EffectiveMinLogLevel = logLevel;

    /// <summary>
    /// Updates logger configuration dynamically.
    /// </summary>
    /// <param name="newLevel">New effective log level.</param>
    /// <param name="newAppenders">New set of appenders.</param>
    /// <remarks>
    /// Applies changes atomically where possible:
    /// <list type="bullet">
    /// <item><description>Log level updated via atomic write</description></item>
    /// <item><description>Appender list replaced under lock</description></item>
    /// </list>
    /// </remarks>
    internal void UpdateConfiguration(LogLevel newLevel, List<ILogAppender> newAppenders)
    {
        // 1. Update level (atomic write)
        EffectiveMinLogLevel = newLevel;

        // 2. Replace appenders atomically
        lock (_appenders)
        {
            _appenders.Clear();
            _appenders.AddRange(newAppenders);
        }

        // TODO: implement the same for filters
    }

    #endregion
}