using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Effect = MCLevelEdit.Model.Domain.Effect;

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
            var modelTypesList = GetModelTypes(_type);

            if (_modelIdx >= 0)
                return modelTypesList[_modelIdx].Key;
            return -1;
        }
        set
        {
            if (!ModelTypes.Any())
                PopulateModelTypes();

            for(int i = 0; i < ModelTypes.Count; i++)
            {
                if(ModelTypes[i].Key == value)
                {
                    ModelIdx = i;
                    break;
                }
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
                    return Color.FromRgb(128, 0, 128);
                case TypeId.Switch:
                    return Color.FromRgb(255, 255, 255);
                case TypeId.Weather:
                    return Color.FromRgb(0, 0, 255);
                default:
                    return Color.FromRgb(0, 0, 0);
            }
        }
    }

    public EntityViewModel Copy()
    {
        return new EntityViewModel()
        {
            Id = this.Id,
            Type = this.Type,
            ModelIdx = this.ModelIdx,
            X = this.X,
            Y = this.Y,
            DisId = this.DisId,
            SwitchSize = this.SwitchSize,
            SwitchId = this.SwitchId,
            Parent = this.Parent,
            Child = this.Child
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
            var types = ((TypeId)type).GetEntityFromTypeId().ModelTypes
                .Select(x => new KeyValuePair<int, string>(key: x.Id, value: x.Name))
                .ToArray();

            return types;

        }
        return Array.Empty<KeyValuePair<int, string>>();     
    }
}
