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
        _mapService.CreateNewMap();
        NodePropertiesViewModel = new NodePropertiesViewModel(eventAggregator, mapService, terrainService);
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);

        ShowDialog = new Interaction<EntitiesTableViewModel, EntitiesTableViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {

            var result = await ShowDialog.Handle(Locator.Current.GetService<EntitiesTableViewModel>());
        });

        OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(MainView.I);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Map File",
                AllowMultiple = false
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.AbsolutePath))
            {
                await mapService.LoadMapFromFileAsync(files[0].Path.AbsolutePath);
                _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
                _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
                _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
            }
        });

        ExitCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        });
    }
}
