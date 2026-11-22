namespace MCLevelEdit.Model.Abstractions;

public interface IGameService
{
    /// <param name="game">The game to run; the game selected in Game Settings when null.</param>
    Task<bool> RunLevelFromSettings(string[] levelFilePaths, Domain.GameTarget? game = null);
    bool RunGame(string gamePath, string args);
    /// <param name="levelsFileName">LEVELS.DAT for Magic Carpet, DDLEVELS.DAT for Hidden Worlds (see <see cref="Domain.LevelsFile"/>).</param>
    Task<bool> BackupLevelFiles(string gameLevelsPath, string gameLevelsBackupPath, string levelsFileName = Domain.LevelsFile.MAGIC_CARPET);
    Task<bool> RestoringLevelFiles(string gameLevelsBackupPath, string[] gameLevelsPaths, string levelsFileName = Domain.LevelsFile.MAGIC_CARPET);
    Task<int> PackageAsync(string[] filePaths, string outputPath);
    Task<int> UnpackAsync(string inputPath, string outputFolder);
}
