using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Effect = MCLevelEdit.Model.Domain.MC1.Effect;
using Vector = Avalonia.Vector;

namespace MCLevelEdit.ViewModels;

public class MapEditorViewModel : ViewModelBase, IEnableLogger
{
    private readonly object _lockPreview = new object();
    private Point _cursorPosition = new Point(0, 0);
    private WriteableBitmap _preview = new WriteableBitmap(
            new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
            new Avalonia.Vector(96, 96),
            PixelFormat.Rgba8888);

    private Layer _selectedLayer = Layer.Game;
    private bool _showSwitchConnections = false;
    private EntityViewModel? _selectedEntityViewModel = null;
    private Point? _pathOrigin = null;

    private Dictionary<int, List<Shape>> _entityShapes = new Dictionary<int, List<Shape>>();

    private Canvas _cvEntity;

    private List<Shape> _selectionCursorShapes = new List<Shape>();
    private List<Shape> _selectedSwitchConnectionShapes = new List<Shape>();

    private bool _pathToolSelected;
    private bool _isInEntitySelectionMode;

    // the isometric view in place of the map: shown, how many quarter turns it is turned, its image, the newest drawing
    // asked for
    private bool _showIsometric = false;
    private int _isometricQuarterTurns = 0;
    private WriteableBitmap? _isometricPreview;
    private int _isometricVersion;
    private readonly object _lockIsometric = new object();

    // the newest isometric drawing's view, and whether its image is of the terrain as it is now (it is not drawn while
    // neither the canvas nor the viewport shows it)
    private TerrainView? _isometricView;
    private bool _isometricCurrent;

    // the viewport in the corner of the map: shows the view the canvas does not (the isometric view, or the map from above)
    private bool _showViewport = true;

    // the entities drawn over the map (their shapes on the canvas shown), or hidden
    private bool _showEntities = true;

    // while the isometric view is shown, how it was drawn: the entities are placed over it and tiles picked in it with
    // this (null for the map from above)
    private TerrainView? _terrainView;


    public WriteableBitmap Preview
    {
        set
        {
            this.RaiseAndSetIfChanged(ref _preview, value);
            this.RaisePropertyChanged(nameof(CanvasImage));
            this.RaisePropertyChanged(nameof(ViewportImage));
        }
        get { return _preview; }
    }

    /// <summary>What the viewport shows: whichever view the map's canvas does not.</summary>
    public WriteableBitmap? ViewportImage => _showIsometric ? _preview : _isometricPreview;

    /// <summary>The viewport in the corner of the map shown (true) or hidden.</summary>
    public bool ShowViewport
    {
        set
        {
            if (_showViewport == value)
                return;
            this.RaiseAndSetIfChanged(ref _showViewport, value);
            this.RaisePropertyChanged(nameof(CanTurnIsometric));
            if (value && !_isometricCurrent)
                RefreshIsometricAsync();
        }
        get { return _showViewport; }
    }

    /// <summary>Whether the isometric view is shown anywhere (the canvas or the viewport), so it can be turned.</summary>
    public bool CanTurnIsometric => _showIsometric || _showViewport;

    /// <summary>What the map's canvas shows under the entities: the map from above, or the isometric view.</summary>
    public WriteableBitmap? CanvasImage => _showIsometric && _terrainView is not null ? _isometricPreview : _preview;

    public Canvas CvEntity
    {
        set
        {
            if (_cvEntity is not null)
                _cvEntity.Children.CollectionChanged -= OnCanvasChildrenChanged;
            this.RaiseAndSetIfChanged(ref _cvEntity, value);
            if (_cvEntity is not null)
                _cvEntity.Children.CollectionChanged += OnCanvasChildrenChanged;
        }
        get { return _cvEntity; }
    }

    /// <summary>
    /// Whether the entities are hidden from the map: their markers, areas, links and the selection. Hidden they can
    /// still be selected and placed with the cursor.
    /// </summary>
    public bool HideEntities
    {
        set
        {
            if (_showEntities == !value)
                return;
            _showEntities = !value;
            this.RaisePropertyChanged(nameof(HideEntities));
            if (_cvEntity is null)
                return;
            foreach (var child in _cvEntity.Children)
                if (child is not Image)
                    child.IsVisible = _showEntities;
        }
        get { return !_showEntities; }
    }

    // the shapes the entities are drawn with (all the canvas but the map's image) shown or hidden as they are added
    private void OnCanvasChildrenChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
            return;
        foreach (var item in e.NewItems)
            if (item is Control control && control is not Image)
                control.IsVisible = _showEntities;
    }

    public EntityViewModel SelectedEntityViewModel
    {
        get { return _selectedEntityViewModel; }
    }

    public bool ShowHeightMap
    {
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedLayer, (value? Layer.Height: Layer.Game));
            RefreshPreviewAsync();
        }
        get { return _selectedLayer == Layer.Height; }
    }

    /// <summary>The isometric view, drawn over the whole canvas of the map in its place (panned and zoomed with it).</summary>
    public WriteableBitmap? IsometricPreview
    {
        set
        {
            this.RaiseAndSetIfChanged(ref _isometricPreview, value);
            this.RaisePropertyChanged(nameof(CanvasImage));
            this.RaisePropertyChanged(nameof(ViewportImage));
        }
        get { return _isometricPreview; }
    }

    /// <summary>
    /// The isometric view in place of the map from above, with the entities on it (true), or the map from above
    /// (false). It is drawn only while it is shown.
    /// </summary>
    public bool ShowIsometric
    {
        set
        {
            if (_showIsometric == value)
                return;
            this.RaiseAndSetIfChanged(ref _showIsometric, value);
            this.RaisePropertyChanged(nameof(CanTurnIsometric));
            if (!value)
            {
                // back to the map from above: the entities and the cursor on its grid again
                _terrainView = null;
                this.RaisePropertyChanged(nameof(CanvasImage));
                this.RaisePropertyChanged(nameof(ViewportImage));
                PlaceEntitiesAgain();
            }
            else if (_isometricCurrent && _isometricView is not null && _isometricPreview is not null)
            {
                // already drawn for the viewport: on the canvas at once, the entities placed over it
                _terrainView = _isometricView;
                this.RaisePropertyChanged(nameof(CanvasImage));
                this.RaisePropertyChanged(nameof(ViewportImage));
                PlaceEntitiesAgain();
            }
            else
            {
                this.RaisePropertyChanged(nameof(ViewportImage));
                RefreshIsometricAsync();
            }
        }
        get { return _showIsometric; }
    }

    /// <summary>
    /// The isometric view's turn for its slider, in quarter turns (0-3): only these fixed angles, 90 degrees apart,
    /// from the first view at 45 degrees.
    /// </summary>
    public double IsometricQuarterTurns
    {
        set
        {
            int turns = Math.Clamp((int)Math.Round(value), 0, 3);
            if (turns == _isometricQuarterTurns)
                return;
            _isometricQuarterTurns = turns;
            this.RaisePropertyChanged(nameof(IsometricQuarterTurns));
            this.RaisePropertyChanged(nameof(IsometricAngle));
            RefreshIsometricAsync();
        }
        get { return _isometricQuarterTurns; }
    }

    /// <summary>How far the isometric view is turned from the first one, in degrees (0, 90, 180, 270).</summary>
    public int IsometricAngle => _isometricQuarterTurns * 90;

    public bool ShowSwitchConnections
    {
        set { 
            this.RaiseAndSetIfChanged(ref _showSwitchConnections, value);
            RefreshEntities();
        }
        get { return _showSwitchConnections; }
    }

    public void OnDeleteSelectedEntity()
    {
        if (!_isInEntitySelectionMode && _selectedEntityViewModel != null)
        {
            DeleteEntity(_selectedEntityViewModel);
            OnEntitySelected(null);
        }
    }

    public void OnMoveSelectedEntity(int moveX, int moveY)
    {
        if (!_isInEntitySelectionMode && _selectedEntityViewModel != null)
        {
            _selectedEntityViewModel.X += (byte)moveX;
            _selectedEntityViewModel.Y += (byte)moveY;

            UpdateEntity(_selectedEntityViewModel);
        }
    }

    public void OnCursorDragged(Point origin, Point dest)
    {
        if (!_isInEntitySelectionMode && _selectedEntityViewModel != null)
        {
            var newlocation = GetCursorPointFromCanvasPoint(dest);

            var vStart = new Vector2((float)newlocation.X, (float)newlocation.Y);
            var vEnd = new Vector2(_selectedEntityViewModel.X, _selectedEntityViewModel.Y);
            var shortestDistance = Vector2.Distance(vStart, vEnd);
            if (shortestDistance < 4)
            {
                _selectedEntityViewModel.X = (byte)newlocation.X;
                _selectedEntityViewModel.Y = (byte)newlocation.Y;

                // the entity moves with the cursor, the terrain (walls, paths, buildings) is built once it is dropped
                UpdateEntity(_selectedEntityViewModel, deferTerrain: true);
            }
        }
    }

    /// <summary>The drag is over (the button released or the capture lost): the terrain is built with the entity where it was dropped.</summary>
    public void OnCursorDragEnded()
    {
        _mapService.CommitTerrain();
    }

    public void OnCursorClicked(Point position, bool left, bool right)
    {
        (Point, bool, bool) cursorEvent = (position, left, right);
        if (!_isInEntitySelectionMode)
        {
            if (right)
            {
                _pathOrigin = null;
            }

            if (left && _pathToolSelected)
            {
                _pathOrigin = _cursorPosition;
            }

            _eventAggregator.RaiseEvent("OnCursorClicked", this, new PubSubEventArgs<object>(cursorEvent));
        }
        else
        {
            _eventAggregator.RaiseEvent("OnCursorSelectionClicked", this, new PubSubEventArgs<object>(cursorEvent));
        }
    }

    public void OnEntitySelected(EntityViewModel? entity)
    {
        _selectedEntityViewModel = entity;

        RefreshSelectedSwitch(entity);
        DrawSelectionCursorForEntity(entity);
    }

    public Point CursorPosition
    {
        get
        {
            return _cursorPosition;
        }
        set
        {
            RemoveEntityShapes(-99);
            _cursorPosition = GetCursorPointFromCanvasPoint(value);

            if (_pathOrigin is not null)
            {
                DrawPathLine((Point)_pathOrigin, _cursorPosition);
            }

            this.RaisePropertyChanged(nameof(CursorPosition));
            this.RaisePropertyChanged(nameof(CursorPositionStr));
        }
    }

    private Point GetCursorPointFromCanvasPoint(Point value)
    {
        var view = _terrainView;
        if (view is null)
            return new Point(Double.Round(value.X / Globals.SQUARE_SIZE, MidpointRounding.ToZero),
                Double.Round(value.Y / Globals.SQUARE_SIZE, MidpointRounding.ToZero));

        // the isometric view maximised: the tile it shows under the cursor (the nearest in front, so not one behind a
        // hill); off the terrain the cursor stays where it was
        var tile = view.TileAtPixel(value.X / CanvasScaleX(view), value.Y / CanvasScaleY(view));
        return tile is null ? _cursorPosition : new Point(tile.Value.X, tile.Value.Y);
    }

    // the image is drawn over the whole canvas (MAX_MAP_SIZE * SQUARE_SIZE square)
    private static double CanvasScaleX(TerrainView view) => (double)(Globals.MAX_MAP_SIZE * Globals.SQUARE_SIZE) / view.OutputWidth;
    private static double CanvasScaleY(TerrainView view) => (double)(Globals.MAX_MAP_SIZE * Globals.SQUARE_SIZE) / view.OutputHeight;

    /// <summary>
    /// The middle of a map tile (or any map point, in tiles) on the canvas: on the map from above, or on the terrain of the
    /// isometric view while it is maximised (points off the map, of lines that wrap, at the height of the tile they wrap to).
    /// </summary>
    private Point TileToCanvas(double x, double y)
    {
        var view = _terrainView;
        if (view is null)
            return new Point((x * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2));
        var (px, py) = view.Project((float)x, (float)y);
        return new Point(px * CanvasScaleX(view), py * CanvasScaleY(view));
    }

    /// <summary>The entities and the selection placed again, on the UI thread (the view they are drawn on changed).</summary>
    private void PlaceEntitiesAgain()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_cvEntity is null)
                return;
            RefreshEntities();
            OnEntitySelected(_selectedEntityViewModel);
        });
    }

    /// <summary>A square on the ground, half the size across in tiles from the middle of the tile, as the canvas shows it.</summary>
    private List<Point> GroundSquare(double x, double y, double half)
    {
        return new List<Point>
        {
            TileToCanvas(x - half, y - half),
            TileToCanvas(x + half, y - half),
            TileToCanvas(x + half, y + half),
            TileToCanvas(x - half, y + half),
        };
    }

    /// <summary>A circle on the ground round the middle of a tile, the radius in tiles, as the canvas shows it.</summary>
    private List<Point> GroundCircle(double x, double y, double radius)
    {
        const int segments = 32;
        var points = new List<Point>(segments);
        for (int i = 0; i < segments; i++)
        {
            double angle = 2 * Math.PI * i / segments;
            points.Add(TileToCanvas(x + radius * Math.Cos(angle), y + radius * Math.Sin(angle)));
        }
        return points;
    }

    public string CursorPositionStr
    {
        get
        {
            return string.Format("Cursor X: {0:000} Y: {0:000}", CursorPosition.X, CursorPosition.Y);
        }
    }

    public MapEditorViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        _eventAggregator.RegisterEvent("RefreshEntities", RefreshEntitiesHandler);
        _eventAggregator.RegisterEvent("AddEntity", AddEntityHandler);
        _eventAggregator.RegisterEvent("UpdateEntity", UpdateEntityHandler);
        _eventAggregator.RegisterEvent("DeleteEntity", DeleteEntityHandler);
        _eventAggregator.RegisterEvent("RefreshTerrain", RefreshDataHandler);
        _eventAggregator.RegisterEvent("NodeSelected", NodeSelectedHandler);
        _eventAggregator.RegisterEvent("KeyPressed", KeyPressedHandler);
        _eventAggregator.RegisterEvent("UpdateWizard", UpdateWizardsHandler);
        _eventAggregator.RegisterEvent("SwitchLayer", (sender, args) =>
        {
            var layer = (Layer?)args?.Item;
            if (layer is not null)
                ShowHeightMap = layer == Layer.Height;
        });
        _eventAggregator.RegisterEvent("ShowConnections", (sender, args) =>
        {
            ShowSwitchConnections = !ShowSwitchConnections;
        });
        _eventAggregator.RegisterEvent("OnToolSelected", (sender, args) =>
        {
            var modelId = (int?)args?.Item;
            _pathToolSelected = modelId > 0;
            _pathOrigin = null;
            _isInEntitySelectionMode = false;
        });
        _eventAggregator.RegisterEvent("OnEntitySelectionModeChanged", (sender, args) =>
        {
            _pathToolSelected = false;
            _pathOrigin = null;
            _isInEntitySelectionMode = (bool?)args?.Item ?? false;
        });
    }

    private void AddEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entity = (EntityViewModel)args.Item;
        if (entity is not null)
        {
            lock (_lockPreview)
            {
                AddEntityToView(entity);
            }
        }
    }

    private void UpdateEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entityViewModel = (EntityViewModel)args.Item;
        if (entityViewModel is not null)
        {
            lock (_lockPreview)
            {
                RemoveEntityShapes(entityViewModel.Id);
                AddEntityToView(entityViewModel);
                if (entityViewModel.IsPathOrWallOrCanyonOrRidge())
                {
                    RefreshWallsAndPaths();
                }
                OnEntitySelected(entityViewModel);
            }
        }
    }

    private void DrawSelectionCursorForEntity(EntityViewModel? entity)
    {

        _cvEntity.Children.RemoveAll(_selectionCursorShapes);
        _selectionCursorShapes.Clear();

        if (entity is not null)
        {
            Rectangle _rectSelection;
            Rectangle _outerRectSelection;
            Line _horizontalSelectionLeft;
            Line _horizontalSelectionRight;
            Line _verticalSelectionTop;
            Line _verticalSelectionBottom;

            // the boxes round the middle of the tile as the preview shows it, the lines along its row and column of the map
            var centre = TileToCanvas(entity.X, entity.Y);
            var rect = new Rect(centre.X - (Globals.SQUARE_SIZE / 2) - 3, centre.Y - (Globals.SQUARE_SIZE / 2) - 3, Globals.SQUARE_SIZE + 3, Globals.SQUARE_SIZE + 3);
            var brush = new SolidColorBrush(Color.FromRgb(255, 255, 255), 1);
            var transBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255), 0.5);

            _rectSelection = new Rectangle()
            {
                Width = 14,
                Height = 14,
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _outerRectSelection = new Rectangle()
            {
                Width = 36,
                Height = 36,
                Stroke = transBrush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            // the lines from the edges of the map to the tile, in canvas coordinates
            _horizontalSelectionLeft = new Line()
            {
                StartPoint = TileToCanvas(0, entity.Y),
                EndPoint = centre,
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _horizontalSelectionRight = new Line()
            {
                StartPoint = centre,
                EndPoint = TileToCanvas(Globals.MAX_MAP_SIZE - 1, entity.Y),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelectionTop = new Line()
            {
                StartPoint = TileToCanvas(entity.X, 0),
                EndPoint = centre,
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelectionBottom = new Line()
            {
                StartPoint = centre,
                EndPoint = TileToCanvas(entity.X, Globals.MAX_MAP_SIZE - 1),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            Canvas.SetLeft(_rectSelection, rect.X);
            Canvas.SetTop(_rectSelection, rect.Y);
            Canvas.SetLeft(_outerRectSelection, rect.X - 11);
            Canvas.SetTop(_outerRectSelection, rect.Y - 11);

            _selectionCursorShapes.Add(_rectSelection);
            _selectionCursorShapes.Add(_outerRectSelection);
            _selectionCursorShapes.Add(_horizontalSelectionLeft);
            _selectionCursorShapes.Add(_horizontalSelectionRight);
            _selectionCursorShapes.Add(_verticalSelectionTop);
            _selectionCursorShapes.Add(_verticalSelectionBottom);

            _cvEntity.Children.AddRange(_selectionCursorShapes);
        }
    }

    private void DrawPathLine(Point origin, Point end)
    {
        if (_cvEntity is not null)
        {
            RemoveEntityShapes(-99);
            List<Shape> shapes = new List<Shape>();

            
            var brush = new SolidColorBrush(Color.FromRgb(255,0,0), 1);

            var line = new Line()
            {
                StartPoint = TileToCanvas(origin.X, origin.Y),
                EndPoint = TileToCanvas(end.X, end.Y),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };
            _cvEntity.Children.Add(line);
            shapes.Add(line);

            _entityShapes.Add(-99, shapes);
        }
    }

    private void UpdateWizardsHandler(object sender, PubSubEventArgs<object> args)
    {

        lock (_lockPreview)
        {
            var wizardEntities = _mapService.GetEntitiesByTypeId(TypeId.Spawn)?.ToEntityViewModels();

            if (wizardEntities?.Any() ?? false)
            {
                foreach (var wizardEntity in wizardEntities)
                {
                    RemoveEntityShapes(wizardEntity.Id);
                    AddEntityToView(wizardEntity);
                }
            }
        }
    }

    private void DeleteEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entityViewModel = (EntityViewModel)args.Item;
        if (entityViewModel is not null)
        {
            lock (_lockPreview)
            {
                RemoveEntityShapes(entityViewModel.Id);
                if (entityViewModel.IsPathOrWallOrCanyonOrRidge())
                {
                    RefreshWallsAndPaths();
                }
                OnEntitySelected(null);
            }
        }
    }

    public void NodeSelectedHandler(object sender, PubSubEventArgs<object> arg)
    {
        bool deselect = true;
        _isInEntitySelectionMode = false;

        if (arg.Item is not null)
        {
            var node = (Node)arg.Item;
            if (node.GetType() == typeof(EntityNode))
            {
                ushort id = 0;
                if (ushort.TryParse(node.Name, out id))
                {
                    var entity = _mapService.GetEntity(id)?.ToEntityViewModel();
                    if (entity is not null)
                    {
                        OnEntitySelected(entity);
                        deselect = false;
                    }
                }
            }
        }

        if (deselect)
            OnEntitySelected(null);
    }

    public void KeyPressedHandler(object sender, PubSubEventArgs<object> arg)
    {
        if (arg.Item is not null)
        {
            var key = (Key)arg.Item;
            switch (key)
            {
                case Key.Delete:
                case Key.Back:
                    OnDeleteSelectedEntity();
                    break;
                case Key.Up:
                    OnMoveSelectedEntity(0, -1);
                    break;
                case Key.Down:
                    OnMoveSelectedEntity(0, 1);
                    break;
                case Key.Left:
                    OnMoveSelectedEntity(-1, 0);
                    break;
                case Key.Right:
                    OnMoveSelectedEntity(1, 0);
                    break;
            }
        }
    }

    private void RemoveEntityShapes(int id)
    {
        if (_entityShapes.ContainsKey(id))
        {
            foreach (var shape in _entityShapes[id])
            {
                _cvEntity.Children.Remove(shape);
            }
            _entityShapes.Remove(id);
        }
    }

    private void RefreshWallsAndPaths()
    {
        var wallEntities = _mapService.GetEntitiesByTypeId(TypeId.Effect)?.Where(e => e.IsPathOrWall() || e.IsCanyonOrRidge())?.ToEntityViewModels();
        if (wallEntities?.Any() ?? false)
        {
            foreach(var wallEntity in wallEntities)
            {
                RemoveEntityShapes(wallEntity.Id);
                AddEntityToView(wallEntity);
            }
        }
    }

    private void RefreshSelectedSwitch(EntityViewModel? entityViewModel)
    {
        _cvEntity.Children.RemoveAll(_selectedSwitchConnectionShapes);

        if (entityViewModel is not null && entityViewModel.IsSwitch() && entityViewModel.SwitchId > 0)
        {
            DrawConnections(entityViewModel, _selectedSwitchConnectionShapes, Color.FromRgb(255,255,0));
        }
    }

    private void AddEntityToView(EntityViewModel entityViewModel)
    {
        if (_cvEntity is not null && entityViewModel is not null)
        {
            List<Shape> shapes = new List<Shape>();

            // the marker on the middle of the tile as the preview shows it (it keeps its size on the screen)
            var centre = TileToCanvas(entityViewModel.X, entityViewModel.Y);
            var rect = new Rect(centre.X - (Globals.SQUARE_SIZE / 2), centre.Y - (Globals.SQUARE_SIZE / 2), Globals.SQUARE_SIZE, Globals.SQUARE_SIZE);
            var brush = new SolidColorBrush(entityViewModel.Colour, 1);
            var rectangle = new Rectangle()
            {
                Width = Globals.SQUARE_SIZE,
                Height = Globals.SQUARE_SIZE,
                Fill = brush,
                ZIndex = 100
            };

            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            _cvEntity.Children.Add(rectangle);
            shapes.Add(rectangle);

            if (entityViewModel.SwitchSize > 0)
            {
                // the switch's area: a circle on the ground, turned and tilted with the preview
                var circle = new Polygon()
                {
                    Points = GroundCircle(entityViewModel.X, entityViewModel.Y, entityViewModel.SwitchSize + 0.5),
                    Stroke = brush,
                    StrokeThickness = 1,
                    ZIndex = 110
                };
                _cvEntity.Children.Add(circle);
                shapes.Add(circle);
            }

            if (_showSwitchConnections && entityViewModel.IsSwitch() && entityViewModel.SwitchId > 0)
            {
                DrawConnections(entityViewModel, shapes, entityViewModel.Colour);
            }

            DrawCanyonOrRidge(entityViewModel, shapes);
            DrawPathOrWall(entityViewModel, shapes);
            DrawTeleport(entityViewModel, shapes);
            DrawCastle(entityViewModel, shapes, rect, brush);

            _entityShapes.Add(entityViewModel.Id, shapes);
        }
    }

    private void DrawConnections(EntityViewModel entityViewModel, List<Shape> shapes, Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color, 0.5);
        var connectedEntities = _mapService.GetEntitiesBySwitchId(entityViewModel.SwitchId, entityViewModel.Id)?.ToEntityViewModels();

        if (connectedEntities?.Any() ?? false)
        {
            foreach (var connectedEntity in connectedEntities)
            {
                var line = new Line()
                {
                    StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                    EndPoint = TileToCanvas(connectedEntity.X, connectedEntity.Y),
                    Stroke = brush,
                    StrokeThickness = 1,
                    ZIndex = 98
                };
                _cvEntity.Children.Add(line);
                shapes.Add(line);
            }
        }
    }

    private void DrawCastle(EntityViewModel entityViewModel, List<Shape> shapes, Rect rect, SolidColorBrush brush)
    {
        // Magic Carpet 2 castles are built into the terrain (at their start level); Magic Carpet's are drawn
        if (entityViewModel.IsSpawn() && entityViewModel.Model != (int)Spawn.Flyer1 && _mapService.GetMap().GameVersion != GameVersion.MagicCarpet2)
        {
            var map = _mapService.GetMap();
            var wizard = map.Wizards[entityViewModel.Model - 4].ToWizardViewModel(map.IsHiddenWorlds);

            for (int i = 1; i < wizard.CastleLevel + 1; i++)
            {
                // the square of each castle level on the ground (its half width in tiles and the middle tile), turned
                // and tilted with the preview
                var castle = new Polygon()
                {
                    Points = GroundSquare(entityViewModel.X, entityViewModel.Y, CalculateCastleWidth(i) + 0.5),
                    Stroke = brush,
                    StrokeThickness = 2,
                    ZIndex = 99
                };

                _cvEntity.Children.Add(castle);
                shapes.Add(castle);
            }
        }
    }

    private void DrawTeleport(EntityViewModel entityViewModel, List<Shape> shapes)
    {
        if (entityViewModel.IsTeleport())
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 128, 255), 1);

            // the destination (Child, Parent) as the preview shows it: the line to it and two circles on the ground
            var destinationLine = new Line()
            {
                StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                EndPoint = TileToCanvas(entityViewModel.Child, entityViewModel.Parent),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };
            _cvEntity.Children.Add(destinationLine);
            shapes.Add(destinationLine);

            var circle1 = new Polygon()
            {
                Points = GroundCircle(entityViewModel.Child, entityViewModel.Parent, 1.5),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };

            _cvEntity.Children.Add(circle1);
            shapes.Add(circle1);

            var circle2 = new Polygon()
            {
                Points = GroundCircle(entityViewModel.Child, entityViewModel.Parent, 0.5),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };

            _cvEntity.Children.Add(circle2);
            shapes.Add(circle2);
        }
    }

    /// <summary>
    /// The links between canyon and ridge nodes, as thin lines: the canyon or ridge itself is in the terrain, which the
    /// editor builds as the game does when the level loads.
    /// </summary>
    private void DrawCanyonOrRidge(EntityViewModel entityViewModel, List<Shape> shapes)
    {
        if (entityViewModel.IsCanyon() || entityViewModel.IsRidge())
        {
            var brush = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0.6);

            if (entityViewModel.IsRidge())
                brush = new SolidColorBrush(Color.FromRgb(160, 140, 110), 0.6);

            if (entityViewModel.Child > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Child)?.ToEntityViewModel();
                if (endEntity != null && (endEntity.IsCanyon() || endEntity.IsRidge()))
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line1 = new Line()
                    {
                        StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                        EndPoint = TileToCanvas(endPoint.X, endPoint.Y),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE / 4,
                        ZIndex = 80
                    };
                    _cvEntity.Children.Add(line1);
                    shapes.Add(line1);
                }
            }
            if (entityViewModel.Parent > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Parent)?.ToEntityViewModel();
                if (endEntity != null && (endEntity.IsCanyon() || endEntity.IsRidge()))
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line2 = new Line()
                    {
                        StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                        EndPoint = TileToCanvas(endPoint.X, endPoint.Y),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE / 4,
                        ZIndex = 80
                    };
                    _cvEntity.Children.Add(line2);
                    shapes.Add(line2);
                }
            }
        }
    }

    /// <summary>
    /// The links between path and wall nodes, as thin lines: the path or wall itself is in the terrain, which the
    /// editor builds as the game does when the level loads.
    /// </summary>
    private void DrawPathOrWall(EntityViewModel entityViewModel, List<Shape> shapes)
    {
        if (entityViewModel.IsPathOrWall())
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(128, 128, 128), 0.6);

            if (entityViewModel.Model == (int)Effect.Path)
                brush = new SolidColorBrush(Color.FromRgb(90, 60, 40), 0.6);

            if (entityViewModel.Child > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Child)?.ToEntityViewModel();
                if (endEntity != null && endEntity.IsPathOrWall())
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line1 = new Line()
                    {
                        StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                        EndPoint = TileToCanvas(endPoint.X, endPoint.Y),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE / 4,
                        ZIndex = 99
                    };
                    _cvEntity.Children.Add(line1);
                    shapes.Add(line1);
                }
            }
            if (entityViewModel.Parent > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Parent)?.ToEntityViewModel();
                if (endEntity != null && endEntity.IsPathOrWall())
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line2 = new Line()
                    {
                        StartPoint = TileToCanvas(entityViewModel.X, entityViewModel.Y),
                        EndPoint = TileToCanvas(endPoint.X, endPoint.Y),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE / 4,
                        ZIndex = 99
                    };
                    _cvEntity.Children.Add(line2);
                    shapes.Add(line2);
                }
            }
        }
    }

    private int CalculateCastleWidth(int castleLevel)
    {
        if (castleLevel > 7)
            return 22;

        switch (castleLevel)
        {
            case 1:
                return 2;
            case 2:
            case 3:
                return 8;
            case 4:
            case 5:
                return 15;
            case 6:
            case 7:
                return 22;
        }
        return 0;
    }

    private Position GetNearestEndPointInMapBounds(ushort mapSize, Position start, Position end)
    {
        var vStart = new Vector2(start.X, start.Y);
        var vEnd = new Vector2(end.X, end.Y);
        var shortestDistance = Vector2.Distance(vStart, vEnd);

        for(int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                var currentDistance = Vector2.Distance(vStart, new Vector2(end.X + (mapSize * x), end.Y + (mapSize * y)));
                if (currentDistance < shortestDistance)
                {
                    shortestDistance = currentDistance;
                    vEnd = new Vector2(end.X + (mapSize * x), end.Y + (mapSize * y));
                }
            }
        }

        return new Position((int)vEnd.X, (int)vEnd.Y);
    }

    public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
         RefreshPreviewAsync();
    }

    public void RefreshEntitiesHandler(object sender, PubSubEventArgs<object> args)
    {
        _isInEntitySelectionMode = false;
        RefreshEntities();
    }

    public WriteableBitmap CreateBitmap(byte[] rgba, int width, int height)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var fb = bitmap.Lock())
        {
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(
                    rgba,
                    y * width * 4,
                    IntPtr.Add(fb.Address, y * fb.RowBytes),
                    width * 4);
            }
        }
        return bitmap;
    }

    protected void RefreshPreviewAsync()
    {
        lock (_lockPreview)
        {
            Task.Run(async () =>
            {
                this.Log().Debug("Refreshing Preview...");

                var map = _mapService.GetMap();
                if (map.Terrain is not null)
                {
                    this.Log().Debug("Drawing Terrain...");
                    var preview = new WriteableBitmap(new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                        new Avalonia.Vector(96, 96),
                        PixelFormat.Rgba8888);

                    BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), preview);

                    Preview = await _terrainService.DrawBitmapAsync(preview, map, _selectedLayer);
                }

                this.Log().Debug("Preview refreshed");
            });
        }
        RefreshIsometricAsync();
    }

    /// <summary>
    /// Draws the isometric view again (the terrain, its layer or its turn changed), unless neither the canvas nor the
    /// viewport shows it, the size of the whole canvas of the map. Its own lock: turning it does not wait for the map from above. Only the newest drawing is
    /// shown, if an older one finishes after it.
    /// </summary>
    protected void RefreshIsometricAsync()
    {
        if (!_showIsometric && !_showViewport)
        {
            _isometricCurrent = false;
            return;
        }

        lock (_lockIsometric)
        {
            float yaw = IsometricTerrainRender.DefaultYawDegrees + IsometricAngle;
            int version = ++_isometricVersion;
            Task.Run(async () =>
            {
                var map = _mapService.GetMap();
                if (map.Terrain?.MapHeightmap_11B4E0 is null)
                    return;

                // the image on the map's canvas with the entities placed over it by its view (and the tiles picked in
                // it), on the UI thread
                var (bitmap, view) = await _terrainService.DrawIsometricViewAsync(map, _selectedLayer, IsometricSize, IsometricSize, yaw);
                Dispatcher.UIThread.Post(() =>
                {
                    if (version != _isometricVersion)
                        return;
                    _isometricView = view;
                    _isometricCurrent = true;
                    if (_showIsometric)
                        _terrainView = view;
                    IsometricPreview = bitmap;
                    if (!_showIsometric || _cvEntity is null)
                        return;
                    RefreshEntities();
                    OnEntitySelected(_selectedEntityViewModel);
                });
            });
        }
    }

    // the isometric view's image: the size of the map's canvas (MAX_MAP_SIZE * SQUARE_SIZE), drawn on it in place of the map
    public const int IsometricSize = Globals.MAX_MAP_SIZE * Globals.SQUARE_SIZE;

    protected void RefreshEntities()
    {
        lock (_lockPreview)
        {
            if (_cvEntity is not null)
            {
                this.Log().Debug("Refreshing Entities Preview...");

                var children = _cvEntity.Children.Where(c => c.GetType() != typeof(Image)).DefaultIfEmpty();
                _cvEntity.Children.RemoveAll(children);
                _entityShapes.Clear();

                for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
                {
                    for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
                    {
                        var entities = _mapService.GetEntitiesByCoords(x, y)?.ToEntityViewModels();

                        if (entities?.Count() > 0)
                        {
                            foreach (var entityViewModel in entities)
                            {
                                AddEntityToView(entityViewModel);
                            }
                        }
                    }
                }
                this.Log().Debug("Entities refreshed");
            }
        }
    }
}
