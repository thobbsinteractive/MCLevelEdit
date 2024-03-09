using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace MCLevelEdit.ViewModels;

public class MapTreeViewModel : ReactiveObject
{
    private float SELECTION_RADIUS = 4;

    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;
    protected readonly Dictionary<string, Bitmap> _icons;
    protected int _entityFilter;

    public List<KeyValuePair<int, string>> TypeIds { get; } =
        Enum.GetValues(typeof(TypeId))
        .Cast<int>()
        .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
        .ToList();

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
            { "Scenery", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/tree-32.png"))) },
            { "Creature", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/dragon-32.png"))) },
            { "Effect", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/volcano-32.png"))) },
            { "Spell", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/magic-32.png"))) },
            { "Switch", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/switch-32.png"))) },
            { "Weather", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/wind-32.png"))) }
        };

        SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

        _eventAggregator.RegisterEvent("RefreshEntities", RefreshDataHandler);
        _eventAggregator.RegisterEvent("AddEntity", AddEntityHandler);
        _eventAggregator.RegisterEvent("UpdateEntity", UpdateEntityHandler);
        _eventAggregator.RegisterEvent("DeleteEntity", DeleteEntityHandler);
        _eventAggregator.RegisterEvent("RefreshWizards", RefreshWizardsHandler);
        _eventAggregator.RegisterEvent("OnCursorClicked", SelectNodeHandler);

        TypeIds.Insert(0, new KeyValuePair<int, string>(0, ""));
        _entityFilter = 0;
        RefreshData();
    }

    public void OnCboEntityTypeSelectionChanged(int index)
    {
        _entityFilter = index;
        RefreshData();
    }

    private void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshData();
    }

    private void AddEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entityViewModel = (EntityViewModel)args.Item;
        if (entityViewModel is not null)
            AddEntityNode(entityViewModel);
    }

    private void UpdateEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entityViewModel = (EntityViewModel)args.Item;
        if (entityViewModel is not null)
            UpdateEntityNode(entityViewModel);
    }

    private void DeleteEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entityViewModel = (EntityViewModel)args.Item;
        if (entityViewModel is not null)
        {
            SelectedNodes.Clear();
            RefreshEntitiesData();
        }
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
                var entityNode = world?.SubNodes.Select(n => (EntityNode)n).Where(n => n.X == cursorEvent.Item1.X && n.Y == cursorEvent.Item1.Y).FirstOrDefault();

                if(entityNode is null)
                {
                    for (int i = 1; i < SELECTION_RADIUS; i++)
                    {
                        entityNode = world?.SubNodes.Select(n => (EntityNode)n).Where(n => Vector2.Distance(new Vector2((float)cursorEvent.Item1.X, (float)cursorEvent.Item1.Y), new Vector2((float)n.X, (float)n.Y)) < i).FirstOrDefault();
                        if (entityNode is not null)
                            break;
                    }
                }

                if (entityNode is not null)
                {
                    SelectedNodes.Clear();
                    SelectedNodes.Add(entityNode);
                }
            }
        }
    }

    private Bitmap GetIconFromEntity(EntityViewModel entityViewModel)
    {
        if (_icons.ContainsKey(((TypeId)entityViewModel.Type).ToString()))
            return _icons[((TypeId)entityViewModel.Type).ToString()];

        return null;
    }

    private void RefreshData()
    {
        SelectedNodes.Clear();
        Nodes.Clear();

        RefreshWizardsData();
        RefreshEntitiesData();
    }

    private void RefreshEntitiesData()
    {
        var map = _mapService.GetMap();
        var worldNode = Nodes.Where(n => n.Name == "World").FirstOrDefault();
        var entitiesCoords = new ObservableCollection<Node>();

        if (worldNode == null)
        {
            worldNode = new Node(_icons["World"], $"World", "Level", "Level", entitiesCoords);
            Nodes.Add(worldNode);
        }

        worldNode.SubNodes.Clear();

        var squareEntities = map?.Entities?.Where(e => (_entityFilter > 0 ? (int)e.EntityType.TypeId == _entityFilter : true))?.OrderBy(e => e.Id)?.ToEntityViewModels();
        if (squareEntities?.Any() ?? false)
        {
            foreach (var entity in squareEntities)
            {
                worldNode.SubNodes.Add(new EntityNode(this, entity.Id.ToString(), entity.X, entity.Y, GetIconFromEntity(entity), GetEntityNodeTitle(entity), string.Empty, GetEntityNodeToolTip(entity)));
            }
        }
    }

    private void RefreshWizardsData()
    {
        var map = _mapService.GetMap();
        var wizardsNode = Nodes.Where(n => n.Name == "Wizards").FirstOrDefault();
        var nodeWizards = new ObservableCollection<Node>();

        if (wizardsNode == null)
        {
            wizardsNode = new Node(_icons["Wizard"], $"Wizards", "Wizards", "Wizards", nodeWizards);
            Nodes.Add(wizardsNode);
        }

        wizardsNode.SubNodes.Clear();

        foreach (var wizard in map.Wizards)
        {
            if (wizard.IsActive)
                wizardsNode.SubNodes.Add(new Node(null, wizard.Name, wizard.Name, wizard.Name));
        }

    }

    public void DeleteEntityNode(int id)
    {
        var entityViewModel = _mapService?.GetEntity((ushort)id)?.ToEntityViewModel();
        if (entityViewModel is not null)
        {
            _mapService.DeleteEntity(entityViewModel.ToEntity());
            _eventAggregator.RaiseEvent("DeleteEntity", this, new PubSubEventArgs<object>(entityViewModel));
        }
    }

    private void AddEntityNode(EntityViewModel entityViewModel)
    {
        var entityNode = new EntityNode(this, entityViewModel.Id.ToString(), entityViewModel.X, entityViewModel.Y, GetIconFromEntity(entityViewModel), GetEntityNodeTitle(entityViewModel), string.Empty, GetEntityNodeToolTip(entityViewModel));
        var worldNode = GetWorldNode();

        if (_entityFilter == 0 || entityViewModel.Type == _entityFilter)
            worldNode.SubNodes.Add(entityNode);
    }

    private void UpdateEntityNode(EntityViewModel entityViewModel)
    {
        var entityNode = GetEntityNodeById(entityViewModel.Id);

        if (entityNode is not null)
        {
            entityNode.X = entityViewModel.X;
            entityNode.Y = entityViewModel.Y;
            entityNode.Icon = GetIconFromEntity(entityViewModel);
            entityNode.Title = GetEntityNodeTitle(entityViewModel);
            entityNode.Subtitle = GetEntityNodeSubTitle(entityViewModel);
        }

        if (_entityFilter > 0 && (int)entityViewModel.Type != _entityFilter)
        {
            var worldNode = GetWorldNode();
            worldNode.SubNodes.Remove(entityNode);
            SelectedNodes.Clear();
            _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(null));
        }

    }

    private Node GetWorldNode()
    {
        return Nodes?.Where(n => n.Name == "World").FirstOrDefault();
    }

    private EntityNode? GetEntityNodeByCoords(int x, int y)
    {
        var world = GetWorldNode();
        if (world != null)
        {
            var entityNodes = world.SubNodes.Select(n => (EntityNode)n).ToList();
            return entityNodes?.Where(n => n.X == x && n.Y == y).FirstOrDefault();
        }
        return null;
    }

    private EntityNode? GetEntityNodeById(int id)
    {
        var world = GetWorldNode();
        if (world != null)
        {
            return world.SubNodes.Select(n => (EntityNode)n).Where(n => n.Name == id.ToString()).FirstOrDefault();
        }
        return null;
    }

    private string GetEntityNodeTitle(EntityViewModel entityViewModel)
    {
        return $"{string.Format("{0:D4}", entityViewModel.Id)}: {entityViewModel.ToEntity().EntityType.Model.Name}";
    }

    private string GetEntityNodeSubTitle(EntityViewModel entityViewModel)
    {
        return $"{entityViewModel.ToEntity().EntityType.Model.Name}";
    }

    private string GetEntityNodeToolTip(EntityViewModel entityViewModel)
    {
        return $"{string.Format("{0:D4}", entityViewModel.Id)}: ({string.Format("{0:D3}", entityViewModel.X)},{string.Format("{0:D3}", entityViewModel.Y)}): {entityViewModel.ToEntity().EntityType.Model.Name}";
    }

    private void SelectedNodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(SelectedNodes.FirstOrDefault()));
    }
}
