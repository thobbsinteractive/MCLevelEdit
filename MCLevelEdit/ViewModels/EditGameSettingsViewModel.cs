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

        private const string GOG_ROOT = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\";
        private const string GOG_ARGS = @"-conf ""..\dosboxMC.conf"" -conf ""..\dosboxMC_single.conf"" -noconsole -c ""exit""";
        private const string CLASSIC_EXE = @"C:\Program Files (x86)\DOSBox-0.74-3\DOSBox.exe";
        private const string CLASSIC_ARGS = @"-conf ""C:\CARPET\dosboxMC.conf""";

        private static readonly DosGameDefaults Mc1GogDefaults = new(
            GOG_ROOT + @"DOSBOX\DOSBox.exe",
            GOG_ARGS,
            GOG_ROOT + @"CARPET.CD\LEVELS\",
            GOG_ROOT + @"cloud_saves\CARPET.CD\LEVELS\",
            GOG_ROOT + @"CARPET.CD\LEVELS\BACKUP\");

        private static readonly DosGameDefaults Mc1ClassicDefaults = new(
            CLASSIC_EXE,
            CLASSIC_ARGS,
            @"C:\CARPET\CARPET.CD\LEVELS\",
            string.Empty,
            @"C:\CARPET\CARPET.CD\LEVELS\BACKUP\");

        // Hidden Worlds ships in the same install as Magic Carpet; its levels live under NETHERW
        private static readonly DosGameDefaults HiddenWorldsGogDefaults = new(
            GOG_ROOT + @"DOSBOX\DOSBox.exe",
            GOG_ARGS,
            GOG_ROOT + @"CARPET.CD\NETHERW\LEVELS\",
            GOG_ROOT + @"cloud_saves\CARPET.CD\NETHERW\LEVELS\",
            GOG_ROOT + @"CARPET.CD\NETHERW\LEVELS\BACKUP\");

        private static readonly DosGameDefaults HiddenWorldsClassicDefaults = new(
            CLASSIC_EXE,
            CLASSIC_ARGS,
            @"C:\CARPET\CARPET.CD\NETHERW\LEVELS\",
            string.Empty,
            @"C:\CARPET\CARPET.CD\NETHERW\LEVELS\BACKUP\");

        // Tab order in the dialog
        private static readonly GameTarget[] TabGames = { GameTarget.MagicCarpet1, GameTarget.HiddenWorlds, GameTarget.MagicCarpet2 };

        private string[] _levelPaths;
        private string _mc2ExePath;
        private int _selectedGameTabIndex;

        public DosGameSettingsViewModel MagicCarpet1 { get; }
        public DosGameSettingsViewModel HiddenWorlds { get; }

        public ICommand RunLevelCommand { get; }
        public ICommand CheckLevelCommand { get; }
        public ICommand SelectLevelsCommand { get; }
        public ICommand SelectMc2ExeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand BackupCommand { get; }

        public TopLevel TopLevel => TopLevel.GetTopLevel(GameSettingsWindow.I);
        public string LevelPathsString => _levelPaths is not null ? string.Join(",", _levelPaths) : string.Empty;
        public bool CanRun => _levelPaths?.Length > 0;

        /// <summary>Tab 0 = Magic Carpet, 1 = Hidden Worlds (both DOSBox), 2 = Magic Carpet 2 (Remc2).</summary>
        public int SelectedGameTabIndex
        {
            get => _selectedGameTabIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedGameTabIndex, value);
                this.RaisePropertyChanged(nameof(IsMagicCarpet2));
                this.RaisePropertyChanged(nameof(IsDosGame));
                this.RaisePropertyChanged(nameof(CanRun));
            }
        }

        public GameTarget SelectedGame => TabGames[System.Math.Clamp(_selectedGameTabIndex, 0, TabGames.Length - 1)];
        public bool IsMagicCarpet2 => SelectedGame == GameTarget.MagicCarpet2;
        public bool IsDosGame => !IsMagicCarpet2;

        /// <summary>The DOSBox settings on the selected tab, or null for Magic Carpet 2.</summary>
        public DosGameSettingsViewModel? SelectedDosGame => SelectedGame switch
        {
            GameTarget.HiddenWorlds => HiddenWorlds,
            GameTarget.MagicCarpet2 => null,
            _ => MagicCarpet1
        };

        public string Mc2ExePath
        {
            get => _mc2ExePath;
            set => this.RaiseAndSetIfChanged(ref _mc2ExePath, value);
        }

        public string[] LevelPaths
        {
            get => _levelPaths;
            set => this.RaiseAndSetIfChanged(ref _levelPaths, value);
        }

        public EditGameSettingsViewModel(EventAggregator<object> eventAggregator, ISettingsPort settingsPort, IGameService gameService)
        {
            _eventAggregator = eventAggregator;
            _settingsPort = settingsPort;
            _gameService = gameService;

            var settings = _settingsPort.LoadSettings();

            LevelPaths = new string[] { _settingsPort.CurrentLevelFilePath };

            MagicCarpet1 = new DosGameSettingsViewModel("Magic Carpet", gameService, settings?.MagicCarpet1, Mc1GogDefaults, Mc1ClassicDefaults);
            HiddenWorlds = new DosGameSettingsViewModel("Hidden Worlds", gameService, settings?.HiddenWorlds, HiddenWorldsGogDefaults, HiddenWorldsClassicDefaults);
            MagicCarpet1.SaveRequested = SaveLevelPaths;
            HiddenWorlds.SaveRequested = SaveLevelPaths;

            if (settings != null)
            {
                SelectedGameTabIndex = System.Math.Max(0, System.Array.IndexOf(TabGames, settings.SelectedGame));
                Mc2ExePath = settings.Mc2ExeLocation;
            }

            SelectLevelsCommand = ReactiveCommand.CreateFromTask(SelectLevelFiles);
            SelectMc2ExeCommand = ReactiveCommand.CreateFromTask(SelectMc2Exe);

            RunLevelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dosGame = SelectedDosGame;
                if (dosGame is null)
                {
                    if (await CheckForMc2Launch() && await SaveLevelPaths())
                    {
                        if (!await _gameService.RunLevelFromSettings(LevelPaths))
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error launching Magic Carpet 2 HD!", ButtonEnum.Ok, Icon.Error);
                            await box.ShowAsPopupAsync(TopLevel);
                        }
                    }
                }
                else if (await dosGame.CheckForGameLaunch(LevelPaths) && await SaveLevelPaths() && await dosGame.BackupLevels())
                {
                    if (!await _gameService.RunLevelFromSettings(LevelPaths))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error packing level!", ButtonEnum.Ok, Icon.Error);
                        await box.ShowAsPopupAsync(TopLevel);
                    }
                }
            });

            CheckLevelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dosGame = SelectedDosGame;
                if (dosGame is null ? await CheckForMc2Launch() : await dosGame.CheckForGameLaunch(LevelPaths))
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
                var dosGame = SelectedDosGame;
                if (dosGame is not null && await dosGame.CheckGameLevelPaths() && await dosGame.BackupLevels())
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Success", $"Backup complete", ButtonEnum.Ok, Icon.Info);
                    await box.ShowAsPopupAsync(TopLevel);
                }
            });

            RestoreCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dosGame = SelectedDosGame;
                if (dosGame is not null)
                    await dosGame.RestoreLevels();
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
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Levels to Package and run",
                AllowMultiple = true
            });

            if (files != null)
            {
                SetLevelPaths(files.Select(f => f.Path.LocalPath).ToArray());
            }
        }

        private async Task SelectMc2Exe()
        {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Magic Carpet 2 HD executable",
                AllowMultiple = false
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.LocalPath))
            {
                Mc2ExePath = files[0].Path.LocalPath;
            }
        }

        private async Task<bool> CheckForMc2Launch()
        {
            string? error = null;
            var levelPath = LevelPaths?.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(Mc2ExePath) || !File.Exists(Mc2ExePath))
                error = "Magic Carpet 2 HD exe not found! Please re-check!";
            else if (string.IsNullOrWhiteSpace(levelPath) || !File.Exists(levelPath))
                error = "No Level selected to run! Please re-check!";
            else if (LevelFormat.DetectFromFileSize(new FileInfo(levelPath).Length) != GameVersion.MagicCarpet2)
                error = "The selected level is not a Magic Carpet 2 level! Please re-check!";
            else
                return true;

            this.Log().Error(error);
            var box = MessageBoxManager.GetMessageBoxStandard("Error", error, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsPopupAsync(TopLevel);
            return false;
        }

        private async Task<bool> SaveLevelPaths()
        {
            var settings = new Settings()
            {
                SelectedGame = SelectedGame,
                MagicCarpet1 = MagicCarpet1.ToSettings(),
                HiddenWorlds = HiddenWorlds.ToSettings(),
                Mc2ExeLocation = Mc2ExePath ?? string.Empty
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
    }
}
