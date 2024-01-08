using Avalonia.Interactivity;
using MCLevelEdit.Abstractions;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Views;
using ReactiveUI;
using Splat;
using System;

namespace MCLevelEdit.ViewModels;

public class MapViewModel : ViewModelBase
{
    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public EditTerrainViewModel EditTerrainViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }

    public MapViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
    {
        EntityToolBarViewModel = new EntityToolBarViewModel(mapService, terrainService);
        EditTerrainViewModel = new EditTerrainViewModel(mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(mapService, terrainService);
        EditTerrainViewModel.GenerateHeightMap();
    }
}
