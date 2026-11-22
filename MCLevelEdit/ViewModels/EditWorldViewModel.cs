using MagicCarpet2Terrain.Model;
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

        /// <summary>Map type (Day/Night/Cave) only exists in Magic Carpet 2 levels.</summary>
        public bool IsMc2 => _mapService.GetMap().GameVersion == GameVersion.MagicCarpet2;

        /// <summary>Magic Carpet format levels can be for Magic Carpet or Hidden Worlds; the file can't say which.</summary>
        public bool IsMc1Format => !IsMc2;

        public bool IsMagicCarpetGame
        {
            get => !_mapService.GetMap().IsHiddenWorlds;
            set { if (value) SetHiddenWorlds(false); }
        }

        public bool IsHiddenWorldsGame
        {
            get => _mapService.GetMap().IsHiddenWorlds;
            set { if (value) SetHiddenWorlds(true); }
        }

        private void SetHiddenWorlds(bool hiddenWorlds)
        {
            var map = _mapService.GetMap();
            if (map.IsHiddenWorlds == hiddenWorlds)
                return;

            // Renames the level's spell jars (Hidden Worlds has Homing Meteor for Wall of Fire)
            map.SetHiddenWorlds(hiddenWorlds);
            RaiseGameChanged();
            _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            // Hidden Worlds generates terrain a little differently (and has its own colours), so regenerate.
            // The variables are unchanged, so the landscape stays the same.
            _ = GenerateHeightMap();
        }

        private void RaiseGameChanged()
        {
            this.RaisePropertyChanged(nameof(IsMc1Format));
            this.RaisePropertyChanged(nameof(IsMagicCarpetGame));
            this.RaisePropertyChanged(nameof(IsHiddenWorldsGame));
        }

        /// <summary>Roof height given to a level converted to a cave that has none (MC2's own caves use around 114-154).</summary>
        private const byte DEFAULT_CAVE_BASIC_HEIGHT = 128;

        public MapType MapType
        {
            get => GenerationParameters?.MapType ?? MapType.Day;
            set
            {
                if (GenerationParameters is null || GenerationParameters.MapType == value)
                    return;

                GenerationParameters.MapType = value;
                _mapService.GetMap().Terrain.GenerationParameters.MapType = value;

                if (value == MapType.Cave && GenerationParameters.BasicHeight == 0)
                {
                    // With no roof the whole cave would be solid rock
                    GenerationParameters.BasicHeight = DEFAULT_CAVE_BASIC_HEIGHT;
                }
                RaiseMapTypeChanged();

                // Each map type is generated differently: caves build a ceiling and rock, and Day is shaded
                // the other way round from Night and Cave. Regenerating converts the level; the variables
                // are unchanged, so the landscape stays the same.
                _ = GenerateHeightMap();
            }
        }

        public bool IsDay
        {
            get => MapType == MapType.Day;
            set { if (value) MapType = MapType.Day; }
        }

        public bool IsNight
        {
            get => MapType == MapType.Night;
            set { if (value) MapType = MapType.Night; }
        }

        public bool IsCave
        {
            get => MapType == MapType.Cave;
            set { if (value) MapType = MapType.Cave; }
        }

        private void RaiseMapTypeChanged()
        {
            this.RaisePropertyChanged(nameof(MapType));
            this.RaisePropertyChanged(nameof(IsDay));
            this.RaisePropertyChanged(nameof(IsNight));
            this.RaisePropertyChanged(nameof(IsCave));
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
        public ICommand IncreaseLRiverCommand { get; }
        public ICommand DecreaseLRiverCommand { get; }
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
        public ICommand IncreaseBasicHeightCommand { get; }
        public ICommand DecreaseBasicHeightCommand { get; }
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

            IncreaseLRiverCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.LRiver++;
                await GenerateHeightMap();
            });
            DecreaseLRiverCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.LRiver--;
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

            // Cave levels: the roof height the ceiling is built down from
            IncreaseBasicHeightCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BasicHeight++;
                await GenerateHeightMap();
            });
            DecreaseBasicHeightCommand = ReactiveCommand.Create(async () =>
            {
                GenerationParameters.BasicHeight--;
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
            this.RaisePropertyChanged(nameof(IsMc2));
            RaiseGameChanged();
            RaiseMapTypeChanged();
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
