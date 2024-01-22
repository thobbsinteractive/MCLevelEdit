using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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
