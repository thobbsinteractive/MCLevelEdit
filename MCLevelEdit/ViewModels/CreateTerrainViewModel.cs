using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class CreateTerrainViewModel : ViewModelBase
    {
        public ICommand GenerateTerrainCommand { get; }
        public bool GenerateTerrainButtonEnable { get; set; }

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
            await _mapService.RecalculateTerrain(GenerationParameters.ToGenerationParameters());
            GenerateTerrainButtonEnable = true;
            await RefreshPreviewAsync();
        }
    }
}
