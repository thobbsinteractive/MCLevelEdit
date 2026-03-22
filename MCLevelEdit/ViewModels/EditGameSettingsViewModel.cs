using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Views;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditGameSettingsViewModel : ReactiveObject, IEnableLogger
    {
        protected readonly EventAggregator<object> _eventAggregator;
        protected readonly ISettingsPort _settingsPort;
        protected readonly IGameService _gameService;

        private string[] _levelPaths;
        private bool _gameIsClassic = false;

        private string _defaultClassicArgs = @"-conf ""C:\CARPET\dosboxMC.conf""";
        private string _defaultClassicExePath = @"C:\Program Files (x86)\DOSBox-0.74-3\DOSBox.exe";
        private string _defaultClassicLevelsPath = @"C:\CARPET\CARPET.CD\LEVELS\";
        private string _defaultClassicLevelsBackupPath = @"C:\CARPET\CARPET.CD\LEVELS\BACKUP\";

        private string _defaultGoGArgs = @"-conf ""..\dosboxMC.conf"" -conf ""..\dosboxMC_single.conf"" -noconsole -c ""exit""";
        private string _defaultGoGExePath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\DOSBOX\DOSBox.exe";
        private string _defaultGoGLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\";
        private string _defaultGoGLevelsBackupPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\BACKUP\";
        private string _defaultGoGCloudLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\cloud_saves\CARPET.CD\LEVELS\";

        private string _gameExePath;
        private string _gameExeArgs;
        private string _gameLevelsPath;
        private string _gameCloudLevelsPath;
        private string _gameLevelsBackupPath;

        public ICommand RunLevelCommand { get; }
        public ICommand CheckLevelCommand { get; }
        public ICommand SetDefaultsCommand { get; }

        public ICommand SelectLevelsCommand { get; }
        public ICommand SelectGameFolderCommand { get; }
        public ICommand SelectLevelsFolderCommand { get; }
        public ICommand SelectCloudLevelsFolderCommand { get; }
        public ICommand SelectBackupLevelsFolderCommand { get; }

        public ICommand SaveCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand BackupCommand { get; }

        public TopLevel TopLevel => TopLevel.GetTopLevel(GameSettingsWindow.I);
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

        public string GameExeArgs
        {
            get => _gameExeArgs;
            set => this.RaiseAndSetIfChanged(ref _gameExeArgs, value);
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

        public string GameLevelsBackupPath
        {
            get => _gameLevelsBackupPath;
            set => this.RaiseAndSetIfChanged(ref _gameLevelsBackupPath, value);
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

                var gameArgs = settings.GameArgs;

                if (!string.IsNullOrEmpty(gameArgs))
                {
                    GameExeArgs = gameArgs;
                }

                var gameLevelsBackupPath = settings.GameBackupFolder;
                if (!string.IsNullOrEmpty(gameLevelsBackupPath))
                {
                    GameLevelsBackupPath = gameLevelsBackupPath;
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
            else
            {
                SetDefaultPaths();
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

            SelectBackupLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectBackupLevelsFolder();
            });

            RunLevelCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                if (await CheckForGameLaunch() && await SaveLevelPaths() && await BackupLevels())
                {
                    if (!await _gameService.RunLevelFromSettings(LevelPaths))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error packing level!", ButtonEnum.Ok, Icon.Error);
                        await box.ShowAsPopupAsync(TopLevel);
                    }
                }
            });

            CheckLevelCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                if (await CheckForGameLaunch())
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Success", $"All paths correct", ButtonEnum.Ok, Icon.Info);
                    await box.ShowAsPopupAsync(TopLevel);
                }

            });

            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await SaveLevelPaths())
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Success", $"Save complete", ButtonEnum.Ok, Icon.Info);
                    await box.ShowAsPopupAsync(TopLevel);
                }
            });

            BackupCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await CheckGameLevelPaths())
                {
                    if (await BackupLevels())
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Success", $"Backup complete", ButtonEnum.Ok, Icon.Info);
                        await box.ShowAsPopupAsync(TopLevel);
                    }
                }
            });

            RestoreCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await RestoreLevels();
            });

            SetDefaultsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Restore Defaults", $"Restoring defaults will clear your current settings, Are you sure?", ButtonEnum.YesNo, Icon.Question);
                var result = await box.ShowAsPopupAsync(TopLevel);
                if (result == ButtonResult.Yes)
                {
                    SetDefaultPaths();
                    await SaveLevelPaths();
                }
            });
        }

        public void SetLevelPaths(string[] levelPaths)
        {
            LevelPaths = levelPaths;
        
            this.RaisePropertyChanged(nameof(LevelPathsString));
            this.RaisePropertyChanged(nameof(CanRun));
        }

        private void SetDefaultPaths()
        {
            if (this.GameIsClassic)
            {
                GameExePath = _defaultClassicExePath;
                GameExeArgs = _defaultClassicArgs;
                GameLevelsPath = _defaultClassicLevelsPath;
                GameCloudLevelsPath = string.Empty;
                GameLevelsBackupPath = _defaultClassicLevelsBackupPath;
            }
            else
            {
                GameExePath = _defaultGoGExePath;
                GameExeArgs = _defaultGoGArgs;
                GameLevelsPath = _defaultGoGLevelsPath;
                GameCloudLevelsPath = _defaultGoGCloudLevelsPath;
                GameLevelsBackupPath = _defaultGoGLevelsBackupPath;
            }
        }

        private async Task SelectLevelFiles()
        {
            // Start async operation to open the dialog.
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Levels to Package and run",
                AllowMultiple = true
            });

            if (files != null)
            {
                LevelPaths = files.Select(f => f.Path.LocalPath).ToArray();
                SetLevelPaths(LevelPaths);
            }
        }

        private async Task SelectGameLauncher()
        {
            // Start async operation to open the dialog.
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Magic Carpet Laucher",
                AllowMultiple = false
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.LocalPath))
            {
                GameExePath = files[0].Path.LocalPath;
            }
        }

        private async Task SelectLevelsFolder()
        {
            // Start async operation to open the dialog.
            var folder = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Levels Directory",
                AllowMultiple = false
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.LocalPath))
            {
                GameLevelsPath = folder[0].Path.LocalPath;
            }
        }

        private async Task SelectCloudLevelsFolder()
        {
            // Start async operation to open the dialog.
            var folder = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Levels Directory",
                AllowMultiple = false
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.LocalPath))
            {
                GameCloudLevelsPath = folder[0].Path.LocalPath;
            }
        }

        private async Task SelectBackupLevelsFolder()
        {
            // Start async operation to open the dialog.
            var folder = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Directory for Levels",
                AllowMultiple = false
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.LocalPath))
            {
                GameLevelsBackupPath = folder[0].Path.LocalPath;
            }
        }

        private async Task<bool> CheckForGameLaunch()
        {
            if (_levelPaths?.Length > 0 && File.Exists(LevelPaths[0]))
            {
                if (File.Exists(GameExePath))
                {
                    return await CheckGameLevelPaths();
                }
                else
                {
                    this.Log().Error($"Game Exe/lnk not found! Please re-check!");
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Game Exe/lnk not found! Please re-check!", ButtonEnum.Ok, Icon.Error);
                    await box.ShowAsPopupAsync(TopLevel);
                }
            }
            else
            {
                this.Log().Error($"No Level selected to run! Please re-check!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"No Level selected to run! Please re-check!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsPopupAsync(TopLevel);
            }
            return false;
        }

        private async Task<bool> CheckGameLevelPaths()
        {
            if (Directory.Exists(GameLevelsPath))
            {
                if (!string.IsNullOrWhiteSpace(GameLevelsBackupPath))
                {
                    if (!GameIsClassic)
                    {
                        if (Directory.Exists(GameCloudLevelsPath))
                        {
                            return true;
                        }
                        else
                        {
                            this.Log().Error($"LEVELS GOG cloud directory not found! Please re-check!");
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"LEVELS GOG cloud directory not found! Please re-check!", ButtonEnum.Ok, Icon.Error);
                            await box.ShowAsPopupAsync(TopLevel);
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    this.Log().Error($"Game Levels Backup directory not set! Please re-check!");
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Game Levels Backup directory not set! Please re-check!", ButtonEnum.Ok, Icon.Error);
                    await box.ShowAsPopupAsync(TopLevel);
                }
            }
            else
            {
                this.Log().Error($"LEVELS directory not found! Please re-check!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"LEVELS directory not found! Please re-check!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsPopupAsync(TopLevel);
            }

            return false;
        }

        private async Task<bool> SaveLevelPaths()
        {
            var settings = new Settings()
            {
                IsForClassic = this.GameIsClassic,
                GameExeLocation = this.GameExePath,
                GameArgs = this.GameExeArgs,
                GameBackupFolder = this.GameLevelsBackupPath,
                GameLevelFolders = string.IsNullOrEmpty(this.GameCloudLevelsPath) ? new string[] { this.GameLevelsPath } : new string[] { this.GameLevelsPath, this.GameCloudLevelsPath }
            };

            if (!_settingsPort.SaveSettings(settings))
            {
                this.Log().Error($"Error saving settings!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error saving settings! Do you want to continue?", ButtonEnum.OkCancel, Icon.Warning);
                var result = await box.ShowAsPopupAsync(TopLevel);
                return result == ButtonResult.Ok;
            }
            return true;
        }

        private async Task<bool> BackupLevels()
        {
            if (!await _gameService.BackupLevelFiles(this.GameLevelsPath, this.GameLevelsBackupPath))
            {
                this.Log().Error($"Error backing up Levels!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error backing up Levels! Please check and validate your paths", ButtonEnum.Ok, Icon.Warning);
                var result = await box.ShowAsPopupAsync(TopLevel);
                return false;
            }
            return true;
        }

        private async Task<bool> RestoreLevels()
        {
            var paths = new string[] { this.GameLevelsPath };

            if (!GameIsClassic)
            {
                paths = new string[] { this.GameLevelsPath, this.GameCloudLevelsPath };
            }

            if (!await _gameService.RestoringLevelFiles(this.GameLevelsBackupPath, paths))
            {
                this.Log().Error($"Error restoring you game files Levels!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error restoring you game files Levels!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsPopupAsync(TopLevel);
                return false;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Success", $"Restore Success!", ButtonEnum.Ok, Icon.Info);
                await box.ShowAsPopupAsync(TopLevel);
            }
            return true;
        }
    }
}
