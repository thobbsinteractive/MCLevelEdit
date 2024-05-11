using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Extensions;
using ReactiveUI;
using System.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class EntityToolBarViewModel : ViewModelBase
{
    private bool _cursorSelected = false;
    private bool _creaturesSelected = false;
    private bool _scenerySelected = false;
    private bool _effectsSelected = false;
    private bool _spellsSelected = false;
    private bool _switchesSelected = false;
    private bool _weathersSelected = false;
    private bool _spawnsSelected = false;
    private bool _wallSelected = false;
    private bool _pathSelected = false;
    private bool _canyonSelected = false;
    private bool _ridgeSelected = false;

    private EntityViewModel _addEntityViewModel;
    private EntityViewModel? _previousPathNodeViewModel = null;

    public EntityViewModel AddEntityViewModel 
    {
        get 
        {
            return _addEntityViewModel;
        }
        set 
        { 
            this.RaiseAndSetIfChanged(ref _addEntityViewModel, value);
        }
    }

    public int ModelIdx
    {
        get
        {
             return AddEntityViewModel?.ModelIdx ?? -1;
        }
        set
        {
            if (AddEntityViewModel != null)
            {
                AddEntityViewModel.ModelIdx = value;
            }
        }
    }

    public bool CursorSelected
    {
        get
        {
            return _cursorSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _cursorSelected, value);
        }
    }

    public bool CreaturesSelected
    {
        get
        {
            return _creaturesSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _creaturesSelected, value);
        }
    }

    public bool ScenerySelected
    {
        get
        {
            return _scenerySelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _scenerySelected, value);
        }
    }

    public bool EffectsSelected
    {
        get
        {
            return _effectsSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _effectsSelected, value);
        }
    }

    public bool SpellsSelected
    {
        get
        {
            return _spellsSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _spellsSelected, value);
        }
    }

    public bool SwitchesSelected
    {
        get
        {
            return _switchesSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _switchesSelected, value);
        }
    }

    public bool WeathersSelected
    {
        get
        {
            return _weathersSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _weathersSelected, value);
        }
    }

    public bool SpawnsSelected
    {
        get
        {
            return _spawnsSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _spawnsSelected, value);
        }
    }

    public bool WallSelected
    {
        get
        {
            return _wallSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _wallSelected, value);
        }
    }

    public bool PathSelected
    {
        get
        {
            return _pathSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _pathSelected, value);
        }
    }

    public bool CanyonSelected
    {
        get
        {
            return _canyonSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _canyonSelected, value);
        }
    }

    public bool RidgeSelected
    {
        get
        {
            return _ridgeSelected;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _ridgeSelected, value);
        }
    }

    public ICommand CursorSelectedCommand { get; }
    public ICommand CreaturesSelectedCommand { get; }
    public ICommand ScenarySelectedCommand { get; }
    public ICommand EffectsSelectedCommand { get; }
    public ICommand SpellsSelectedCommand { get; }
    public ICommand SwitchesSelectedCommand { get; }
    public ICommand WeathersSelectedCommand { get; }
    public ICommand SpawnsSelectedCommand { get; }
    public ICommand WallSelectedCommand { get; }
    public ICommand PathSelectedCommand { get; }
    public ICommand CanyonSelectedCommand { get; }
    public ICommand RidgeSelectedCommand { get; }

    public EntityToolBarViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        _eventAggregator.RegisterEvent("OnCursorClicked", CursorClickedHandler);
        CursorSelected = true;

        CursorSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            CursorSelected = true;
        });

        CreaturesSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            CreaturesSelected = true;
            OnEntityTypeSelected(TypeId.Creature);
        });

        ScenarySelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            ScenerySelected = true;
            OnEntityTypeSelected(TypeId.Scenery);
        });

        EffectsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            EffectsSelected = true;
            OnEntityTypeSelected(TypeId.Effect);
        });

        SpellsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            SpellsSelected = true;
            OnEntityTypeSelected(TypeId.Spell);
        });

        SwitchesSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            SwitchesSelected = true;
            OnEntityTypeSelected(TypeId.Switch);
        });

        WeathersSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            WeathersSelected = true;
            OnEntityTypeSelected(TypeId.Weather);
        });

        SpawnsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            SpawnsSelected = true;
            OnEntityTypeSelected(TypeId.Spawn);
        });

        WallSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            WallSelected = true;
            OnPathTypeClicked((int)Effect.Wall);
        });

        PathSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            PathSelected = true;
            OnPathTypeClicked((int)Effect.Path);
        });

        CanyonSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            CanyonSelected = true;
            OnPathTypeClicked((int)Effect.Canyon);
        });

        RidgeSelectedCommand = ReactiveCommand.Create(() =>
        {
            ClearSelection();
            RidgeSelected = true;
            OnPathTypeClicked((int)Effect.RidgeNode);
        });

    }

    private void ToggleButtons(bool enable)
    {
        CursorSelected = enable;
        CreaturesSelected = enable;
        ScenerySelected = enable;
        EffectsSelected = enable;
        SpellsSelected = enable;
        SwitchesSelected = enable;
        WeathersSelected = enable;
        SpawnsSelected = enable;
        WallSelected = enable;
        PathSelected = enable;
        CanyonSelected = enable;
        RidgeSelected = enable;
    }

    private void CursorClickedHandler(object sender, PubSubEventArgs<object> arg)
    {
        if (arg.Item is not null && _addEntityViewModel is not null)
        {
            var cursorEvent = ((Point, bool, bool))arg.Item;

            if (cursorEvent.Item2 && !cursorEvent.Item3)
            {
                var existingEntities = _mapService.GetEntitiesByCoords((int)cursorEvent.Item1.X, (int)cursorEvent.Item1.Y);

                if (existingEntities is null || !existingEntities.Any())
                {
                    var addEntityViewModel = _addEntityViewModel.Copy();
                    addEntityViewModel.X = (byte)cursorEvent.Item1.X;
                    addEntityViewModel.Y = (byte)cursorEvent.Item1.Y;

                    if (_previousPathNodeViewModel is not null && _previousPathNodeViewModel.EqualsTypeAndModel(_addEntityViewModel))
                    {
                        addEntityViewModel.Child = (ushort)_previousPathNodeViewModel.Id;
                    }
                    int id = this.AddEntity(addEntityViewModel);
                    if (_previousPathNodeViewModel is not null && _previousPathNodeViewModel.EqualsTypeAndModel(_addEntityViewModel))
                    {
                        _previousPathNodeViewModel.Parent = (ushort)id;
                        this.UpdateEntity(_previousPathNodeViewModel);
                    }

                    if (addEntityViewModel.IsPathOrWallOrCanyonOrRidge())
                        _previousPathNodeViewModel = addEntityViewModel.Copy();
                }
            }
            else if (cursorEvent.Item3)
            {
                ClearSelection();
                CursorSelected = true;
            }
        }
    }

    private void ClearSelection()
    {
        AddEntityViewModel = null;
        ToggleButtons(false);
        _previousPathNodeViewModel = null;
        _eventAggregator.RaiseEvent("OnToolSelected", this, new PubSubEventArgs<object>(0));
    }

    private void OnEntityTypeSelected(TypeId typeId)
    {
        AddEntityViewModel = new EntityViewModel()
        {
            Id = 0,
            Type = (int)typeId,
            ModelIdx = 0,
            X = 128,
            Y = 128,
            DisId = 0,
            SwitchSize = 0,
            SwitchId = 0,
            Parent = 0,
            Child = 0
        };
        this.RaisePropertyChanged(nameof(ModelIdx));
    }

    public void OnPathTypeClicked(int modelId)
    {
        AddEntityViewModel = new EntityViewModel()
        {
            Id = 0,
            Type = (int)TypeId.Effect,
            Model = modelId,
            X = 128,
            Y = 128,
            DisId = 0,
            SwitchSize = 0,
            SwitchId = 0,
            Parent = 0,
            Child = 0
        };
        this.RaisePropertyChanged(nameof(ModelIdx));
        _eventAggregator.RaiseEvent("OnToolSelected", this, new PubSubEventArgs<object>(modelId));
    }
}
