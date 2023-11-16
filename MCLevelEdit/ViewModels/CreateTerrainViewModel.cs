using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class CreateTerrainViewModel : ViewModelBase
    {
        public ICommand GenerateTerrainCommand { get; }
        public bool GenerateTerrainButtonEnable { get; set; }
        public TerrainGenerationParameters TerrainGenerationParameters => Map.Instance.TerrainGenerationParameters;

        public CreateTerrainViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            GenerateTerrainButtonEnable = true;

            GenerateTerrainCommand = ReactiveCommand.Create(async () =>
            {
                await GenerateHeightMap();
            });
        }

        public async Task GenerateHeightMap()
        {
            GenerateTerrainButtonEnable = false;
            Map.Instance.HeightMap = await _terrainService.CalculateTerrain(TerrainGenerationParameters);
            GenerateTerrainButtonEnable = true;
            await RefreshPreviewAsync();
        }
    }
}
