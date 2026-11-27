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

    public static IList<KeyValuePair<int, string>> TypeIds => EntityTypeLists.TypeIds;

    public ViewModelBase(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        _terrainService = terrainService;
    }

    protected int AddEntity(EntityViewModel entityView)
    {
        if (entityView.Type == (int)TypeId.Spell)
            entityView.SwitchId = 1;

        if (entityView.Type == (int)TypeId.Switch)
        {
            entityView.SwitchId = (ushort)(_mapService.GetMaxSwitchId() + 1);
            entityView.SwitchSize = 3;
        }

        if (entityView.IsBuilding())
        {
            entityView.DisId = ushort.MaxValue;
            entityView.SwitchId = ushort.MaxValue;
            entityView.Parent = 1;
        }

        if (entityView.IsPathOrWall() || entityView.IsCanyon() || entityView.IsRidge())
        {
            entityView.DisId = ushort.MaxValue;
            entityView.SwitchId = 1;
        }
        
        int id = _mapService.AddEntity(entityView.ToEntity());
        entityView.Id = id;
        _eventAggregator.RaiseEvent("AddEntity", this, new PubSubEventArgs<object>(entityView));
        return id;
    }

    protected void DeleteEntity(EntityViewModel entityView)
    {
        _mapService.DeleteEntity(entityView.ToEntity());
        _eventAggregator.RaiseEvent("DeleteEntity", this, new PubSubEventArgs<object>(entityView));
    }

    /// <param name="deferTerrain">leave the terrain as it is until IMapService.CommitTerrain (while dragging)</param>
    protected void UpdateEntity(EntityViewModel entityView, bool deferTerrain = false)
    {
        _mapService.UpdateEntity(entityView.ToEntity(), deferTerrain);
        _eventAggregator.RaiseEvent("UpdateEntity", this, new PubSubEventArgs<object>(entityView));
    }
}
