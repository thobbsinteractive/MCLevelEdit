using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
using System.Threading.Tasks;
using Effect = MCLevelEdit.Model.Domain.Effect;

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

    private bool _pathToolSelected;

    public WriteableBitmap Preview 
    {
        set { this.RaiseAndSetIfChanged(ref _preview, value); }
        get { return _preview; }
    }

    public Canvas CvEntity
    {
        set { this.RaiseAndSetIfChanged(ref _cvEntity, value); }
        get { return _cvEntity; }
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
        if (_selectedEntityViewModel != null)
        {
            DeleteEntity(_selectedEntityViewModel);
            OnEntitySelected(null);
        }
    }

    public void OnMoveSelectedEntity(int moveX, int moveY)
    {
        if (_selectedEntityViewModel != null)
        {
            _selectedEntityViewModel.X += (byte)moveX;
            _selectedEntityViewModel.Y += (byte)moveY;

            UpdateEntity(_selectedEntityViewModel);
        }
    }

    public void OnCursorDragged(Point origin, Point dest)
    {
        if (_selectedEntityViewModel != null)
        {
            var newlocation = GetCursorPointFromCanvasPoint(dest);

            var vStart = new Vector2((float)newlocation.X, (float)newlocation.Y);
            var vEnd = new Vector2(_selectedEntityViewModel.X, _selectedEntityViewModel.Y);
            var shortestDistance = Vector2.Distance(vStart, vEnd);
            if (shortestDistance < 4)
            {
                _selectedEntityViewModel.X = (byte)newlocation.X;
                _selectedEntityViewModel.Y = (byte)newlocation.Y;

                UpdateEntity(_selectedEntityViewModel);
            }
        }
    }

    public void OnCursorClicked(Point position, bool left, bool right)
    {
        if (right)
        {
            _pathOrigin = null;
        }

        if (left && _pathToolSelected)
        {
            _pathOrigin = _cursorPosition;
        }

        (Point, bool, bool) cursorEvent = (position, left, right);
        _eventAggregator.RaiseEvent("OnCursorClicked", this, new PubSubEventArgs<object>(cursorEvent));
    }

    public void OnEntitySelected(EntityViewModel? entity)
    {
        _selectedEntityViewModel = entity;

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
        return new Point(Double.Round(value.X / Globals.SQUARE_SIZE, MidpointRounding.ToZero),
            Double.Round(value.Y / Globals.SQUARE_SIZE, MidpointRounding.ToZero));
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
        _eventAggregator.RegisterEvent("PathToolSelected", (sender, args) =>
        {
            var modelId = (int?)args?.Item;
            _pathToolSelected = modelId > 0;
            _pathOrigin = null;
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

            var rect = new Rect(entity.X * Globals.SQUARE_SIZE - 3, entity.Y * Globals.SQUARE_SIZE - 3, Globals.SQUARE_SIZE + 3, Globals.SQUARE_SIZE + 3);
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

            _horizontalSelectionLeft = new Line()
            {
                StartPoint = new Point((-entity.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), 7),
                EndPoint = new Point(0, 7),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _horizontalSelectionRight = new Line()
            {
                StartPoint = new Point(14, 7),
                EndPoint = new Point((256 * Globals.SQUARE_SIZE) - (entity.X * Globals.SQUARE_SIZE), 7),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelectionTop = new Line()
            {
                StartPoint = new Point(7, (-entity.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                EndPoint = new Point(7, 0),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelectionBottom = new Line()
            {
                StartPoint = new Point(7, 14),
                EndPoint = new Point(7, (256 * Globals.SQUARE_SIZE) - (entity.Y * Globals.SQUARE_SIZE)),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            Canvas.SetLeft(_rectSelection, rect.X);
            Canvas.SetTop(_rectSelection, rect.Y);
            Canvas.SetLeft(_outerRectSelection, rect.X - 11);
            Canvas.SetTop(_outerRectSelection, rect.Y - 11);
            Canvas.SetLeft(_horizontalSelectionLeft, rect.X);
            Canvas.SetTop(_horizontalSelectionLeft, rect.Y);
            Canvas.SetLeft(_horizontalSelectionRight, rect.X);
            Canvas.SetTop(_horizontalSelectionRight, rect.Y);
            Canvas.SetLeft(_verticalSelectionTop, rect.X);
            Canvas.SetTop(_verticalSelectionTop, rect.Y);
            Canvas.SetLeft(_verticalSelectionBottom, rect.X);
            Canvas.SetTop(_verticalSelectionBottom, rect.Y);

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
                StartPoint = new Point((origin.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), origin.Y * Globals.SQUARE_SIZE),
                EndPoint = new Point(end.X * Globals.SQUARE_SIZE, (end.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
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

    private void AddEntityToView(EntityViewModel entityViewModel)
    {
        if (_cvEntity is not null && entityViewModel is not null)
        {
            List<Shape> shapes = new List<Shape>();

            var rect = new Rect(entityViewModel.X * Globals.SQUARE_SIZE, entityViewModel.Y * Globals.SQUARE_SIZE, Globals.SQUARE_SIZE, Globals.SQUARE_SIZE);
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
                var circle = new Ellipse()
                {
                    Width = ((entityViewModel.SwitchSize * Globals.SQUARE_SIZE) * 2) + Globals.SQUARE_SIZE,
                    Height = ((entityViewModel.SwitchSize * Globals.SQUARE_SIZE) * 2) + Globals.SQUARE_SIZE,
                    Stroke = brush,
                    StrokeThickness = 1,
                    ZIndex = 110
                };
                Canvas.SetLeft(circle, rect.X - (entityViewModel.SwitchSize * Globals.SQUARE_SIZE));
                Canvas.SetTop(circle, rect.Y - (entityViewModel.SwitchSize * Globals.SQUARE_SIZE));
                _cvEntity.Children.Add(circle);
                shapes.Add(circle);
            }

            if (_showSwitchConnections && entityViewModel.Type == (int)TypeId.Switch && entityViewModel.SwitchId > 0)
            {
                brush = new SolidColorBrush(entityViewModel.Colour, 0.5);

                var connectedEntities = _mapService.GetEntitiesBySwitchId(entityViewModel.SwitchId, entityViewModel.Id)?.ToEntityViewModels();

                if (connectedEntities?.Any() ?? false)
                {
                    foreach (var connectedEntity in connectedEntities)
                    {
                        var line = new Line()
                        {
                            StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), entityViewModel.Y * Globals.SQUARE_SIZE),
                            EndPoint = new Point(connectedEntity.X * Globals.SQUARE_SIZE, (connectedEntity.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                            Stroke = brush,
                            StrokeThickness = 1,
                            ZIndex = 98
                        };
                        _cvEntity.Children.Add(line);
                        shapes.Add(line);
                    }
                }
            }

            DrawCanyonOrRidge(entityViewModel, shapes);
            DrawPathOrWall(entityViewModel, shapes);
            DrawTeleport(entityViewModel, shapes);
            DrawCastle(entityViewModel, shapes, rect, brush);

            _entityShapes.Add(entityViewModel.Id, shapes);
        }
    }

    private void DrawCastle(EntityViewModel entityViewModel, List<Shape> shapes, Rect rect, SolidColorBrush brush)
    {
        if (entityViewModel.IsSpawn() && entityViewModel.Model != (int)Spawn.Flyer1)
        {
            var map = _mapService.GetMap();
            var wizard = map.Wizards[entityViewModel.Model - 4].ToWizardViewModel();

            for (int i = 1; i < wizard.CastleLevel + 1; i++)
            {
                int width, height;
                GetCastleWidthForWizard(i, out width, out height);
                var castle = new Rectangle()
                {
                    Width = width,
                    Height = height,
                    Stroke = brush,
                    StrokeThickness = 2,
                    ZIndex = 99
                };
                Canvas.SetLeft(castle, rect.X - (width / 2) + (Globals.SQUARE_SIZE / 2));
                Canvas.SetTop(castle, rect.Y - (height / 2) + (Globals.SQUARE_SIZE / 2));

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

            var destinationLine = new Line()
            {
                StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                EndPoint = new Point((entityViewModel.Child * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Parent * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };
            _cvEntity.Children.Add(destinationLine);
            shapes.Add(destinationLine);

            var circle1 = new Ellipse()
            {
                Width = (Globals.SQUARE_SIZE) * 2 + Globals.SQUARE_SIZE,
                Height = (Globals.SQUARE_SIZE) * 2 + Globals.SQUARE_SIZE,
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };
            Canvas.SetLeft(circle1, (entityViewModel.Child * Globals.SQUARE_SIZE) - Globals.SQUARE_SIZE);
            Canvas.SetTop(circle1, (entityViewModel.Parent * Globals.SQUARE_SIZE) - Globals.SQUARE_SIZE);

            _cvEntity.Children.Add(circle1);
            shapes.Add(circle1);

            var circle2 = new Ellipse()
            {
                Width = Globals.SQUARE_SIZE,
                Height = Globals.SQUARE_SIZE,
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 250
            };
            Canvas.SetLeft(circle2, entityViewModel.Child * Globals.SQUARE_SIZE);
            Canvas.SetTop(circle2, entityViewModel.Parent * Globals.SQUARE_SIZE);

            _cvEntity.Children.Add(circle2);
            shapes.Add(circle2);
        }
    }

    private void DrawCanyonOrRidge(EntityViewModel entityViewModel, List<Shape> shapes)
    {
        if (entityViewModel.IsCanyon() || entityViewModel.IsRidge())
        {
            var brush = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0.25);
            var circleBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0.5);

            if (entityViewModel.IsRidge())
            {
                brush = new SolidColorBrush(Color.FromRgb(32, 32, 32), 0.25);
                circleBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32), 0.5);
            }

            var circle = new Ellipse()
            {
                Width = (7 * Globals.SQUARE_SIZE),
                Height = (7 * Globals.SQUARE_SIZE),
                Stroke = brush,
                Fill = circleBrush,
                StrokeThickness = 1,
                ZIndex = 80
            };
            Canvas.SetLeft(circle, (entityViewModel.X * Globals.SQUARE_SIZE) - ((Globals.SQUARE_SIZE * 7) / 2) + (Globals.SQUARE_SIZE / 2));
            Canvas.SetTop(circle, (entityViewModel.Y * Globals.SQUARE_SIZE) - ((Globals.SQUARE_SIZE * 7) / 2) + (Globals.SQUARE_SIZE / 2));
            _cvEntity.Children.Add(circle);
            shapes.Add(circle);

            if (entityViewModel.Child > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Child)?.ToEntityViewModel();
                if (endEntity != null && (endEntity.IsCanyon() || endEntity.IsRidge()))
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line1 = new Line()
                    {
                        StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        EndPoint = new Point((endPoint.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (endPoint.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE * 7,
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
                        StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        EndPoint = new Point((endPoint.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (endPoint.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE * 7,
                        ZIndex = 80
                    };
                    _cvEntity.Children.Add(line2);
                    shapes.Add(line2);
                }
            }
        }
    }

    private void DrawPathOrWall(EntityViewModel entityViewModel, List<Shape> shapes)
    {
        if (entityViewModel.IsPathOrWall())
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(128, 128, 128), 1);

            if (entityViewModel.Model == (int)Effect.Path)
                brush = new SolidColorBrush(Color.FromRgb(90, 60, 40), 1);

            if (entityViewModel.Child > 0)
            {
                var endEntity = _mapService?.GetEntity(entityViewModel.Child)?.ToEntityViewModel();
                if (endEntity != null && endEntity.IsPathOrWall())
                {
                    var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, new Position(entityViewModel.X, entityViewModel.Y), new Position(endEntity.X, endEntity.Y));

                    var line1 = new Line()
                    {
                        StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        EndPoint = new Point((endPoint.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (endPoint.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE,
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
                        StartPoint = new Point((entityViewModel.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (entityViewModel.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        EndPoint = new Point((endPoint.X * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2), (endPoint.Y * Globals.SQUARE_SIZE) + (Globals.SQUARE_SIZE / 2)),
                        Stroke = brush,
                        StrokeThickness = Globals.SQUARE_SIZE,
                        ZIndex = 99
                    };
                    _cvEntity.Children.Add(line2);
                    shapes.Add(line2);
                }
            }
        }
    }

    private void GetCastleWidthForWizard(int castleLevel, out int width, out int height)
    {
        width = (Globals.SQUARE_SIZE) * CalculateCastleWidth(castleLevel) * 2 + Globals.SQUARE_SIZE;
        height = (Globals.SQUARE_SIZE) * CalculateCastleWidth(castleLevel) * 2 + Globals.SQUARE_SIZE;
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
        RefreshEntities();
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

                    Preview = await _terrainService.DrawBitmapAsync(preview, map.Terrain, _selectedLayer);
                }

                this.Log().Debug("Preview refreshed");
            });
        }
    }

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
