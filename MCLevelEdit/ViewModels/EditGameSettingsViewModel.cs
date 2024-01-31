using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditGameSettingsViewModel : ReactiveObject
    {
        protected readonly EventAggregator<object> _eventAggregator;
        protected readonly IGameService _gameService;

        private string[] _levelPaths;
        private string _gamePath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\Launch Magic Carpet Plus.lnk";
        private string _gameLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\";
        private string _gameCloudLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\cloud_saves\CARPET.CD\LEVELS\";

        public ICommand RunLevelCommand { get; }
        public ICommand CheckLevelCommand { get; }

        public ICommand SelectLevelsCommand { get; }
        public ICommand SelectGameFolderCommand { get; }
        public ICommand SelectLevelsFolderCommand { get; }
        public ICommand SelectCloudLevelsFolderCommand { get; }

        public string LevelPathsString => _levelPaths is not null ? string.Join(",", _levelPaths) : string.Empty;
        public bool CanRun => _levelPaths?.Length > 0;

        public string[] LevelPaths
        {
            get => _levelPaths;
            set => this.RaiseAndSetIfChanged(ref _levelPaths, value);
        }

        public string GamePath
        {
            get => _gamePath;
            set => this.RaiseAndSetIfChanged(ref _gamePath, value);
        }

        public string GameLevelsPath
        {
            get => _gameLevelsPath;
            set => this.RaiseAndSetIfChanged(ref _gameLevelsPath, value);
        }

        public string GameCloudLevelsPath
        {
            get => _gameCloudLevelsPath;
            set => this.RaiseAndSetIfChanged(ref _gameCloudLevelsPath, value);
        }

        public EditGameSettingsViewModel(EventAggregator<object> eventAggregator, IGameService gameService)
        {
            _eventAggregator = eventAggregator;
            _gameService = gameService;

            SelectLevelsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectLevelFiles();
            });

            SelectGameFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectGameLauncher();
            });

            SelectLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectLevelsFolder();
            });

            SelectCloudLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectCloudLevelsFolder();
            });

            RunLevelCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                if (await CheckForGameLaunch())
                {
                    DeleteExistingFiles(GameLevelsPath);
                    DeleteExistingFiles(GameCloudLevelsPath);

                    if (await _gameService.PackageLevelAsync(LevelPaths, GameLevelsPath, GameCloudLevelsPath))
                    {
                        SetFilesToReadonly(GameLevelsPath);
                        SetFilesToReadonly(GameCloudLevelsPath);
                        _gameService.RunGame(GamePath);
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error packing level!", ButtonEnum.Ok, Icon.Error);
                        await box.ShowAsync();
                    }
                }
            });

            CheckLevelCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                await CheckForGameLaunch();
            });
        }

        public void SetLevelPaths(string[] levelPaths)
        {
            LevelPaths = levelPaths;
        
            this.RaisePropertyChanged(nameof(LevelPathsString));
            this.RaisePropertyChanged(nameof(CanRun));
        }

        private void DeleteExistingFiles(string folderPath)
        {
            string levelsdat = GetFilePath(folderPath, "LEVELS.DAT");
            if (levelsdat != null)
            {
                FileInfo fInfo = new FileInfo(levelsdat);
                if (fInfo.IsReadOnly)
                {
                    fInfo.IsReadOnly = false;
                }
                File.Delete(levelsdat);
            }
            string levelstab = GetFilePath(folderPath, "LEVELS.TAB");
            if (levelstab != null)
            {
                FileInfo fInfo = new FileInfo(levelstab);
                if (fInfo.IsReadOnly)
                {
                    fInfo.IsReadOnly = false;
                }
                File.Delete(levelstab);
            }
        }

        private void SetFilesToReadonly(string folderPath)
        {
            string levelsdat = GetFilePath(folderPath, "LEVELS.DAT");
            if (levelsdat != null)
            {
                FileInfo fInfo = new FileInfo(levelsdat);
                fInfo.IsReadOnly = true;
   
            }
            string levelstab = GetFilePath(folderPath, "LEVELS.TAB");
            if (levelstab != null)
            {
                FileInfo fInfo = new FileInfo(levelstab);
                fInfo.IsReadOnly = true;
            }
        }

        private async Task SelectLevelFiles()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(GameSettingsWindow.I);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Levels to Package and run",
                AllowMultiple = true
            });

            if (files != null)
            {
                LevelPaths = files.Select(f => f.Path.AbsolutePath).ToArray();
                SetLevelPaths(LevelPaths);
            }
        }

        private async Task SelectGameLauncher()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(GameSettingsWindow.I);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Magic Carpet Laucher",
                AllowMultiple = false
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.AbsolutePath))
            {
                GamePath = Path.GetDirectoryName(files[0].Path.AbsolutePath);
                AutoPopulateFolders(GameLevelsPath);
            }
        }

        private async Task SelectLevelsFolder()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(GameSettingsWindow.I);

            // Start async operation to open the dialog.
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Levels Directory",
                AllowMultiple = false
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.AbsolutePath))
            {
                GameLevelsPath = folder[0].Path.AbsolutePath;
            }
        }

        private async Task SelectCloudLevelsFolder()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(GameSettingsWindow.I);

            // Start async operation to open the dialog.
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Levels Directory",
                AllowMultiple = false
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.AbsolutePath))
            {
                GameCloudLevelsPath = folder[0].Path.AbsolutePath;
            }
        }

        private void AutoPopulateFolders(string gameFolder)
        {
            GamePath = GetFilePath(gameFolder, "Launch Magic Carpet Plus");
            var gameLevelsFolder = Path.Combine(gameFolder, @"CARPET.CD\LEVELS");
            if (GetFilePath(gameLevelsFolder, "LEVELS.DAT")?.Length > 0 && GetFilePath(gameLevelsFolder, "LEVELS.TAB")?.Length > 0)
            {
                GameLevelsPath = gameLevelsFolder;
            }
            var gameCloudLevelsFolder = Path.Combine(gameFolder, @"cloud_saves\CARPET.CD\LEVELS");
            if (GetFilePath(gameCloudLevelsFolder, "LEVELS.DAT")?.Length > 0 && GetFilePath(gameCloudLevelsFolder, "LEVELS.TAB")?.Length > 0)
            {
                GameCloudLevelsPath = gameCloudLevelsFolder;
            }
        }

        private string? GetFilePath(string gameFolder, string fileName)
        {
            gameFolder = Path.GetDirectoryName(gameFolder);
            if (Directory.Exists(gameFolder))
            {
                string[] files = Directory.GetFiles(gameFolder);
                return files?.Where(f => f.EndsWith(fileName, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            return null;
        }

        private string? GetFolderPath(string gameFolder, string folderName)
        {
            gameFolder = Path.GetDirectoryName(gameFolder);
            if (Directory.Exists(gameFolder) && gameFolder.EndsWith(folderName, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return gameFolder;
            }
            return null;
        }

        private async Task<bool> CheckForGameLaunch()
        {
            if (GetFilePath(GamePath, Path.GetFileName(GamePath))?.Length > 0)
            {
                if (GetFolderPath(GameLevelsPath, "LEVELS")?.Length > 0)
                {
                    if (GetFolderPath(GameCloudLevelsPath, "LEVELS")?.Length > 0)
                    {
                        return true;
                    }
                }
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Incorrect paths. Please re-check!", ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
            return false;
        }
    }
}
