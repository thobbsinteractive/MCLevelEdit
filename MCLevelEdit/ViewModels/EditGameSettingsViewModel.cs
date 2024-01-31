using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Views;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MCLevelEdit.Model.Abstractions;

namespace MCLevelEdit.ViewModels
{
    public class EditGameSettingsViewModel : ReactiveObject
    {
        protected readonly EventAggregator<object> _eventAggregator;

        private string _gamePath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\Launch Magic Carpet Plus.lnk";
        private string _gameLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\";
        private string _gameCloudLevelsPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\cloud_saves\CARPET.CD\LEVELS\";

        public ICommand RunLevelCommand { get; }
        public ICommand CheckLevelCommand { get; }
        public ICommand SelectGameFolderCommand { get; }
        public ICommand SelectLevelsFolderCommand { get; }
        public ICommand SelectCloudLevelsFolderCommand { get; }
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
                if (!await CheckForGameLaunch())
                {

                }
            });

            CheckLevelCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                await CheckForGameLaunch();
            });
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

        private async Task<bool> CheckForGameLaunch()
        {
            if (GetFilePath(GamePath, "Launch Magic Carpet Plus.lnk")?.Length > 0)
            {
                if (GetFilePath(GameLevelsPath, "LEVELS.DAT")?.Length > 0 && GetFilePath(GameLevelsPath, "LEVELS.TAB")?.Length > 0)
                {
                    if (GetFilePath(GameCloudLevelsPath, "LEVELS.DAT")?.Length > 0 && GetFilePath(GameCloudLevelsPath, "LEVELS.TAB")?.Length > 0)
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
