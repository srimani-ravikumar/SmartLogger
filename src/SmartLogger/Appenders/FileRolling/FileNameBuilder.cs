using SmartLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartLogger.Appenders.FileRolling;

/// <summary>
/// Responsible for constructing log file names based on the provided <see cref="FileConfiguration"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class encapsulates the naming strategy for rolling log files.
/// It combines static configuration (base path, extension) with runtime values (date, index)
/// to produce a deterministic file path.
/// </para>
/// 
/// <para>
/// Design Intent:
/// - Keep naming logic isolated from rolling logic.
/// - Ensure consistency across all file appenders.
/// - Allow flexible naming via configuration without modifying core logic.
/// </para>
/// 
/// <para>
/// Example Output:
/// <c>app-log_2026-05-02_1.log</c>
/// </para>
/// </remarks>
internal sealed class FileNameBuilder
{
    /// <summary>
    /// Configuration used to determine how the file name should be constructed.
    /// </summary>
    private readonly FileConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNameBuilder"/> class.
    /// </summary>
    /// <param name="config">The file configuration containing naming rules and base path.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public FileNameBuilder(FileConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Builds the full file path for the log file based on configuration and runtime state.
    /// </summary>
    /// <param name="index">
    /// Optional rolling index used to differentiate multiple files within the same time period.
    /// Only applied when index-based naming is enabled and index is greater than zero.
    /// </param>
    /// <returns>
    /// A fully qualified file path including directory, file name, and extension.
    /// </returns>
    /// <remarks>
    /// The file name is composed in the following order:
    /// <list type="number">
    /// <item><description>Base file name (without directory)</description></item>
    /// <item><description>Date (if enabled in configuration)</description></item>
    /// <item><description>Index (if enabled and greater than zero)</description></item>
    /// </list>
    /// 
    /// All parts are joined using the configured separator.
    /// </remarks>
    public string Build(int index = 0)
    {
        var parts = new List<string>();

        // Extract only the file name (without directory) from the base path
        parts.Add(Path.GetFileName(_config.BasePath));

        // Append date component if enabled in configuration
        if (_config.Naming.IncludeDate)
        {
            // Uses configured date format (e.g., yyyy-MM-dd)
            var date = DateTime.Now.ToString(_config.Naming.DateFormat);
            parts.Add(date);
        }

        // Append index only when:
        // 1. Index-based naming is enabled
        // 2. Index is greater than zero (avoid unnecessary "_0")
        if (_config.Naming.IncludeIndex && index > 0)
        {
            parts.Add(index.ToString());
        }

        // Join all parts using configured separator (e.g., "_", "-")
        var fileName = string.Join(_config.Naming.Separator, parts);

        // Combine directory path with constructed file name and extension
        var fullPath = Path.Combine(
            Path.GetDirectoryName(_config.BasePath)!,
            $"{fileName}.{_config.Extension}"
        );

        return fullPath;
    }
}