namespace MCLevelEdit.Model.Abstractions;

public interface IGameService
{
    Task<bool> PackageLevelAsync(string[] levelFilePaths, string[] gameLevelsPaths);

    bool RunGame(string gamePath);
}
