using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Views;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class MainViewModel : ViewModelBase
{
    public IObservable<bool> IsRefreshed { get; }

    public ICommand NewFileCommand { get; }
    public ICommand NewRandomFileCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand EditEntitiesCommand { get; }
    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }
    public NodePropertiesViewModel NodePropertiesViewModel { get; }
    public Interaction<EntitiesTableViewModel, EntitiesTableViewModel?> ShowDialog { get; }

    public MainViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        _mapService.CreateNewMap(true);
        NodePropertiesViewModel = new NodePropertiesViewModel(eventAggregator, mapService, terrainService);
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);

        ShowDialog = new Interaction<EntitiesTableViewModel, EntitiesTableViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowDialog.Handle(Locator.Current.GetService<EntitiesTableViewModel>());
        });

        NewFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(MainWindow.I);

            await mapService.CreateNewMap();

            MainWindow.I.Title = GetTitle("NewMap.mc1");
        });

        NewRandomFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(MainWindow.I);

            await mapService.CreateNewMap(true);

            MainWindow.I.Title = GetTitle("NewMap.mc1");
        });

        OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
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
                await mapService.LoadMapFromFileAsync(filePath);
                _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
                _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
                _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));

                MainWindow.I.Title = GetTitle(filePath);
            }
        });

        SaveFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(MainWindow.I);

            var map = _mapService.GetMap();

            string suggestedExtension = "mc1";
            string suggestedFileName = "NewMap";
            IStorageFolder storageFolder = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

            if (!string.IsNullOrWhiteSpace(map.FilePath))
            {
                suggestedFileName = Path.GetFileName(map.FilePath);
                storageFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(map.FilePath));
                suggestedExtension = Path.GetExtension(map.FilePath);
            }

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Map File",
                SuggestedFileName = suggestedFileName,
                ShowOverwritePrompt = true,
                DefaultExtension = suggestedExtension,
                SuggestedStartLocation = storageFolder
            });
        });

        ExitCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        });

        MainWindow.I.Title = GetTitle("NewMap.mc1");
    }

    private string GetTitle(string filePath)
    {
        string title = !string.IsNullOrWhiteSpace(filePath)? Path.GetFileName(filePath) : string.Empty;
        return $"Magic Carpet 1 Level Editor: {title}";
    }
}
