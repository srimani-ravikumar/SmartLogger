namespace SmartLogger.Core
{
    /// <summary>
    /// Defines a contract for formatting a <see cref="LogMessage"/>
    /// into a string representation based on a configurable pattern.
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Formats the specified <see cref="LogMessage"/> 
        /// into a string according to the configured pattern.
        /// </summary>
        /// <param name="message">The log message to format.</param>
        /// <returns>The formatted log string.</returns>
        string Format(LogMessage message);

        /// <summary>
        /// Sets the formatting pattern used to generate log output.
        /// </summary>
        /// <param name="pattern">
        /// The pattern string (e.g., "%date [%level] %message").
        /// </param>
        void SetPattern(string pattern);

        /// <summary>
        /// Gets the currently configured formatting pattern.
        /// </summary>
        /// <returns>The active pattern string.</returns>
        string GetPattern();

        /// <summary>
        /// Sets the date format used when rendering timestamps.
        /// </summary>
        /// <param name="dateFormat">
        /// The date format string (e.g., "yyyy-MM-dd HH:mm:ss").
        /// </param>
        void SetDateFormat(string dateFormat);
    }
}