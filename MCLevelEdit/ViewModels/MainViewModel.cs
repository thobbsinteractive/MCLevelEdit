using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class MainViewModel : ViewModelBase
{
    public IObservable<bool> IsRefreshed { get; }

    public ICommand NewFileCommand { get; }
    public ICommand NewRandomFileCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand ExportHeightMapCommand { get; }
    public ICommand ExportTerrainRenderCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand EditEntitiesCommand { get; }
    public ICommand EditGameSettingsCommand { get; }

    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }
    public NodePropertiesViewModel NodePropertiesViewModel { get; }
    public Interaction<EntitiesTableViewModel, EntitiesTableViewModel?> ShowEntitiesDialog { get; }
    public Interaction<EditGameSettingsViewModel, EditGameSettingsViewModel?> ShowGameSettingsDialog { get; }

    public MainViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        _mapService.CreateNewMap(true);
        NodePropertiesViewModel = new NodePropertiesViewModel(eventAggregator, mapService, terrainService);
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);

        ShowGameSettingsDialog = new Interaction<EditGameSettingsViewModel, EditGameSettingsViewModel?>();
        ShowEntitiesDialog = new Interaction<EntitiesTableViewModel, EntitiesTableViewModel?>();

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
            await mapService.CreateNewMap();
            MainWindow.I.Title = GetTitle("LEV00000.DAT");
        });

        NewRandomFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await mapService.CreateNewMap(true);
            MainWindow.I.Title = GetTitle("LEV00000.DAT");
        });

        OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await OpenFile();
        });

        SaveFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await SaveFile();
        });

        ExportHeightMapCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await ExportImageMap(Model.Enums.Layer.Height);
        });

        ExportTerrainRenderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await ExportImageMap(Model.Enums.Layer.Game);
        });

        ExitCommand = ReactiveCommand.CreateFromTask(() =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
            return Task.CompletedTask;
        });

        MainWindow.I.Title = GetTitle("LEV00000.DAT");
    }

    private async Task ExportImageMap(Model.Enums.Layer layer)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow.I);

        var map = _mapService.GetMap();

        string suggestedExtension = "bmp";
        string suggestedFileName = "LEV00000";
        IStorageFolder storageFolder = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        if (!string.IsNullOrWhiteSpace(map.FilePath))
        {
            suggestedFileName = Path.GetFileName(map.FilePath);
            storageFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(map.FilePath));
            suggestedExtension = Path.GetExtension(map.FilePath);
        }

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

    private async Task SaveFile()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(MainWindow.I);

        var map = _mapService.GetMap();

        string suggestedExtension = "DAT";
        string suggestedFileName = "LEV00000";
        IStorageFolder storageFolder = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        if (!string.IsNullOrWhiteSpace(map.FilePath))
        {
            suggestedFileName = Path.GetFileName(map.FilePath);
            storageFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(map.FilePath));
            suggestedExtension = Path.GetExtension(map.FilePath);
        }

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Map File",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            DefaultExtension = suggestedExtension,
            SuggestedStartLocation = storageFolder
        });

        if (file != null)
        {
            string filePath = file.Path.AbsolutePath;
            if (!await _mapService.SaveMapToFileAsync(filePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Unable to save the map file {filePath}!", ButtonEnum.Ok, Icon.Error);
                await box.ShowAsync();
            }
            MainWindow.I.Title = GetTitle(filePath);
        }
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
        }
    }

    private string GetTitle(string filePath)
    {
        string title = !string.IsNullOrWhiteSpace(filePath)? Path.GetFileName(filePath) : string.Empty;
        return $"Magic Carpet 1 Level Editor: {title}";
    }
}
