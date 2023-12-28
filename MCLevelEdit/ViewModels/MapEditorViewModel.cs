using MCLevelEdit.Model.Abstractions;

namespace MCLevelEdit.ViewModels;

public class MapEditorViewModel : ViewModelBase
{
    public MapEditorViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
    {
    }
}
