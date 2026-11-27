using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Utils;
using MCLevelEdit.Views;
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
    /// <summary>Default paths for one DOSBox install type (GOG or original).</summary>
    public record DosGameDefaults(string ExePath, string Args, string LevelsPath, string CloudLevelsPath, string BackupPath);

    /// <summary>
    /// Settings for a game run through DOSBox: Magic Carpet 1 and Hidden Worlds each get one,
    /// shown on their own tab of the Game Settings dialog.
    /// </summary>
    public class DosGameSettingsViewModel : ReactiveObject, IEnableLogger
    {
        private readonly IGameService _gameService;
        private readonly DosGameDefaults _gogDefaults;
        private readonly DosGameDefaults _classicDefaults;

        private bool _gameIsClassic = false;
        private string _gameExePath;
        private string _gameExeArgs;
        private string _gameLevelsPath;
        private string _gameCloudLevelsPath;
        private string _gameLevelsBackupPath;

        public string GameName { get; }

        public ICommand SetDefaultsCommand { get; }
        public ICommand SelectGameFolderCommand { get; }
        public ICommand SelectLevelsFolderCommand { get; }
        public ICommand SelectCloudLevelsFolderCommand { get; }
        public ICommand SelectBackupLevelsFolderCommand { get; }

        /// <summary>Raised after Set Defaults is confirmed, so the owner can save.</summary>
        public System.Func<Task<bool>> SaveRequested { get; set; }

        public TopLevel TopLevel => TopLevel.GetTopLevel(GameSettingsWindow.I);

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

        /// <summary>The packed levels file this game reads: LEVELS.DAT, or DDLEVELS.DAT for Hidden Worlds.</summary>
        public string LevelsFileName { get; }

        public string LevelsFolderLabel => $"{LevelsFileName} folder:";
        public string CloudLevelsFolderLabel => $"{LevelsFileName} (Cloud) folder:";

        public DosGameSettingsViewModel(string gameName, string levelsFileName, IGameService gameService, DosGameSettings? settings, DosGameDefaults gogDefaults, DosGameDefaults classicDefaults)
        {
            GameName = gameName;
            LevelsFileName = levelsFileName;
            _gameService = gameService;
            _gogDefaults = gogDefaults;
            _classicDefaults = classicDefaults;

            if (settings != null && !string.IsNullOrEmpty(settings.GameExeLocation))
            {
                Load(settings);
            }
            else
            {
                GameIsClassic = settings?.IsForClassic ?? false;
                SetDefaultPaths();
            }

            SelectGameFolderCommand = ReactiveCommand.CreateFromTask(SelectGameLauncher);
            SelectLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () => GameLevelsPath = await PickFolder("Select Levels Directory", GameLevelsPath) ?? GameLevelsPath);
            SelectCloudLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () => GameCloudLevelsPath = await PickFolder("Select Levels Directory", GameCloudLevelsPath) ?? GameCloudLevelsPath);
            SelectBackupLevelsFolderCommand = ReactiveCommand.CreateFromTask(async () => GameLevelsBackupPath = await PickFolder("Select Backup Directory for Levels", GameLevelsBackupPath) ?? GameLevelsBackupPath);

            SetDefaultsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Restore Defaults", $"Restoring defaults will clear your current {GameName} settings, Are you sure?", ButtonEnum.YesNo, Icon.Question);
                var result = await box.ShowAsPopupAsync(TopLevel);
                if (result == ButtonResult.Yes)
                {
                    SetDefaultPaths();
                    if (SaveRequested != null)
                        await SaveRequested();
                }
            });
        }

        private void Load(DosGameSettings settings)
        {
            GameIsClassic = settings.IsForClassic;
            GameExePath = settings.GameExeLocation;

            if (!string.IsNullOrEmpty(settings.GameArgs))
                GameExeArgs = settings.GameArgs;

            if (!string.IsNullOrEmpty(settings.GameBackupFolder))
                GameLevelsBackupPath = settings.GameBackupFolder;

            var folders = settings.GameLevelFolders ?? new string[0];
            if (folders.Length > 0)
                GameLevelsPath = folders[0];
            if (!settings.IsForClassic && folders.Length > 1)
                GameCloudLevelsPath = folders[1];
        }

        public DosGameSettings ToSettings() => new DosGameSettings
        {
            IsForClassic = GameIsClassic,
            GameExeLocation = GameExePath ?? string.Empty,
            GameArgs = GameExeArgs ?? string.Empty,
            GameBackupFolder = GameLevelsBackupPath ?? string.Empty,
            GameLevelFolders = string.IsNullOrEmpty(GameCloudLevelsPath)
                ? new string[] { GameLevelsPath ?? string.Empty }
                : new string[] { GameLevelsPath ?? string.Empty, GameCloudLevelsPath }
        };

        public void SetDefaultPaths()
        {
            var defaults = GameIsClassic ? _classicDefaults : _gogDefaults;
            GameExePath = defaults.ExePath;
            GameExeArgs = defaults.Args;
            GameLevelsPath = defaults.LevelsPath;
            GameCloudLevelsPath = defaults.CloudLevelsPath;
            GameLevelsBackupPath = defaults.BackupPath;
        }

        private async Task SelectGameLauncher()
        {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Select {GameName} Launcher",
                AllowMultiple = false,
                SuggestedStartLocation = await PickerStartFolder.GetAsync(TopLevel.StorageProvider, GameExePath)
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.LocalPath))
            {
                GameExePath = files[0].Path.LocalPath;
            }
        }

        private async Task<string?> PickFolder(string title, string? currentPath)
        {
            var folder = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = await PickerStartFolder.GetAsync(TopLevel.StorageProvider, currentPath, isFolder: true)
            });

            if (folder != null && folder.Count == 1 && Directory.Exists(folder[0].Path.LocalPath))
                return folder[0].Path.LocalPath;

            return null;
        }

        private async Task ShowError(string message)
        {
            this.Log().Error(message);
            var box = MessageBoxManager.GetMessageBoxStandard("Error", message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsPopupAsync(TopLevel);
        }

        public async Task<bool> CheckForGameLaunch(string[] levelPaths)
        {
            if (!(levelPaths?.Length > 0 && File.Exists(levelPaths[0])))
            {
                await ShowError("No Level selected to run! Please re-check!");
                return false;
            }

            if (!File.Exists(GameExePath))
            {
                await ShowError($"{GameName} Exe/lnk not found! Please re-check!");
                return false;
            }

            return await CheckGameLevelPaths();
        }

        public async Task<bool> CheckGameLevelPaths()
        {
            if (!Directory.Exists(GameLevelsPath))
            {
                await ShowError($"{GameName} LEVELS directory not found! Please re-check!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(GameLevelsBackupPath))
            {
                await ShowError($"{GameName} Levels Backup directory not set! Please re-check!");
                return false;
            }

            if (!GameIsClassic && !Directory.Exists(GameCloudLevelsPath))
            {
                await ShowError($"{GameName} LEVELS GOG cloud directory not found! Please re-check!");
                return false;
            }

            return true;
        }

        public async Task<bool> BackupLevels()
        {
            if (!await _gameService.BackupLevelFiles(GameLevelsPath, GameLevelsBackupPath, LevelsFileName))
            {
                this.Log().Error($"Error backing up {GameName} Levels!");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error backing up {GameName} Levels! Please check and validate your paths", ButtonEnum.Ok, Icon.Warning);
                await box.ShowAsPopupAsync(TopLevel);
                return false;
            }
            return true;
        }

        public async Task<bool> RestoreLevels()
        {
            var paths = GameIsClassic
                ? new string[] { GameLevelsPath }
                : new string[] { GameLevelsPath, GameCloudLevelsPath };

            if (!await _gameService.RestoringLevelFiles(GameLevelsBackupPath, paths, LevelsFileName))
            {
                await ShowError($"Error restoring your {GameName} Levels!");
                return false;
            }

            var box = MessageBoxManager.GetMessageBoxStandard("Success", $"Restore Success!", ButtonEnum.Ok, Icon.Info);
            await box.ShowAsPopupAsync(TopLevel);
            return true;
        }
    }
}
