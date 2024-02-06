using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
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
        protected readonly ISettingsPort _settingsPort;
        protected readonly IGameService _gameService;

        private string[] _levelPaths;
        private bool _gameIsClassic = false;

        private string _gameExePath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\Launch Magic Carpet Plus.lnk";
        private string _gameLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\";
        private string _gameCloudLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\cloud_saves\CARPET.CD\LEVELS\";

        public ICommand RunLevelCommand { get; }
        public ICommand CheckLevelCommand { get; }

        public ICommand SelectLevelsCommand { get; }
        public ICommand SelectGameFolderCommand { get; }
        public ICommand SelectLevelsFolderCommand { get; }
        public ICommand SelectCloudLevelsFolderCommand { get; }

        public ICommand SaveCommand { get; }

        public string LevelPathsString => _levelPaths is not null ? string.Join(",", _levelPaths) : string.Empty;
        public bool CanRun => _levelPaths?.Length > 0;

        public string[] LevelPaths
        {
            get => _levelPaths;
            set => this.RaiseAndSetIfChanged(ref _levelPaths, value);
        }

        public bool GameIsClassic
        {
            get => _gameIsClassic;
            set => this.RaiseAndSetIfChanged(ref _gameIsClassic, value);
        }

        public string GameExePath
        {
            get => _gameExePath;
            set => this.RaiseAndSetIfChanged(ref _gameExePath, value);
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

        public EditGameSettingsViewModel(EventAggregator<object> eventAggregator, ISettingsPort settingsPort, IGameService gameService)
        {
            _eventAggregator = eventAggregator;
            _settingsPort = settingsPort;
            _gameService = gameService;

            var settings = _settingsPort.LoadSettings();

            LevelPaths = new string[] { _settingsPort.CurrentLevelFilePath };

            if (settings != null)
            {
                var gameExePath = settings.GameExeLocation;

                GameIsClassic = settings.IsForClassic;

                if (!string.IsNullOrEmpty(gameExePath))
                {
                    GameExePath = gameExePath;
                }

                var gameLevelFolders = settings.GameLevelFolders;

                if (gameLevelFolders.Any())
                {
                    if (gameLevelFolders.Length > 0)
                        GameLevelsPath = gameLevelFolders[0];

                    if (!settings.IsForClassic && gameLevelFolders.Length > 1)
                        GameCloudLevelsPath = gameLevelFolders[1];
                }
            }

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
                if (await CheckForGameLaunch() && SaveLevelPaths())
                {
                    if (!await _gameService.RunLevelFromSettings(LevelPaths))
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

            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                return SaveLevelPaths();
            });
        }

        public void SetLevelPaths(string[] levelPaths)
        {
            LevelPaths = levelPaths;
        
            this.RaisePropertyChanged(nameof(LevelPathsString));
            this.RaisePropertyChanged(nameof(CanRun));
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
                GameExePath = Path.GetDirectoryName(files[0].Path.AbsolutePath);
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
            GameExePath = GetFilePath(gameFolder, "Launch Magic Carpet Plus.lnk");
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
            if (_levelPaths?.Length > 0 && File.Exists(LevelPaths[0]))
            {
                if (GetFilePath(GameExePath, Path.GetFileName(GameExePath))?.Length > 0)
                {
                    if (GetFolderPath(GameLevelsPath, "LEVELS")?.Length > 0)
                    {
                        if (GetFolderPath(GameCloudLevelsPath, "LEVELS")?.Length > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Incorrect paths. Please re-check!", ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
            return false;
        }

        private bool SaveLevelPaths()
        {
            var settings = new Settings()
            {
                IsForClassic = this.GameIsClassic,
                GameExeLocation = this.GameExePath,
                GameLevelFolders = new string[] { this.GameLevelsPath, this.GameCloudLevelsPath }
            };

            return _settingsPort.SaveSettings(settings);
        }
    }
}
