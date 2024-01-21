using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class MapTreeViewModel
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;
    protected readonly Dictionary<string, Bitmap> _icons;

    public ObservableCollection<Node> Nodes { get; } = new ObservableCollection<Node>();
    public ObservableCollection<Node> SelectedNodes { get; } = new ObservableCollection<Node>();

    public MapTreeViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        _terrainService = terrainService;

        _icons = new Dictionary<string, Bitmap>
        {
            { "World", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/world-32.png"))) },
            { "Wizard", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/wizard-32.png"))) },
            { "Spawn", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/magic-carpet-32.png"))) },
            { "Scenary", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/tree-32.png"))) },
            { "Creature", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/dragon-32.png"))) },
            { "Effect", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/volcano-32.png"))) },
            { "Spell", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/magic-32.png"))) },
            { "Switch", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/switch-32.png"))) },
            { "Weather", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/wind-32.png"))) }
        };

        SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

        _eventAggregator.RegisterEvent("RefreshEntities", RefreshDataHandler);
        _eventAggregator.RegisterEvent("AddEntity", AddEntityHandler);
        _eventAggregator.RegisterEvent("RefreshWizards", RefreshWizardsHandler);
        _eventAggregator.RegisterEvent("OnCursorClicked", SelectNodeHandler);
        RefreshData();
    }

    private void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshData();
    }

    private void AddEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entity = (Entity)args.Item;
        if (entity is not null)
            AddEntityNode(entity);
    }

    private void RefreshWizardsHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshWizardsData();
    }

    private void SelectNodeHandler(object sender, PubSubEventArgs<object> arg)
    {
        if(arg.Item is not null)
        {
            var cursorEvent = ((Point, bool, bool))arg.Item;
            var world = Nodes?.Where(n => n.Name == "World").FirstOrDefault();
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

    private Bitmap GetIconFromEntity(EntityType entityType)
    {
        if (_icons.ContainsKey(entityType.TypeId.ToString()))
            return _icons[entityType.TypeId.ToString()];

        return null;
    }

    private void RefreshData()
    {
        SelectedNodes.Clear();
        Nodes.Clear();

        RefreshWizardsData();

        var map = _mapService.GetMap();

        var entitiesCoords = new ObservableCollection<Node>();
        var world = new Node(_icons["World"], $"World", "", entitiesCoords);
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
                        nodeEntities.Add(new Node(GetIconFromEntity(entity.EntityType), entity.Id.ToString(), $"{entity.Id}: {entity.EntityType.Model.Name}"));
                    }
                    entitiesCoords.Add(new CoordNode(x, y, "Coord", $"{string.Format("{0:D3}", x)},{string.Format("{0:D3}", y)}", nodeEntities));
                }
            }
        }
    }

    private void AddEntityNode(Entity entity)
    {
        var entityNode = new Node(GetIconFromEntity(entity.EntityType), entity.Id.ToString(), $"{entity.Id}: {entity.EntityType.Model.Name}");
        var coordNode = GetNodeByCoord(entity.Position.X, entity.Position.Y);
        var worldNode = GetWorldNode();

        if (coordNode is null)
        {
            var entities = new ObservableCollection<Node>();
            entities.Add(entityNode);
            coordNode = new CoordNode(entity.Position.X, entity.Position.Y, "Coord", $"{string.Format("{0:D3}", entity.Position.X)},{string.Format("{0:D3}", entity.Position.Y)}", entities);
        }
        worldNode.SubNodes.Add(coordNode);
  
    }

    private Node GetWorldNode()
    {
        return Nodes?.Where(n => n.Name == "World").FirstOrDefault();
    }

    private CoordNode? GetNodeByCoord(int x, int y)
    {
        var world = GetWorldNode();
        if (world != null)
        {
            var coordNodes = world.SubNodes.Where(n => n.GetType() is CoordNode).Select(n => (CoordNode)n).ToList();
            return coordNodes.Where(n => n.X == x && n.Y == y).FirstOrDefault();
        }
        return null;
    }

    private void RefreshWizardsData()
    {
        var map = _mapService.GetMap();
        var wizardsNode = Nodes.Where(n => n.Name == "Wizards").FirstOrDefault();
        var nodeWizards = new ObservableCollection<Node>();

        if (wizardsNode == null)
        {
            wizardsNode = new Node(_icons["Wizard"], $"Wizards", "", nodeWizards);
            Nodes.Add(wizardsNode);
        }

        wizardsNode.SubNodes.Clear();

        foreach (var wizard in map.Wizards)
        {
            if (wizard.IsActive)
                wizardsNode.SubNodes.Add(new Node(null, wizard.Name, wizard.Name));
        }
        
    }

    private void SelectedNodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(SelectedNodes.FirstOrDefault()));
    }

}
