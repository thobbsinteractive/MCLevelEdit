using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class PackageFilesViewModel : ReactiveObject, IEnableLogger
    {
        private bool _canPack = true;
        private IGameService _gameService;
        public ICommand PackageCommand { get; }
        public ICommand SelectFilesCommand { get; }
        public ICommand SelectOutputPathCommand { get; }
        public AvaloniaList<string> FilesList { get; init; }
        public string OutputPath { get; set; }

        public bool CanPack
        {
            get => _canPack;
            set => this.RaiseAndSetIfChanged(ref _canPack, value);
        }

        public PackageFilesViewModel(EventAggregator<object> eventAggregator, IGameService gameService)
        {
            _gameService = gameService;
            FilesList = new AvaloniaList<string>();

            OutputPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Globals.APP_DIRECTORY, "LEVELS.DAT"));

            PackageCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    CanPack = false;

                    if (!Directory.Exists(OutputPath))
                    {
                        Directory.CreateDirectory(OutputPath);
                    }

                    var errorCode = PackageFiles(FilesList.ToArray(), OutputPath);
                    if (errorCode != 0)
                        throw new Exception($"Unknown Error, code {errorCode}");
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error unpacking files!");
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error unpacking Levels! Please check and validate your paths: {ex.Message}", ButtonEnum.Ok, Icon.Warning);
                    await box.ShowAsync();
                }
                finally
                {
                    CanPack = true;
                }
            });

            SelectFilesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectFiles();
            });

            SelectOutputPathCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectOutputFile();
            });
        }

        public int PackageFiles(string[] inputPaths, string outputPath)
        {
            if (!Directory.Exists(outputPath))
                throw new ArgumentException($"Output directory not found: {outputPath}", nameof(outputPath));

            return  _gameService.PackageAsync(inputPaths, outputPath).Result;
        }

        private async Task SelectFiles()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(PackageLevelsWindow.I);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select files to pack",
            });

            if (files != null)
            {
                FilesList.AddRange(files.Select(f => f.Path.LocalPath));
            }
        }

        private async Task SelectOutputFile()
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(PackageLevelsWindow.I);

            var uri = Path.GetDirectoryName(OutputPath) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Globals.APP_DIRECTORY);
            var startlocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(uri);

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save as",
                ShowOverwritePrompt = true,
                SuggestedFileName = "LEVELS.DAT",
                SuggestedStartLocation = startlocation
            });

            if (file != null)
            {
                OutputPath = file.Path.LocalPath;
                this.RaisePropertyChanged(nameof(OutputPath));
            }
        }
    }
}
