using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
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
    public ICommand ExitCommand { get; }
    public ICommand EditEntitiesCommand { get; }
    public EntitiesTableViewModel EntitiesTableViewModel {  get; }
    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public EditTerrainViewModel EditTerrainViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }
    public Interaction<EntitiesTableViewModel, EntitiesTableViewModel?> ShowDialog { get; }

    public MainViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        EntitiesTableViewModel = Locator.Current.GetService<EntitiesTableViewModel>();

        _mapService.CreateNewMap();
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        EditTerrainViewModel = new EditTerrainViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);
        EditTerrainViewModel.GenerateHeightMap();

        ShowDialog = new Interaction<EntitiesTableViewModel, EntitiesTableViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowDialog.Handle(EntitiesTableViewModel);
            LoadEntities(Entities.ToEntities());
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
                var map = mapService.GetMap();

                GenerationParameters.SetParameters(map.Terrain.GenerationParameters.ToTerrainGenerationParamsViewModel());
                this.RaisePropertyChanged(nameof(GenerationParameters));
                LoadEntityViewModels(map.Entities.ToEntityViewModels());
            }
        });
    }
}
