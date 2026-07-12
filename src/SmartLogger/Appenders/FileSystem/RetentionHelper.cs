using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Appenders.FileSystem;

/// <summary>
/// Provides helper methods for cleaning up expired archived log files.
/// </summary>
internal static class RetentionHelper
{
    /// <summary>
    /// Deletes archived log files older than the configured retention period.
    /// </summary>
    /// <param name="configuration">
    /// File configuration containing archive and retention settings.
    /// </param>
    internal static void Cleanup(FileConfiguration configuration)
    {
        if (!configuration.Archive.Enabled)
            return;

        var archiveDirectory = configuration.Archive.Directory;

        if (!Directory.Exists(archiveDirectory))
            return;

        var retentionDate = DateTime.Now.AddDays(
            -configuration.Retention.RetentionDays);

        foreach (var file in Directory.GetFiles(archiveDirectory))
        {
            try
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.LastWriteTime < retentionDate)
                {
                    fileInfo.Delete();
                }
            }
            catch
            {
                // Ignore cleanup failures.
                // Logging should never fail because an old archive
                // could not be deleted.
            }
        }
    }
}