namespace MCLevelEdit.Model.Abstractions;

public interface IGameService
{
    Task<bool> PackageLevelAsync(string[] levelFilePaths, string[] gameLevelsPaths);
    Task<bool> RunLevelFromSettings(string[] levelFilePaths);
    bool RunGame(string gamePath);
    Task<bool> BackupLevelFiles(string[] gameLevelsPaths);
    Task<bool> RestoringLevelFiles(string[] gameLevelsPaths);
}
