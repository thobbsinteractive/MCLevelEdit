using Avalonia;
using DynamicData;
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
        SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;
        Nodes = new ObservableCollection<Node>();

        _eventAggregator.RegisterEvent("RefreshData", RefreshDataHandler);
        _eventAggregator.RegisterEvent("OnCursorClicked", SelectNodeHandler);
        RefreshData();
    }

    public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshData();
    }

    public void SelectNodeHandler(object sender, PubSubEventArgs<object> arg)
    {
        if(arg.Item is not null)
        {
            var cursorEvent = ((Point, bool, bool))arg.Item;
            var world = Nodes?.Where(n => n.Title == "World").FirstOrDefault();
            if(world is not null)
            {
                var coordNode = world?.SubNodes.OfType<CoordNode>().Where(n => n.X == cursorEvent.Item1.X && n.Y == cursorEvent.Item1.Y).FirstOrDefault();
                if (coordNode is not null && coordNode.SubNodes is not null && coordNode.SubNodes.Count > 0)
                {
                    SelectedNodes.Clear();
                    SelectedNodes.Add(coordNode.SubNodes[0]);
                }
            }
        }
    }

    public void RefreshData()
    {
        SelectedNodes.Clear();
        Nodes.Clear();

        var map = _mapService.GetMap();

        var entitiesCoords = new ObservableCollection<Node>();
        var world = new Node("avares://MCLevelEdit/Assets/world-32.png", $"World", "", entitiesCoords);

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
                        nodeEntities.Add(new Node("", "Entity", $"{entity.EntityType.TypeId}: {entity.Id}: {entity.EntityType.Model.Name}"));
                    }
                    entitiesCoords.Add(new CoordNode(x, y, "Coord", $"{x},{y}", nodeEntities));
                }
            }
        }
    }

    private void SelectedNodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(SelectedNodes.FirstOrDefault()));
    }

}
