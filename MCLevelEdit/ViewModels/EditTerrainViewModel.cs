using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditTerrainViewModel : ReactiveObject
    {
        protected readonly IMapService _mapService;
        protected readonly EventAggregator<object> _eventAggregator;
        private TerrainGenerationParamsViewModel _generationParameters;

        public TerrainGenerationParamsViewModel GenerationParameters
        {
            get => _generationParameters;
            set => this.RaiseAndSetIfChanged(ref _generationParameters, value);
        }

        public ICommand GenerateTerrainCommand { get; }
        public bool GenerateTerrainButtonEnable { get; set; }

        public EditTerrainViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
        {
            _eventAggregator = eventAggregator;
            _mapService = mapService;
            GenerationParameters = _mapService.GetMap()?.Terrain.GenerationParameters.ToTerrainGenerationParamsViewModel();
            _eventAggregator.RegisterEvent("RefreshTerrain", RefreshDataHandler);

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
            _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
        }

        public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
        {
            GenerationParameters = _mapService.GetMap()?.Terrain.GenerationParameters.ToTerrainGenerationParamsViewModel();
        }

    }
}
