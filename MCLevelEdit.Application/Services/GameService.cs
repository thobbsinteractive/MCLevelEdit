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

        public Task<bool> PackageLevelAsync(string[] levelFilePaths, string[] gameLevelsPaths)
        {
            return Task.Run(async () =>
            {
                bool success = false;
                foreach (string levelFilePath in levelFilePaths)
                {
                    if (Directory.Exists(levelFilePath))
                    {
                        success = await _packagePort.PackageFilesAsync(levelFilePaths, levelFilePath);
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
    }
}
