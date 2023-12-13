using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using MCLevelEdit.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class EntityView : ObservableObject
{
    private int _type;

    private int _modelIdx;

    public IAvaloniaList<KeyValuePair<int, string>> ModelTypes { get; init; } = new AvaloniaList<KeyValuePair<int, string>>();

    public int Id { get; set; }
    public int Type
    {
        get { return _type; }
        set
        {
            SetProperty(ref _type, value);
            ModelTypes.Clear();
            var modelTypesList = GetModelTypes(_type);
            if (modelTypesList.Any())
            {
                ModelTypes.AddRange(modelTypesList);
                ModelIdx = 0;
                OnPropertyChanged(nameof(ModelTypes));
            }
        }
    }

    public int Model
    {
        get { return ModelTypes[_modelIdx].Key; }
    }

    public int ModelIdx
    {
        get { return _modelIdx; }
        set
        {
            SetProperty(ref _modelIdx, value);
        }
    }

    public int X { get; set; }
    public int Y { get; set; }
    public ushort Parent { get; set; }
    public ushort Child { get; set; }

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
