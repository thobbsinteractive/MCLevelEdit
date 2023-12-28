using MCLevelEdit.Model.Abstractions;
using Splat;

namespace MCLevelEdit.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public EntityToolBarViewModel EntityToolBarViewModel { get; }
        public EditTerrainViewModel EditTerrainViewModel { get; }

        public MapViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            EntityToolBarViewModel = Locator.Current.GetService<EntityToolBarViewModel>();
            EditTerrainViewModel = Locator.Current.GetService<EditTerrainViewModel>();
            EditTerrainViewModel.GenerateHeightMap();
        }
    }
}
