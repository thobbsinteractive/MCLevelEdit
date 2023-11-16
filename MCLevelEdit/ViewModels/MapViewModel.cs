using MCLevelEdit.Interfaces;
using Splat;

namespace MCLevelEdit.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public CreateEntityViewModel CreateEntityViewModel { get; }
        public CreateTerrainViewModel CreateTerrainViewModel { get; }

        public MapViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            CreateEntityViewModel = Locator.Current.GetService<CreateEntityViewModel>();
            CreateTerrainViewModel = Locator.Current.GetService<CreateTerrainViewModel>();
            CreateTerrainViewModel.GenerateHeightMap();
        }
    }
}
