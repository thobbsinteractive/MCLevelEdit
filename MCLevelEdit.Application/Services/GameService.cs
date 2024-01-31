using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using System.ComponentModel;
using System.Diagnostics;

namespace MCLevelEdit.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IPackagePort _packagePort;

        public GameService(IPackagePort packagePort) 
        {
            _packagePort = packagePort;
        }

        public Task<bool> PackageLevelAsync(string[] levelFilePaths, string gameLevelsPath, string gameCloudLevelsPath)
        {
            return Task.Run(async () =>
            {
                bool success = false;
                if (Directory.Exists(gameLevelsPath))
                {
                    success = await _packagePort.PackageFilesAsync(levelFilePaths, gameLevelsPath);
                    if (!success)
                        return false;
                }
                if (Directory.Exists(gameCloudLevelsPath))
                {
                    success = await _packagePort.PackageFilesAsync(levelFilePaths, gameCloudLevelsPath);
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
    }
}
