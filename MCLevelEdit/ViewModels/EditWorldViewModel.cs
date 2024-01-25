using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditWorldViewModel : ReactiveObject
    {
        protected readonly IMapService _mapService;
        protected readonly EventAggregator<object> _eventAggregator;
        private uint _manaTotal;
        private byte _manaTarget;
        private TerrainGenerationParamsViewModel _generationParameters;

        public uint ManaTotal
        {
            get => _manaTotal;
            set
            {
                this.RaiseAndSetIfChanged(ref _manaTotal, value);
                _mapService.UpdateManaTotal(_manaTotal);
            }
        }

        public byte ManaTarget
        {
            get => _manaTarget;
            set
            {
                this.RaiseAndSetIfChanged(ref _manaTarget, value);
                _mapService.UpdateManaTarget(_manaTarget);
            }
        }

        public TerrainGenerationParamsViewModel GenerationParameters
        {
            get => _generationParameters;
            set => this.RaiseAndSetIfChanged(ref _generationParameters, value);
        }

        public ICommand GenerateTerrainCommand { get; }
        public bool GenerateTerrainButtonEnable { get; set; }

        public EditWorldViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
        {
            _eventAggregator = eventAggregator;
            _mapService = mapService;

            RefreshTerrainData();
            RefreshWorldData();

            _eventAggregator.RegisterEvent("RefreshTerrain", RefreshDataHandler);
            _eventAggregator.RegisterEvent("RefreshWorld", RefreshWorldHandler);

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
        }

        public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
        {
            RefreshTerrainData();
        }

        public void RefreshTerrainData()
        {
            var map = _mapService.GetMap();
            GenerationParameters = map.Terrain.GenerationParameters.ToTerrainGenerationParamsViewModel();
            GenerateTerrainButtonEnable = true;
        }

        public void RefreshWorldHandler(object sender, PubSubEventArgs<object> args)
        {
            RefreshWorldData();
        }

        public void RefreshWorldData()
        {
            var map = _mapService.GetMap();
            ManaTotal = map.ManaTotal;
            ManaTarget = map.ManaTarget;
        }
    }
}
