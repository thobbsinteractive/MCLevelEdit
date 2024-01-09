using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;

namespace MCLevelEdit.ViewModels;

public class MapViewModel : ViewModelBase
{
    public EntityToolBarViewModel EntityToolBarViewModel { get; }
    public EditTerrainViewModel EditTerrainViewModel { get; }
    public MapTreeViewModel MapTreeViewModel { get; }
    public MapEditorViewModel MapEditorViewModel { get; }

    public MapViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        EntityToolBarViewModel = new EntityToolBarViewModel(eventAggregator, mapService, terrainService);
        EditTerrainViewModel = new EditTerrainViewModel(eventAggregator, mapService, terrainService);
        MapTreeViewModel = new MapTreeViewModel(eventAggregator, mapService, terrainService);
        MapEditorViewModel = new MapEditorViewModel(eventAggregator, mapService, terrainService);
        EditTerrainViewModel.GenerateHeightMap();
    }
}
