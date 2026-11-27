using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
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

    public IList<KeyValuePair<int, string>> TypeIds => EntityTypeLists.TypeIdsWithAll;

    /// <summary>The game the open map is for. Hidden Worlds levels use the Magic Carpet 1 format; they show as Hidden Worlds once marked as such.</summary>
    public string GameName => _mapService.GetMap() switch
    {
        { GameVersion: GameVersion.MagicCarpet2 } => "Magic Carpet 2",
        { IsHiddenWorlds: true } => "Magic Carpet Hidden Worlds",
        _ => "Magic Carpet 1"
    };

    public ObservableCollection<Node> Nodes { get; } = new ObservableCollection<Node>();
    public ObservableCollection<Node> SelectedNodes { get; } = new ObservableCollection<Node>();

    // the selection set from code (SelectEntities): the tree's nodes change without announcing each change, the whole
    // selection is announced once after
    private bool _suppressSelectionChanged;

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
            { "Weather", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/wind-32.png"))) },
            { "Vase", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/magic-32.png"))) },
            { "Special", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/switch-32.png"))) }
        };

        SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

        _eventAggregator.RegisterEvent("RefreshEntities", RefreshDataHandler);
        _eventAggregator.RegisterEvent("AddEntity", AddEntityHandler);
        _eventAggregator.RegisterEvent("UpdateEntity", UpdateEntityHandler);
        _eventAggregator.RegisterEvent("DeleteEntity", DeleteEntityHandler);
        _eventAggregator.RegisterEvent("RefreshWizards", RefreshWizardsHandler);
        _eventAggregator.RegisterEvent("OnCursorClicked", SelectNodeHandler);
        _eventAggregator.RegisterEvent("SelectEntities", SelectEntitiesHandler);

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

            // a right click deselects, wherever it is (NodeSelected with none, so the map and the properties clear too)
            if (cursorEvent.Item3)
            {
                if (SelectedNodes.Count > 0)
                    SelectedNodes.Clear();
                return;
            }

            var world = Nodes?.Where(n => n.Name == "World").FirstOrDefault();
            if(world is not null)
            {
                var entityNodes = world.SubNodes.OfType<EntityNode>().ToList();
                var entityNode = entityNodes.Where(n => n.X == cursorEvent.Item1.X && n.Y == cursorEvent.Item1.Y).FirstOrDefault();

                if(entityNode is null)
                {
                    for (int i = 1; i < SELECTION_RADIUS; i++)
                    {
                        entityNode = entityNodes.Where(n => Vector2.Distance(new Vector2((float)cursorEvent.Item1.X, (float)cursorEvent.Item1.Y), new Vector2((float)n.X, (float)n.Y)) < i).FirstOrDefault();
                        if (entityNode is not null)
                            break;
                    }
                }

                if (entityNode is not null)
                {
                    // the entities on the square the click picked, lowest id first: the first click selects the lowest,
                    // each next click on the square the next one, back round to the lowest after the last
                    var onSquare = entityNodes
                        .Where(n => n.X == entityNode.X && n.Y == entityNode.Y)
                        .OrderBy(n => int.TryParse(n.Name, out int id) ? id : int.MaxValue)
                        .ToList();
                    int current = SelectedNodes.FirstOrDefault() is EntityNode selected ? onSquare.IndexOf(selected) : -1;
                    entityNode = onSquare[(current + 1) % onSquare.Count];

                    SelectedNodes.Clear();
                    SelectedNodes.Add(entityNode);
                }
                else if (cursorEvent.Item2 && SelectedNodes.Count > 0)
                {
                    SelectedNodes.Clear();
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

        this.RaisePropertyChanged(nameof(GameName));
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

        var squareEntities = map?.VisibleEntities.Where(e => (_entityFilter > 0 ? (int)e.EntityType.TypeId == _entityFilter : true))?.OrderBy(e => e.Id)?.ToEntityViewModels();
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
        if (_suppressSelectionChanged)
            return;

        // one node (or none): as it always was
        if (SelectedNodes.Count <= 1)
        {
            _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(SelectedNodes.FirstOrDefault()));
            return;
        }

        // more than one: only the entities can be selected together (not the World, the Wizards or a wizard), the
        // others are taken out of the selection once the tree has finished changing it
        var entityNodes = SelectedNodes.OfType<EntityNode>().ToList();
        if (entityNodes.Count < SelectedNodes.Count)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var others = SelectedNodes.Where(n => n is not EntityNode).ToList();
                if (others.Count == 0 || SelectedNodes.OfType<EntityNode>().Count() == 0)
                    return;
                _suppressSelectionChanged = true;
                foreach (var other in others)
                    SelectedNodes.Remove(other);
                _suppressSelectionChanged = false;
                PublishSelection(SelectedNodes.OfType<EntityNode>().Select(EntityNodeId).ToList());
            });
        }

        if (entityNodes.Count == 0)
            _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(SelectedNodes.FirstOrDefault()));
        else
            PublishSelection(entityNodes.Select(EntityNodeId).ToList());
    }

    private static int EntityNodeId(EntityNode node) => int.TryParse(node.Name, out int id) ? id : -1;

    /// <summary>
    /// The entities asked to be selected (by the box on the map or the entities table): (ids, add to the selection). The
    /// tree's nodes are selected for those it lists (its type filter may hide some); all the ids are selected.
    /// </summary>
    private void SelectEntitiesHandler(object sender, PubSubEventArgs<object> args)
    {
        if (args.Item is not ValueTuple<IList<int>, bool> request)
            return;

        var ids = request.Item1.Where(id => id > 0).ToList();
        if (request.Item2)
            ids = SelectedNodes.OfType<EntityNode>().Select(EntityNodeId).Where(id => id > 0).Concat(ids).Distinct().ToList();

        var world = GetWorldNode();
        _suppressSelectionChanged = true;
        SelectedNodes.Clear();
        if (world is not null)
        {
            var idSet = ids.ToHashSet();
            foreach (var node in world.SubNodes.OfType<EntityNode>().Where(n => idSet.Contains(EntityNodeId(n))))
                SelectedNodes.Add(node);
        }
        _suppressSelectionChanged = false;

        PublishSelection(ids);
    }

    /// <summary>
    /// The selection announced: none or one entity as a selected node (NodeSelected, as when one node is clicked), more
    /// than one as their ids (EntitiesSelected).
    /// </summary>
    private void PublishSelection(IList<int> ids)
    {
        if (ids.Count > 1)
        {
            _eventAggregator.RaiseEvent("EntitiesSelected", this, new PubSubEventArgs<object>(ids));
            return;
        }

        Node? node = null;
        if (ids.Count == 1)
        {
            node = GetEntityNodeById(ids[0]);
            if (node is null)
            {
                // an entity the tree's filter hides: a node for it all the same, to select it by
                var entity = _mapService.GetEntity((ushort)ids[0])?.ToEntityViewModel();
                if (entity is not null)
                    node = new EntityNode(this, entity.Id.ToString(), entity.X, entity.Y, GetIconFromEntity(entity), GetEntityNodeTitle(entity), string.Empty, GetEntityNodeToolTip(entity));
            }
        }
        _eventAggregator.RaiseEvent("NodeSelected", this, new PubSubEventArgs<object>(node));
    }
}
