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

    // AsyncLocal ensures the CorrelationId is unique to the current request/execution flow
    private static readonly AsyncLocal<string> _correlationId = new();

    /// <summary>
    /// Gets the current global log level for this logger.
    /// </summary>
    internal LogLevel LogLevel { get; private set; }

    internal LoggerImplementation() : this("DefaultLogger") { }

    internal LoggerImplementation(string name) : this(name, LogLevel.INFO, true) { }

    internal LoggerImplementation(string name, LogLevel loglevel) : this(name, loglevel, true) { }

    internal LoggerImplementation(string name, LogLevel logLevel, bool enableDefaultAppender)
    {
        _name = name;
        LogLevel = logLevel;
        _appenders = new List<ILogAppender>();
        _filters = new List<ILogFilter>();

        if (enableDefaultAppender)
        {
            _appenders.Add(new ConsoleAppender());
        }
    }

    /// <summary>
    /// Sets a Correlation ID for the current logical operation/thread.
    /// </summary>
    internal static void SetCorrelationId(string id) => _correlationId.Value = id;

    /// <summary>
    /// The primary logging method that constructs the <see cref="LogMessage"/> and notifies appenders.
    /// </summary>
    public void Log(LogLevel level, string message)
    {
        // 1. Minimum Level Check
        if (!level.IsGreaterOrEqual(LogLevel)) return;

        // 2. Build the Message using the Builder (captures ThreadId automatically)
        var logMessage = new LogMessage.Builder()
            .WithLevel(level)
            .WithMessage(message)
            .FromSource(GetCallingSource())
            .WithCorrelationId(_correlationId.Value) // Injected from the AsyncLocal storage
            .Build();

        // 3. Apply Filters
        if (_filters.Any(filter => !filter.ShouldLog(logMessage)))
        {
            return;
        }

        // 4. Distribute to Enabled Appenders
        foreach (var appender in _appenders)
        {
            if (appender.IsEnabled(level))
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

    /// <summary>
    /// Automatically determines the calling class and method name by inspecting the execution stack.
    /// </summary>
    /// <remarks>
    /// This method walks up the stack frames to find the first caller that is not part of the 
    /// <see cref="LoggerImplementation"/> class. This ensures convenience methods like 
    /// <c>Info()</c> or <c>Debug()</c> do not appear as the source.
    /// </remarks>
    /// <returns>A string formatted as "ClassName.MethodName".</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private string GetCallingSource()
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            System.Reflection.MethodBase method = null;

            // Walk up the stack (starting at index 1 to skip this method)
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var tempMethod = frame?.GetMethod();
                var declaringType = tempMethod?.DeclaringType;

                // Keep walking until we find a class that ISN'T LoggerImplementation
                if (declaringType != typeof(LoggerImplementation) &&
                    declaringType != typeof(ISmartLogger))
                {
                    method = tempMethod;
                    break;
                }
            }

            if (method is null) return "UnknownSource";

            return $"{method.DeclaringType?.Name ?? "UnknownClass"}.{method.Name}";
        }
        catch
        {
            return "UnknownSource";
        }
    }
}