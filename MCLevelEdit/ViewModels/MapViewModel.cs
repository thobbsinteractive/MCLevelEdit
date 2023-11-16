using MCLevelEdit.Interfaces;
using Splat;

namespace MCLevelEdit.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public CreateEntityViewModel CreateEntityViewModel { get; }
        public CreateTerrainViewModel CreateTerrainViewModel { get; }

        public MapViewModel(IMapService mapService, ITerrainService terrainService, IFileService fileService) : base(mapService, terrainService, fileService)
        {
            CreateEntityViewModel = Locator.Current.GetService<CreateEntityViewModel>();
            CreateTerrainViewModel = Locator.Current.GetService<CreateTerrainViewModel>();
            CreateTerrainViewModel.GenerateHeightMap();
        }
    }
}
