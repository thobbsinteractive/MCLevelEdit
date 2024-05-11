using MagicCarpetLevelPackager;
using MagicCarpetLevelPackager.Abstractions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
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

        public Task<bool> PackageLevelAsync(string[] levelFilePaths, string[] gameLevelsPaths)
        {
            try
            {
                return Task.Run(async () =>
                {
                    bool success = false;
                    foreach (string gameLevelPath in gameLevelsPaths)
                    {
                        if (Directory.Exists(gameLevelPath))
                        {
                            success = await _packagePort.PackageFilesAsync(levelFilePaths, gameLevelPath);
                            if (!success)
                                return false;
                        }
                    }
                    return success;
                });
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"Error Packing Level:\n{ex.Message}");
                return Task.FromResult(false);
            }
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

        public async Task<bool> RunLevelFromSettings(string[] levelFilePaths)
        {
            var settings = _settingsPort.LoadSettings();

            var gameLevelsPaths = settings?.GameLevelFolders;
            var gameExeLocation = settings?.GameExeLocation;
            var gameArgs = settings?.GameArgs;
            var gameLevelsBackupPath = settings?.GameBackupFolder;

            if (!string.IsNullOrWhiteSpace(gameExeLocation) && gameLevelsPaths is not null && gameLevelsPaths.Any())
            {
                try
                {
                    await BackupLevelFiles(gameLevelsPaths[0], gameLevelsBackupPath);

                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        FileUtils.DeleteExistingFiles(gameLevelsPath);
                    }

                    if (await PackageLevelAsync(levelFilePaths, gameLevelsPaths))
                    {
                        foreach (var gameLevelsPath in gameLevelsPaths)
                        {
                            FileUtils.SetFilesToReadonly(gameLevelsPath);
                        }

                        return RunGame(gameExeLocation, gameArgs);
                    }
                } 
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error runing level from settings:\n{ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> BackupLevelFiles(string gameLevelsPath, string gameLevelsBackupPath)
        { 
            if (gameLevelsPath is not null)
            {
                try
                {
                    return FileUtils.CopyBackupFiles(gameLevelsPath, gameLevelsBackupPath);
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error backing up files:\n{ex.Message}");
                }
            }
            return false;
        }

        public async Task<bool> RestoringLevelFiles(string gameLevelsBackupPath, string[] gameLevelsPaths)
        {
            if (gameLevelsPaths is not null && gameLevelsPaths.Any())
            {
                try
                {
                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        FileUtils.DeleteExistingFiles(gameLevelsPath);

                        if (!FileUtils.RestoreBackupFiles(gameLevelsPath, gameLevelsBackupPath))
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
    }
}
