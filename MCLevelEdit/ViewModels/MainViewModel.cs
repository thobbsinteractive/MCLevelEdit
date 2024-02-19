using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class MainViewModel : ViewModelBase
{
    private const string DOCS_DIRECTORY = "MCLevelEdit";

    private int _failCount = 0;
    private int _warningCount = 0;

    protected readonly ISettingsPort _settingsPort;
    protected readonly IGameService _gameService;

    public int FailCount
    {
        get => _failCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _failCount, value);
        }
    }

    public int WarningCount
    {
        get => _warningCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _warningCount, value);
        }
    }

    public ICommand NewFileCommand { get; }
    public ICommand NewRandomFileCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand SaveFileAsCommand { get; }
    public ICommand ExportHeightMapCommand { get; }
    public ICommand ExportTerrainRenderCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand EditEntitiesCommand { get; }
    public ICommand EditGameSettingsCommand { get; }
    public ICommand DisplayFailCommand { get; }
    public ICommand DisplayWarningsCommand { get; }
    public ICommand DisplayAboutCommand { get; }

    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }
    public NodePropertiesViewModel NodePropertiesViewModel { get; }
    public Interaction<EntitiesTableViewModel, EntitiesTableViewModel?> ShowEntitiesDialog { get; }
    public Interaction<EditGameSettingsViewModel, EditGameSettingsViewModel?> ShowGameSettingsDialog { get; }
    public Interaction<ValidationResultsTableViewModel, ValidationResultsTableViewModel?> ShowValidationResultsDialog { get; }
    public Interaction<AboutWindowViewModel, AboutWindowViewModel?> ShowAboutDialog { get; }

    public MainViewModel(EventAggregator<object> eventAggregator, ISettingsPort settingsPort, IMapService mapService, ITerrainService terrainService, IGameService gameService) : base(eventAggregator, mapService, terrainService)
    {
        _settingsPort = settingsPort;
        _gameService = gameService;

        _mapService.CreateNewMap(true);
        NodePropertiesViewModel = new NodePropertiesViewModel(eventAggregator, mapService, terrainService);
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);

        ShowGameSettingsDialog = new Interaction<EditGameSettingsViewModel, EditGameSettingsViewModel?>();
        ShowEntitiesDialog = new Interaction<EntitiesTableViewModel, EntitiesTableViewModel?>();
        ShowValidationResultsDialog = new Interaction<ValidationResultsTableViewModel, ValidationResultsTableViewModel?>();
        ShowAboutDialog = new Interaction<AboutWindowViewModel, AboutWindowViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowEntitiesDialog.Handle(Locator.Current.GetService<EntitiesTableViewModel>());
        });

        EditGameSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowGameSettingsDialog.Handle(Locator.Current.GetService<EditGameSettingsViewModel>());
        });

        NewFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await PromptSaveAndOrContinue())
            {
                await mapService.CreateNewMap();
                MainWindow.I.Title = GetTitle("NewLevel.DAT");
            }
        });

        NewRandomFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await PromptSaveAndOrContinue())
            {
                await mapService.CreateNewMap(true);
                MainWindow.I.Title = GetTitle("NewLevel.DAT");
            }
        });

        OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await OpenFile();
        });

        SaveFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SaveFile(false);
        });

        SaveFileAsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SaveFile(true);
        });

        ExportHeightMapCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await ExportImageMap(Model.Enums.Layer.Height);
        });

        ExportTerrainRenderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await ExportImageMap(Model.Enums.Layer.Game);
        });

        RunCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await SaveFile(false) && !string.IsNullOrWhiteSpace(_settingsPort.CurrentLevelFilePath) && File.Exists(_settingsPort.CurrentLevelFilePath))
            {
                var settings = _settingsPort.LoadSettings();
                if (settings == null || string.IsNullOrWhiteSpace(settings.GameExeLocation))
                {
                    await ShowGameSettingsDialog.Handle(Locator.Current.GetService<EditGameSettingsViewModel>());
                }
                else
                {
                    await _gameService.RunLevelFromSettings(new string[] { _settingsPort.CurrentLevelFilePath });
                }
            }
        });

        ExitCommand = ReactiveCommand.CreateFromTask(() =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
            return Task.CompletedTask;
        });

        DisplayFailCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new ValidationResultsTableViewModel(_eventAggregator, _mapService);
            vm.Filter = Result.Fail;
            await ShowValidationResultsDialog.Handle(vm);
        });

        DisplayWarningsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new ValidationResultsTableViewModel(_eventAggregator, _mapService);
            vm.Filter = Result.Warning;
            await ShowValidationResultsDialog.Handle(vm);
        });

        DisplayAboutCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var versionStr = Assembly.GetExecutingAssembly().GetName().Version?.ToString() + "-beta";
            await ShowAboutDialog.Handle(new AboutWindowViewModel()
            {
                Version = versionStr
            });
        });

        MainWindow.I.Title = GetTitle("NewLevel.DAT");
    }

    public void OnKeyPressed(Key key)
    {
        _eventAggregator.RaiseEvent("KeyPressed", this, new PubSubEventArgs<object>(key));
    }

    private async Task ExportImageMap(Model.Enums.Layer layer)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow.I);

        var map = _mapService.GetMap();

        string suggestedExtension = "bmp";
        string suggestedFileName = !string.IsNullOrWhiteSpace(map.FilePath) ? Path.GetFileNameWithoutExtension(map.FilePath) : "NewLevel";
        IStorageFolder storageFolder = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures);

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Height Map",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            DefaultExtension = suggestedExtension,
            SuggestedStartLocation = storageFolder
        });

        if (file != null)
        {
            string filePath = file.Path.AbsolutePath;
            try
            {
                WriteableBitmap preview = new WriteableBitmap(
                    new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                    new Vector(96, 96),
                    PixelFormat.Rgba8888);

                var bmp = await _terrainService.DrawBitmapAsync(preview, map.Terrain, layer);

                bmp.Save(filePath);
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Unable to save the height map file {filePath}!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }

            MainWindow.I.Title = GetTitle(filePath);
        }
    }

    private async Task<bool> PromptSaveAndOrContinue()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Question", $"Do you want to Save your changes?", ButtonEnum.YesNoCancel, Icon.Question);
        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            return await SaveFile(false);
        }
        else if (result == ButtonResult.Cancel)
        {
            return false;
        }
        return true;
    }

    private async Task<bool> SaveFile(bool saveAs)
    {
        var filePath = _settingsPort.CurrentLevelFilePath;
        if (!File.Exists(filePath) && !saveAs)
        {
            saveAs = true;
        }

        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(MainWindow.I);

        var map = _mapService.GetMap();

        if (saveAs)
        {
            string suggestedExtension = "DAT";
            string suggestedFileName = !string.IsNullOrWhiteSpace(map.FilePath) ? Path.GetFileNameWithoutExtension(map.FilePath) : "NewLevel";
            IStorageFolder storageFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DOCS_DIRECTORY));
            
            if (storageFolder != null && !Directory.Exists(storageFolder.Path.AbsolutePath))
            {
                Directory.CreateDirectory(storageFolder.Path.AbsolutePath);
            }

            if (!string.IsNullOrWhiteSpace(map.FilePath))
            {
                suggestedFileName = Path.GetFileName(map.FilePath);
                storageFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(map.FilePath));
                suggestedExtension = Path.GetExtension(map.FilePath);
            }

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Map File As...",
                SuggestedFileName = suggestedFileName,
                ShowOverwritePrompt = true,
                DefaultExtension = suggestedExtension,
                SuggestedStartLocation = storageFolder
            });

            if (file != null)
            {
                filePath = file.Path.AbsolutePath;
            }
        }

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!string.IsNullOrWhiteSpace(filePath) && !await _mapService.SaveMapToFileAsync(filePath))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Unable to save the map file {filePath}!", ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
            return false;
        }
        MainWindow.I.Title = GetTitle(filePath);
        _settingsPort.CurrentLevelFilePath = filePath;
        map.FilePath = filePath;

        RefreshData();
        return true;
    }

    private async Task OpenFile()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(MainWindow.I);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Map File",
            AllowMultiple = false
        });

        if (files != null && files.Count == 1 && File.Exists(files[0].Path.AbsolutePath))
        {
            string filePath = files[0].Path.AbsolutePath;
            if (!await _mapService.LoadMapFromFileAsync(filePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Unable to load the map file {filePath}!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }

            _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
            _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));

            MainWindow.I.Title = GetTitle(filePath);
            _settingsPort.CurrentLevelFilePath = filePath;
        }
        RefreshData();
    }

    private string GetTitle(string filePath)
    {
        string title = !string.IsNullOrWhiteSpace(filePath)? Path.GetFileName(filePath) : string.Empty;
        return $"Magic Carpet 1 Level Editor: {title}";
    }

    private void RefreshData()
    {
        FailCount = 0;
        WarningCount = 0;
        var results = _mapService.GetValidationResults();

        if (results != null)
        {
            FailCount = results.Where(r => r.Result == Result.Fail).Count();
            WarningCount = results.Where(r => r.Result == Result.Warning).Count();
        }
    }
}
