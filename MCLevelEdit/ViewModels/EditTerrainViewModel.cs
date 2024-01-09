using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditTerrainViewModel : ViewModelBase
    {
        public ICommand GenerateTerrainCommand { get; }
        public bool GenerateTerrainButtonEnable { get; set; }

        public EditTerrainViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
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
            _eventAggregator.RaiseEvent("RefreshData", this, new Application.Model.PubSubEventArgs<object>("RefreshData"));
        }
    }
}
