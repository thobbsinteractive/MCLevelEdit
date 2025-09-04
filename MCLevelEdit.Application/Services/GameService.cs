using MagicCarpetLevelPackager;
using MagicCarpetLevelPackager.Abstractions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
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
  
                    foreach (var gameLevelsPath in gameLevelsPaths)
                    {
                        var result = await PackageAsync(levelFilePaths, Path.Combine(gameLevelsPath, "LEVELS.DAT"));
                        FileUtils.SetFilesToReadonly(gameLevelsPath);
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

                return Task.Run(() =>
                {
                    return rncProPack.DoSearch(ref vars, vars.FileSize, true, outputFolder);
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
                var microsoftLogger = new SerilogLoggerFactory(Log.Logger).CreateLogger("GameService");
                var rncProPack = new RncProPackDotNet.RncProPack(microsoftLogger);
                var vars = rncProPack.InitVars();
                vars.Output = new byte[0x1E00000];
                vars.Temp = new byte[0x1E00000];

                return Task.Run(() =>
                {
                    return rncProPack.DoPackAndPackageBullfrogFilesToDatandTab(ref vars, filePaths, 38812, true, outputPath);
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
