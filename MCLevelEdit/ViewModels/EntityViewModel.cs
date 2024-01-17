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

    public IAvaloniaList<KeyValuePair<int, string>> ModelTypes { get; init; } = new AvaloniaList<KeyValuePair<int, string>>();
    public int Id { get; set; }
    public int Type
    {
        get { return _type; }
        set
        {
            SetProperty(ref _type, value);
            PopulateModelTypes();
        }
    }
    public int Model
    {
        
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
    public byte X { get; set; }
    public byte Y { get; set; }
    public ushort Parent { get; set; }
    public ushort Child { get; set; }

    public EntityViewModel Copy()
    {
        return new EntityViewModel()
        {
            Id = this.Id, 
            Type = this.Type,
            ModelIdx = this.ModelIdx,
            X = this.X,
            Y = this.Y,
            Parent = this.Parent,
            Child = this.Child
        };
    }

    private void PopulateModelTypes()
    {
        ModelTypes.Clear();
        var modelTypesList = GetModelTypes(_type);
        if (modelTypesList.Any())
        {
            ModelTypes.AddRange(modelTypesList);
            OnPropertyChanged(nameof(ModelTypes));
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
