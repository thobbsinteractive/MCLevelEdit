﻿using DynamicData;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class MapTreeViewModel
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;

    public ObservableCollection<Node> Nodes { get; }
    public ObservableCollection<Node> SelectedNodes { get; }

    public MapTreeViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        _terrainService = terrainService;
        SelectedNodes = new ObservableCollection<Node>();
        Nodes = new ObservableCollection<Node>();

        _eventAggregator.RegisterEvent("RefreshData", RefreshDataHandler);

        RefreshData();
    }

    public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshData();
    }

    public void RefreshData()
    {
        SelectedNodes.Clear();
        Nodes.Clear();

        var map = _mapService.GetMap();

        var entitiesCoords = new ObservableCollection<Node>();
        var world = new Node($"World", entitiesCoords);

        Nodes.Add(world);

        for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
        {
            for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
            {
                var squareEntities = map.Entities.Where(e => e.Position.X == x && e.Position.Y == y).OrderBy(e => e.EntityType.TypeId);
                if (squareEntities.Any())
                {
                    var nodeEntities = new ObservableCollection<Node>();
                    foreach (var entity in squareEntities)
                    {
                        nodeEntities.Add(new Node($"{entity.EntityType.TypeId}: {entity.Id}"));
                    }
                    entitiesCoords.Add(new Node($"{x},{y}", nodeEntities));
                }
            }
        }
    }
}
