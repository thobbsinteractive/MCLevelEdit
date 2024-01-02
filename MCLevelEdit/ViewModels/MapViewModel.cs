using MCLevelEdit.Model.Abstractions;
using Splat;

namespace MCLevelEdit.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public EntityToolBarViewModel EntityToolBarViewModel { get; }
        public EditTerrainViewModel EditTerrainViewModel { get; }
        public MapEditorViewModel MapEditorViewModel { get; }

        public MapViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            EntityToolBarViewModel = new EntityToolBarViewModel(mapService, terrainService);
            EditTerrainViewModel = new EditTerrainViewModel(mapService, terrainService);
            MapEditorViewModel = new MapEditorViewModel(mapService, terrainService);
            EditTerrainViewModel.GenerateHeightMap();
        }
    }
}
