namespace SmartLogger.Core
{
    /// <summary>
    /// Defines a contract for formatting a <see cref="LogMessage"/>
    /// into a string representation based on a configurable pattern.
    /// </summary>
    public interface ILogOutputFormatterStrategy
    {
        /// <summary>
        /// Formats the specified <see cref="LogMessage"/> 
        /// into a string according to the configured pattern.
        /// </summary>
        /// <param name="message">The log message to format.</param>
        /// <returns>The formatted log string.</returns>
        string Format(LogMessage message);       
    }
}