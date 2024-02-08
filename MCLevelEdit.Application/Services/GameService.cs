using MagicCarpetLevelPackager;
using MagicCarpetLevelPackager.Abstractions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using System.ComponentModel;
using System.Diagnostics;

namespace MCLevelEdit.Application.Services
{
    public class GameService : IGameService
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

        public bool RunGame(string gamePath)
        {
            try
            {
                Console.WriteLine($"Trying to launch '{gamePath}'...");

                new Process
                {
                    StartInfo = new ProcessStartInfo(gamePath)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(gamePath)
                    }
                }.Start();
                return true;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> RunLevelFromSettings(string[] levelFilePaths)
        {
            var settings = _settingsPort.LoadSettings();

            var gameLevelsPaths = settings?.GameLevelFolders;
            var gameExeLocation = settings?.GameExeLocation;

            if (!string.IsNullOrWhiteSpace(gameExeLocation) && gameLevelsPaths is not null && gameLevelsPaths.Any())
            {
                try
                {
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

                        return RunGame(gameExeLocation);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine("Error running game: " + ex.Message);
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
                    Console.WriteLine("Error backing up files: " + ex.Message);
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
                    Console.WriteLine("Error restoring up files: " + ex.Message);
                }
            }
            return false;
        }
    }
}
