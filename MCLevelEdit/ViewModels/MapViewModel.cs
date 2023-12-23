using MCLevelEdit.Model.Abstractions;
using Splat;

namespace MCLevelEdit.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public EntityToolBarViewModel EntityToolBarViewModel { get; }
        public CreateTerrainViewModel CreateTerrainViewModel { get; }

        public MapViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            EntityToolBarViewModel = Locator.Current.GetService<EntityToolBarViewModel>();
            CreateTerrainViewModel = Locator.Current.GetService<CreateTerrainViewModel>();
            CreateTerrainViewModel.GenerateHeightMap();
        }
    }
}
