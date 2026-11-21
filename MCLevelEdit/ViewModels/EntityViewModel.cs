using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Effect = MCLevelEdit.Model.Domain.MC1.Effect;

namespace MCLevelEdit.ViewModels;

public class EntityViewModel : ObservableObject
{
    private int _type;
    private int _modelIdx;
    private byte _x;
    private byte _y;
    private ushort _disId;
    private ushort _switchSize;
    private ushort _switchId;
    private ushort _parent;
    private ushort _child;
    private ushort _par3;
    private int _rawModel = -1;

    public IAvaloniaList<KeyValuePair<int, string>> ModelTypes { get; init; } = new AvaloniaList<KeyValuePair<int, string>>();

    public int Id { get; set; }

    public int Type
    {
        get { return _type; }
        set
        {
            _type = value;
            PopulateModelTypes();
            OnPropertyChanged(nameof(Type));
        }
    }

    public string TypeName
    {
        get { return Enum.GetName(typeof(TypeId), Type); }
    }

    public int Model
    {
        get 
        {
            if (_modelIdx >= 0 && _modelIdx < ModelTypes.Count)
                return ModelTypes[_modelIdx].Key;
            return _rawModel;
        }
        set
        {
            _rawModel = value;

            if (!ModelTypes.Any())
                PopulateModelTypes();

            for(int i = 0; i < ModelTypes.Count; i++)
            {
                if(ModelTypes[i].Key == value)
                {
                    ModelIdx = i;
                    return;
                }
            }

            // Keep model IDs the editor has no name for yet (e.g. Magic Carpet 2 creatures)
            // instead of silently switching to the first model in the list.
            if (ModelTypes.Any())
            {
                ModelTypes.Add(new KeyValuePair<int, string>(value, $"Unknown{value}"));
                ModelIdx = ModelTypes.Count - 1;
            }
        }
    }

    public int ModelIdx
    {
        get { return _modelIdx; }
        set { SetProperty(ref _modelIdx, value); }
    }

    public string ModelName
    {
        get { return ModelTypes?.Where(m => m.Key == Model)?.Select(m => m.Value).FirstOrDefault() ?? string.Empty; }
    }

    public byte X
    {
        get { return _x; }
        set 
        {
            SetProperty(ref _x, value); 
        }
    }

    public byte Y
    {
        get { return _y; }
        set { SetProperty(ref _y, value); }
    }

    public ushort DisId
    {
        get { return _disId; }
        set { SetProperty(ref _disId, value); }
    }

    public ushort SwitchSize
    {
        get { return _switchSize; }
        set { SetProperty(ref _switchSize, value); }
    }

    public ushort SwitchId
    {
        get { return _switchId; }
        set { SetProperty(ref _switchId, value); }
    }

    public ushort Parent
    {
        get { return _parent; }
        set { SetProperty(ref _parent, value); }
    }

    public ushort Child
    {
        get { return _child; }
        set { SetProperty(ref _child, value); }
    }

    /// <summary>Magic Carpet 2 only.</summary>
    public ushort Par3
    {
        get { return _par3; }
        set { SetProperty(ref _par3, value); }
    }

    public Color Colour
    {
        get 
        { 
            switch((TypeId)Type)
            {
                case TypeId.Creature:
                    return Color.FromRgb(255, 0, 0);
                case TypeId.Effect:
                    return Color.FromRgb(255, 0, 255);
                case TypeId.Scenery:
                    return Color.FromRgb(0, 255, 0);
                case TypeId.Spawn:
                    return Color.FromRgb(255, 255, 0);
                case TypeId.Spell:
                case TypeId.Vase:
                    return Color.FromRgb(128, 0, 128);
                case TypeId.Special:
                    return Color.FromRgb(0, 255, 255);
                case TypeId.Switch:
                    return Color.FromRgb(255, 255, 255);
                case TypeId.Weather:
                    return Color.FromRgb(0, 0, 255);
                default:
                    return Color.FromRgb(0, 0, 0);
            }
        }
    }

    public EntityViewModel() { }

    public EntityViewModel(int id, int type, int model, int modelIdx, byte x, byte y, ushort disId, ushort switchSize, ushort switchId, ushort parent, ushort child)
    {
        Id = id;
        Type = type;
        Model = model;
        ModelIdx = modelIdx;
        X = x;
        Y = y;
        DisId = disId;
        SwitchSize = switchSize;
        SwitchId = switchId;
        Parent = parent;
        Child = child;
    }

    public EntityViewModel Copy()
    {
        return new EntityViewModel()
        {
            Id = this.Id,
            Type = this.Type,
            Model = this.Model,
            X = this.X,
            Y = this.Y,
            DisId = this.DisId,
            SwitchSize = this.SwitchSize,
            SwitchId = this.SwitchId,
            Parent = this.Parent,
            Child = this.Child,
            Par3 = this.Par3
        };
    }

    public bool IsBuilding() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.VillagerBuilding;
    public bool IsPath() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.Path;
    public bool IsWall() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.Wall;
    public bool IsCanyon() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.Canyon;
    public bool IsRidge() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.RidgeNode;
    public bool IsPathOrWall() => IsPath() || IsWall();
    public bool IsPathOrWallOrCanyonOrRidge() => IsPath() || IsWall() || IsCanyon() || IsRidge();
    public bool IsSwitch() => this?.Type == (int)TypeId.Switch;
    public bool IsTeleport() => this?.Type == (int)TypeId.Effect && this?.Model == (int)Effect.Teleport;
    public bool IsSpawn() => this?.Type == (int)TypeId.Spawn;

    private void PopulateModelTypes()
    {
        ModelTypes.Clear();
        var modelTypesList = GetModelTypes(_type);
        if (modelTypesList.Any())
        {
            ModelTypes.AddRange(modelTypesList);
            _modelIdx = 0;
            OnPropertyChanged(nameof(ModelTypes));
            OnPropertyChanged(nameof(ModelIdx));
        }
    }

    private KeyValuePair<int, string>[] GetModelTypes(int type)
    {
        if (type > 0)
        {
            var types = (((TypeId)type).GetEntityFromTypeId()?.ModelTypes ?? Array.Empty<ModelType>())
                .Select(x => new KeyValuePair<int, string>(key: x.Id, value: x.Name))
                .ToArray();

            return types;

        }
        return Array.Empty<KeyValuePair<int, string>>();     
    }
}
