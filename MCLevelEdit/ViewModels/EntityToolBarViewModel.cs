using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Extensions;
using MCLevelEdit.ViewModels.Mappers;
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

        CursorSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            CursorSelected = true;
            ClearSelection();
        });

        CreaturesSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            CreaturesSelected = true;
            OnEntityTypeSelected(TypeId.Creature);
        });

        ScenarySelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            ScenerySelected = true;
            OnEntityTypeSelected(TypeId.Scenery);
        });

        EffectsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            EffectsSelected = true;
            OnEntityTypeSelected(TypeId.Effect);
        });

        SpellsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            SpellsSelected = true;
            OnEntityTypeSelected(TypeId.Spell);
        });

        SwitchesSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            SwitchesSelected = true;
            OnEntityTypeSelected(TypeId.Switch);
        });

        WeathersSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            WeathersSelected = true;
            OnEntityTypeSelected(TypeId.Weather);
        });

        SpawnsSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            SpawnsSelected = true;
            OnEntityTypeSelected(TypeId.Spawn);
        });

        WallSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            WallSelected = true;
            OnPathTypeClicked((int)Effect.Wall);
        });

        PathSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            PathSelected = true;
            OnPathTypeClicked((int)Effect.Path);
        });

        CanyonSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            CanyonSelected = true;
            OnPathTypeClicked((int)Effect.Canyon);
        });

        RidgeSelectedCommand = ReactiveCommand.Create(() =>
        {
            ToggleButtons(false);
            RidgeSelected = true;
            OnPathTypeClicked((int)Effect.RidgeNode);
        });

    }

    private void ToggleButtons(bool enable)
    {
        CursorSelected = enable;
        CreaturesSelected = enable;
        ScenerySelected = enable;
        CursorSelected = enable;
        EffectsSelected = enable;
        SpellsSelected = enable;
        SwitchesSelected = enable;
        WeathersSelected = enable;
        SpawnsSelected = enable;
        WallSelected = enable;
        PathSelected = enable;
        CanyonSelected = enable;
        RidgeSelected = enable;
        //EntityModelType.SelectedIndex = 0;
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
                    _previousPathNodeViewModel = addEntityViewModel.Copy();
                }
                else if (_previousPathNodeViewModel is not null && _previousPathNodeViewModel.Parent == 0 && existingEntities.Any())
                {
                    var parent = existingEntities.Where(e => (int)e.EntityType.TypeId == _previousPathNodeViewModel.Type && (int)e.EntityType.Model.Id == _previousPathNodeViewModel.Model)?.FirstOrDefault()?.ToEntityViewModel();

                    if (parent is not null && _previousPathNodeViewModel.Id != parent.Id)
                    {
                        parent.Child = (ushort)_previousPathNodeViewModel.Id;
                        this.UpdateEntity(parent);
                        _previousPathNodeViewModel.Parent = (ushort)parent.Id;
                        this.UpdateEntity(_previousPathNodeViewModel);

                        ClearSelection();
                    }
                }
            }
            else if (cursorEvent.Item3)
            {
                ClearSelection();
            }
        }
    }

    private void ClearSelection()
    {
        AddEntityViewModel = null;
        _previousPathNodeViewModel = null;
        ToggleButtons(false);
        CursorSelected = true;
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
        AddEntityViewModel.ModelIdx = 0;
    }

    public void OnPathTypeClicked(int modelId)
    {
        AddEntityViewModel = new EntityViewModel()
        {
            Id = 0,
            Type = (int)TypeId.Effect,
            ModelIdx = modelId,
            X = 128,
            Y = 128,
            DisId = 0,
            SwitchSize = 0,
            SwitchId = 0,
            Parent = 0,
            Child = 0
        };
    }
}
