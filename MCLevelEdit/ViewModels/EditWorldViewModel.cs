using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
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

        public ICommand CalculateManaCommand { get; }
        public ICommand GenerateTerrainCommand { get; }

        public ICommand IncreaseSeedCommand { get; }
        public ICommand DecreaseSeedCommand { get; }
        public ICommand IncreaseOffsetCommand { get; }
        public ICommand DecreaseOffsetCommand { get; }
        public ICommand IncreaseRaiseCommand { get; }
        public ICommand DecreaseRaiseCommand { get; }
        public ICommand IncreaseGnarlCommand { get; }
        public ICommand DecreaseGnarlCommand { get; }
        public ICommand IncreaseRiverCommand { get; }
        public ICommand DecreaseRiverCommand { get; }
        public ICommand IncreaseSourceCommand { get; }
        public ICommand DecreaseSourceCommand { get; }
        public ICommand IncreaseSnLinCommand { get; }
        public ICommand DecreaseSnLinCommand { get; }
        public ICommand IncreaseSnFltCommand { get; }
        public ICommand DecreaseSnFltCommand { get; }
        public ICommand IncreaseBhLinCommand { get; }
        public ICommand DecreaseBhLinCommand { get; }
        public ICommand IncreaseSandCommand { get; }
        public ICommand DecreaseSandCommand { get; }
        public ICommand IncreaseRkSteCommand { get; }
        public ICommand DecreaseRkSteCommand { get; }

        public bool GenerateTerrainButtonEnable { get; set; }

        public EditWorldViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
        {
            _eventAggregator = eventAggregator;
            _mapService = mapService;

            RefreshTerrainData();
            RefreshWorldData();

            _eventAggregator.RegisterEvent("RefreshTerrain", RefreshDataHandler);
            _eventAggregator.RegisterEvent("RefreshWorld", RefreshWorldHandler);

            CalculateManaCommand = ReactiveCommand.Create(async () =>
            {
                ManaTotal = _mapService.CalculateMana();
                _mapService.UpdateManaTotal(ManaTotal);
            });

            GenerateTerrainCommand = ReactiveCommand.Create(async () =>
            {
                await GenerateHeightMap();
            });

            IncreaseSeedCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Seed++;
                await GenerateHeightMap();
            });
            DecreaseSeedCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Seed--;
                await GenerateHeightMap();
            });

            IncreaseOffsetCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Offset++;
                await GenerateHeightMap();
            });
            DecreaseOffsetCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Offset--;
                await GenerateHeightMap();
            });

            IncreaseRaiseCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Raise++;
                await GenerateHeightMap();
            });
            DecreaseRaiseCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Raise--;
                await GenerateHeightMap();
            });

            IncreaseGnarlCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Gnarl++;
                await GenerateHeightMap();
            });
            DecreaseGnarlCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Gnarl--;
                await GenerateHeightMap();
            });

            IncreaseRiverCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.River++;
                await GenerateHeightMap();
            });
            DecreaseRiverCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.River--;
                await GenerateHeightMap();
            });

            IncreaseSourceCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Source++;
                await GenerateHeightMap();
            });
            DecreaseSourceCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.Source--;
                await GenerateHeightMap();
            });

            IncreaseSnLinCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.SnLin++;
                await GenerateHeightMap();
            });
            DecreaseSnLinCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.SnLin--;
                await GenerateHeightMap();
            });

            IncreaseSnFltCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.SnFlt++;
                await GenerateHeightMap();
            });
            DecreaseSnFltCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.SnFlt--;
                await GenerateHeightMap();
            });

            IncreaseBhLinCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BhLin++;
                await GenerateHeightMap();
            });
            DecreaseBhLinCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BhLin--;
                await GenerateHeightMap();
            });

            IncreaseSandCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BhFlt++;
                await GenerateHeightMap();
            });
            DecreaseSandCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BhFlt--;
                await GenerateHeightMap();
            });

            IncreaseRkSteCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.RkSte++;
                await GenerateHeightMap();
            });
            DecreaseRkSteCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.RkSte--;
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
