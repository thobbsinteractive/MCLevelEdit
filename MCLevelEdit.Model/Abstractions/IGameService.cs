namespace MCLevelEdit.Model.Abstractions;

public interface IGameService
{
    Task<bool> RunLevelFromSettings(string[] levelFilePaths);
    bool RunGame(string gamePath, string args);
    Task<bool> BackupLevelFiles(string gameLevelsPath, string gameLevelsBackupPath);
    Task<bool> RestoringLevelFiles(string gameLevelsBackupPath, string[] gameLevelsPaths);
    Task<int> PackageAsync(string[] filePaths, string outputPath);
    Task<int> UnpackAsync(string inputPath, string outputFolder);
}
