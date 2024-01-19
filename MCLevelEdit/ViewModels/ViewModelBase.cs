using Avalonia.Collections;
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

    public static IAvaloniaList<EntityViewModel> Entities { get; } = new AvaloniaList<EntityViewModel>();

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

    protected void AddEntity(EntityViewModel entityView)
    {
        Entities.Add(entityView.Copy());
        _mapService.AddEntity(entityView.ToEntity());
        _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
    }

    protected void DeleteEntity(EntityViewModel entityView)
    {
        Entities.Remove(entityView);
        _mapService.DeleteEntity(entityView.ToEntity());
        _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
    }

    protected void LoadEntityViewModels(IEnumerable<EntityViewModel> entitiesViewModels)
    {
        Entities.Clear();
        Entities.AddRange(entitiesViewModels);
        _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
    }

    protected void LoadEntities(IEnumerable<Entity> entities)
    {
        foreach(var entity in entities)
        {
            _mapService.UpdateEntity(entity);
        }
        _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
    }
}
