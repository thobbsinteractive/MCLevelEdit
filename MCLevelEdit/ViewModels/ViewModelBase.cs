using DynamicData;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;

    public static KeyValuePair<int, string>[] TypeIds { get; } =
        Enum.GetValues(typeof(TypeId))
        .Cast<int>()
        .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
        .ToArray();

    public ViewModelBase(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        _terrainService = terrainService;
    }

    protected int AddEntity(EntityViewModel entityView)
    {
        var entity = entityView.ToEntity();
        if (entity.EntityType.TypeId == TypeId.Spell)
            entity.SwitchId = 1;
        
        int id = _mapService.AddEntity(entity);
        entity.Id = id;
        _eventAggregator.RaiseEvent("AddEntity", this, new PubSubEventArgs<object>(entity));
        return id;
    }

    protected void DeleteEntity(EntityViewModel entityView)
    {
        var entity = entityView.ToEntity();
        _mapService.DeleteEntity(entity);
        _eventAggregator.RaiseEvent("DeleteEntity", this, new PubSubEventArgs<object>(entity));
    }

    protected void UpdateEntity(EntityViewModel entityView)
    {
        var entity = entityView.ToEntity();
        _mapService.UpdateEntity(entity);
        _eventAggregator.RaiseEvent("UpdateEntity", this, new PubSubEventArgs<object>(entity));
    }
}
