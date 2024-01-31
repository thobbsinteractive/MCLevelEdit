namespace MCLevelEdit.Model.Abstractions;

public interface IGameService
{
    Task<bool> PackageLevelAsync(string[] levelFilePaths, string gameLevelsPath, string gameCloudLevelsPath);

    bool RunGame(string gamePath);
}
