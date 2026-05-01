namespace SmartLogger.Appenders.FileRolling;

public interface IRollingStrategy
{
    bool ShouldRoll(string filePath);
    string GetNextFilePath(string basePath);
    void OnRoll(string currentFilePath);
}
