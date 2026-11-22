using MagicCarpetLevelPackager;
using MagicCarpetLevelPackager.Abstractions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using RncProPackDotNet;
using Serilog;
using Serilog.Extensions.Logging;
using Splat;
using System.ComponentModel;
using System.Diagnostics;

namespace MCLevelEdit.Application.Services
{
    public class GameService : IGameService, IEnableLogger
    {
        private readonly IPackagePort _packagePort;
        private readonly ISettingsPort _settingsPort;

        public GameService(ISettingsPort settingsPort) 
        {
            _settingsPort = settingsPort;
            _packagePort = new MagicCarpetPackageAdapter();
        }

        public bool RunGame(string gamePath, string args)
        {
            try
            {
                this.Log().Info($"Trying to launch '{gamePath}'...");

                new Process
                {
                    StartInfo = new ProcessStartInfo(gamePath)
                    {
                        Arguments = args,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(gamePath)
                    }
                }.Start();
                return true;
            }
            catch (Win32Exception ex)
            {
                this.Log().Error(ex, $"Error running level:\n{ex.Message}");
                return false;
            }
        }

        public async Task<bool> RunLevelFromSettings(string[] levelFilePaths, GameTarget? game = null)
        {
            var settings = _settingsPort.LoadSettings();
            if (settings is null)
                return false;

            // The level's own game when given (the main window's Run), else the game selected in Game Settings
            var target = game ?? settings.SelectedGame;

            if (target == GameTarget.MagicCarpet2)
            {
                if (string.IsNullOrWhiteSpace(settings.Mc2ExeLocation) || !File.Exists(settings.Mc2ExeLocation))
                {
                    this.Log().Error($"Magic Carpet 2 HD exe not found: '{settings.Mc2ExeLocation}'");
                    return false;
                }
                // Magic Carpet 2 HD runs a single uncompressed level directly: --custom_level "level path"
                var levelPath = levelFilePaths?.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(levelPath) || !File.Exists(levelPath))
                {
                    this.Log().Error($"Level file not found: '{levelPath}'");
                    return false;
                }
                if (LevelFormat.DetectFromFileSize(new FileInfo(levelPath).Length) != GameVersion.MagicCarpet2)
                {
                    this.Log().Error($"'{levelPath}' is not a Magic Carpet 2 level ({LevelFormat.MC2_FILE_SIZE} bytes).");
                    return false;
                }
                return RunGame(settings.Mc2ExeLocation, $"--custom_level \"{Path.GetFullPath(levelPath)}\"");
            }

            // Magic Carpet 1 or Hidden Worlds, both run through DOSBox
            var notMc1 = levelFilePaths?.FirstOrDefault(p => !File.Exists(p) || LevelFormat.DetectFromFileSize(new FileInfo(p).Length) != GameVersion.MagicCarpet1);
            if (levelFilePaths is null || levelFilePaths.Length == 0 || notMc1 is not null)
            {
                // Never pack a Magic Carpet 2 level into the game's LEVELS.DAT / DDLEVELS.DAT
                this.Log().Error($"'{notMc1}' is not a Magic Carpet level ({LevelFormat.MC1_FILE_SIZE} bytes).");
                return false;
            }

            var dosSettings = settings.GetDosSettings(target);
            var gameLevelsPaths = dosSettings?.GameLevelFolders;
            var gameExeLocation = dosSettings?.GameExeLocation;
            var gameArgs = dosSettings?.GameArgs;
            var gameLevelsBackupPath = dosSettings?.GameBackupFolder;
            // Hidden Worlds reads DDLEVELS.DAT, Magic Carpet LEVELS.DAT, from the same LEVELS folder
            var levelsFileName = LevelsFile.For(target);

            if (!string.IsNullOrWhiteSpace(gameExeLocation) && gameLevelsPaths is not null && gameLevelsPaths.Any())
            {
                try
                {
                    await BackupLevelFiles(gameLevelsPaths[0], gameLevelsBackupPath, levelsFileName);

                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        FileUtils.DeleteExistingFiles(gameLevelsPath, levelsFileName);
                    }

                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        var result = await PackageAsync(levelFilePaths, Path.Combine(gameLevelsPath, levelsFileName));
                        FileUtils.SetFilesToReadonly(gameLevelsPath, levelsFileName);
                    }
                    return RunGame(gameExeLocation, gameArgs);
                    
                } 
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error runing level from settings:\n{ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> BackupLevelFiles(string gameLevelsPath, string gameLevelsBackupPath, string levelsFileName = LevelsFile.MAGIC_CARPET)
        {
            if (gameLevelsPath is not null)
            {
                try
                {
                    return FileUtils.CopyBackupFiles(gameLevelsPath, gameLevelsBackupPath, levelsFileName);
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error backing up files:\n{ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> RestoringLevelFiles(string gameLevelsBackupPath, string[] gameLevelsPaths, string levelsFileName = LevelsFile.MAGIC_CARPET)
        {
            if (gameLevelsPaths is not null && gameLevelsPaths.Any())
            {
                try
                {
                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        FileUtils.DeleteExistingFiles(gameLevelsPath, levelsFileName);

                        if (!FileUtils.RestoreBackupFiles(gameLevelsPath, gameLevelsBackupPath, levelsFileName))
                            return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error restoring up files:\n{ex.Message}");
                }
            }
            return false;
        }

        public Task<int> UnpackAsync(string inputPath, string outputFolder)
        {
            try
            {
                uint MAX_BUF_SIZE = 0x1E00000;
                var microsoftLogger = new SerilogLoggerFactory(Log.Logger).CreateLogger("GameService");
                var rncProPack = new RncProPackDotNet.RncProPack(microsoftLogger);
                var vars = rncProPack.InitVars();

                if (vars.Method == 1)
                {
                    if (vars.DictSize > 0x8000)
                        vars.DictSize = 0x8000;
                    vars.MaxMatches = 0x1000;
                }
                else if (vars.Method == 2)
                {
                    if (vars.DictSize > 0x1000)
                        vars.DictSize = 0x1000;
                    vars.MaxMatches = 0xFF;
                }

                using (FileStream inFile = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                {
                    vars.FileSize = (uint)(inFile.Length - vars.ReadStartOffset);
                    inFile.Seek(vars.ReadStartOffset, SeekOrigin.Begin);
                    vars.Input = new byte[vars.FileSize];
                    inFile.Read(vars.Input, 0, (int)vars.FileSize);
                }

                vars.Output = new byte[MAX_BUF_SIZE];
                vars.Temp = new byte[MAX_BUF_SIZE];

                vars.FileSize = RncUtils.FindEndOfRncData(vars.Input);

                return Task.Run(() =>
                {
                    var result = rncProPack.DoSearch(ref vars, vars.FileSize, true, outputFolder);
                    if (result == 0)
                        RncUtils.NumberExtractedFiles(outputFolder);
                    return result;
                });
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"Error unpacking Levels:\n{ex.Message}");
                return Task.FromResult(-1);
            }
        }

        public Task<int> PackageAsync(string[] filePaths, string outputPath)
        {
            try
            {
                // Every level in a package must be from the same game; the level size tells them apart
                var levelSizes = filePaths.Select(f => new FileInfo(f).Length).Distinct().ToArray();
                if (levelSizes.Length != 1 || LevelFormat.DetectFromFileSize(levelSizes[0]) is null)
                {
                    this.Log().Error($"Cannot package levels: all files must be uncompressed levels from the same game " +
                        $"({LevelFormat.MC1_FILE_SIZE} bytes for Magic Carpet 1, {LevelFormat.MC2_FILE_SIZE} bytes for Magic Carpet 2).");
                    return Task.FromResult(-1);
                }
                int levelSize = (int)levelSizes[0];

                var microsoftLogger = new SerilogLoggerFactory(Log.Logger).CreateLogger("GameService");
                var rncProPack = new RncProPackDotNet.RncProPack(microsoftLogger);
                var vars = rncProPack.InitVars();
                vars.Output = new byte[0x1E00000];
                vars.Temp = new byte[0x1E00000];

                return Task.Run(() =>
                {
                    return rncProPack.DoPackAndPackageBullfrogFilesToDatandTab(ref vars, filePaths, levelSize, true, outputPath);
                });
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"Error Packing:\n{ex.Message}");
                return Task.FromResult(-1);
            }
        }
    }
}
