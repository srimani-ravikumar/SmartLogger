using System.IO;
using System.IO.Compression;

namespace SmartLogger.Appenders.FileSystem;

internal static class CompressionHelper
{
    /// <summary>
    /// Compresses the specified log file into a ZIP archive.
    /// </summary>
    /// <param name="archiveFilePath">
    /// Full path of the archived log file.
    /// </param>
    internal static void Compress(string archiveFilePath)
    {
        if (!File.Exists(archiveFilePath))
            return;

        var zipFilePath = Path.ChangeExtension(
            archiveFilePath,
            ".zip");

        // Overwrite existing archive if present
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        using (var archive = ZipFile.Open(
            zipFilePath,
            ZipArchiveMode.Create))
        {
            archive.CreateEntryFromFile(
                archiveFilePath,
                Path.GetFileName(archiveFilePath),
                CompressionLevel.Optimal);
        }

        // Remove original log after successful compression
        File.Delete(archiveFilePath);
    }
}
