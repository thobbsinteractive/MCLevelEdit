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

    private Dictionary<int, List<Shape>> _entityShapes = new Dictionary<int, List<Shape>>();

    private Canvas _cvEntity;
    private Rectangle _rectSelection;
    private Rectangle _outerRectSelection;
    private Line _horizontalSelection1;
    private Line _horizontalSelection2;
    private Line _verticalSelection1;
    private Line _verticalSelection2;

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

    public Layer SelectedLayer
    {
        set
        { 
            this.RaiseAndSetIfChanged(ref _selectedLayer, value);
            RefreshPreviewAsync();
        }
        get { return _selectedLayer; }
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

    public void OnCursorClicked(Point position, bool left, bool right)
    {
        (Point, bool, bool) cursorEvent = (position, left, right);
        _eventAggregator.RaiseEvent("OnCursorClicked", this, new PubSubEventArgs<object>(cursorEvent));
    }

    public void OnEntitySelected(Entity? entity)
    {
        _selectedEntityViewModel = entity?.ToEntityViewModel();

        if (_rectSelection is not null)
            _cvEntity.Children.Remove(_rectSelection);

        if (_outerRectSelection is not null)
            _cvEntity.Children.Remove(_outerRectSelection);

        if (_horizontalSelection1 is not null)
            _cvEntity.Children.Remove(_horizontalSelection1);

        if (_horizontalSelection2 is not null)
            _cvEntity.Children.Remove(_horizontalSelection2);

        if (_verticalSelection1 is not null)
            _cvEntity.Children.Remove(_verticalSelection1);

        if (_verticalSelection2 is not null)
            _cvEntity.Children.Remove(_verticalSelection2);

        if (entity is not null)
        {
            var rect = new Rect(entity.Position.X * Globals.SQUARE_SIZE - 3, entity.Position.Y * Globals.SQUARE_SIZE - 3, Globals.SQUARE_SIZE + 3, Globals.SQUARE_SIZE + 3);
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

            _horizontalSelection1 = new Line()
            {
                StartPoint = new Point(-16, 7),
                EndPoint = new Point(0, 7),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _horizontalSelection2 = new Line()
            {
                StartPoint = new Point(14, 7),
                EndPoint = new Point(30, 7),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelection1 = new Line()
            {
                StartPoint = new Point(7, -16),
                EndPoint = new Point(7, 0),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            _verticalSelection2 = new Line()
            {
                StartPoint = new Point(7, 14),
                EndPoint = new Point(7, 30),
                Stroke = brush,
                StrokeThickness = 2,
                ZIndex = 200
            };

            Canvas.SetLeft(_rectSelection, rect.X);
            Canvas.SetTop(_rectSelection, rect.Y);
            Canvas.SetLeft(_outerRectSelection, rect.X - 11);
            Canvas.SetTop(_outerRectSelection, rect.Y - 11);
            Canvas.SetLeft(_horizontalSelection1, rect.X);
            Canvas.SetTop(_horizontalSelection1, rect.Y);
            Canvas.SetLeft(_horizontalSelection2, rect.X);
            Canvas.SetTop(_horizontalSelection2, rect.Y);
            Canvas.SetLeft(_verticalSelection1, rect.X);
            Canvas.SetTop(_verticalSelection1, rect.Y);
            Canvas.SetLeft(_verticalSelection2, rect.X);
            Canvas.SetTop(_verticalSelection2, rect.Y);

            _cvEntity.Children.Add(_rectSelection);
            _cvEntity.Children.Add(_outerRectSelection);
            _cvEntity.Children.Add(_horizontalSelection1);
            _cvEntity.Children.Add(_horizontalSelection2);
            _cvEntity.Children.Add(_verticalSelection1);
            _cvEntity.Children.Add(_verticalSelection2);
        }
    }

    public Point CursorPosition { 
        get 
        { 
            return _cursorPosition; 
        }
        set
        {
            _cursorPosition = new Point(Double.Round(value.X / Globals.SQUARE_SIZE, MidpointRounding.ToZero), 
                Double.Round(value.Y / Globals.SQUARE_SIZE, MidpointRounding.ToZero));
            this.RaisePropertyChanged(nameof(CursorPosition));
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
    }

    private void AddEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entity = (Entity)args.Item;
        if (entity is not null)
        {
            lock (_lockPreview)
            {
                AddEntity(entity);
            }
        }
    }

    private void UpdateEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entity = (Entity)args.Item;
        if (entity is not null)
        {
            lock (_lockPreview)
            {
                if (_entityShapes.ContainsKey(entity.Id))
                {
                    foreach (var shape in _entityShapes[entity.Id])
                    {
                        _cvEntity.Children.Remove(shape);
                    }
                    _entityShapes.Remove(entity.Id);
                }
                AddEntity(entity);
                OnEntitySelected(entity);
            }
        }
    }

    private void DeleteEntityHandler(object sender, PubSubEventArgs<object> args)
    {
        var entity = (Entity)args.Item;
        if (entity is not null)
        {
            lock (_lockPreview)
            {
                if (_entityShapes.ContainsKey(entity.Id))
                {
                    foreach (var shape in _entityShapes[entity.Id])
                    {
                        _cvEntity.Children.Remove(shape);
                    }
                    _entityShapes.Remove(entity.Id);
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
                    var entity = _mapService.GetEntity(id);
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
            if (key == Key.Delete)
            {
                OnDeleteSelectedEntity();
            }
        }
    }

    private void AddEntity(Entity entity)
    {
        if (_cvEntity is not null && entity is not null)
        {
            List<Shape> shapes = new List<Shape>();

            var rect = new Rect(entity.Position.X * Globals.SQUARE_SIZE, entity.Position.Y * Globals.SQUARE_SIZE, Globals.SQUARE_SIZE, Globals.SQUARE_SIZE);
            var brush = new SolidColorBrush(entity.EntityType.Colour, 1);
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

            if (entity.SwitchSize > 0)
            {
                var circle = new Ellipse()
                {
                    Width = ((entity.SwitchSize * Globals.SQUARE_SIZE) * 2) + Globals.SQUARE_SIZE,
                    Height = ((entity.SwitchSize * Globals.SQUARE_SIZE) * 2) + Globals.SQUARE_SIZE,
                    Stroke = brush,
                    StrokeThickness = 1,
                    ZIndex = 110
                };
                Canvas.SetLeft(circle, rect.X - (entity.SwitchSize * Globals.SQUARE_SIZE));
                Canvas.SetTop(circle, rect.Y - (entity.SwitchSize * Globals.SQUARE_SIZE));
                _cvEntity.Children.Add(circle);
                shapes.Add(circle);
            }

            if (_showSwitchConnections && entity.EntityType.TypeId == TypeId.Switch && entity.SwitchId > 0)
            {
                brush = new SolidColorBrush(entity.EntityType.Colour, 0.5);

                var connectedEntities = _mapService.GetEntitiesBySwitchId(entity.SwitchId);

                if (connectedEntities != null && connectedEntities.Any())
                {
                    foreach(var connectedEntity in connectedEntities)
                    {
                        var line = new Line()
                        {
                            StartPoint = new Point(entity.Position.X * Globals.SQUARE_SIZE, entity.Position.Y * Globals.SQUARE_SIZE),
                            EndPoint = new Point(connectedEntity.Position.X * Globals.SQUARE_SIZE, connectedEntity.Position.Y * Globals.SQUARE_SIZE),
                            Stroke = brush,
                            StrokeThickness = 1,
                            ZIndex = 98
                        };
                        _cvEntity.Children.Add(line);
                        shapes.Add(line);
                    }
                }
            }

            if (entity.IsPathOrWall())
            {
                brush = new SolidColorBrush(Color.FromRgb(128,128,128), 1);

                if (entity.EntityType.Model.Id == (int)Effect.Path)
                    brush = new SolidColorBrush(Color.FromRgb(90, 60, 40), 1);

                if (entity.Child > 0)
                {
                    var endEntity = _mapService.GetEntity(entity.Child);
                    if (endEntity != null && endEntity.IsPathOrWall())
                    {
                        var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, entity.Position, endEntity.Position);

                        var line1 = new Line()
                        {
                            StartPoint = new Point(entity.Position.X * Globals.SQUARE_SIZE, entity.Position.Y * Globals.SQUARE_SIZE),
                            EndPoint = new Point(endPoint.X * Globals.SQUARE_SIZE, endPoint.Y * Globals.SQUARE_SIZE),
                            Stroke = brush,
                            StrokeThickness = Globals.SQUARE_SIZE,
                            ZIndex = 99
                        };
                        _cvEntity.Children.Add(line1);
                        shapes.Add(line1);
                    }
                }
                if (entity.Parent > 0)
                {
                    var endEntity = _mapService.GetEntity(entity.Parent);
                    if (endEntity != null && endEntity.IsPathOrWall())
                    {
                        var endPoint = GetNearestEndPointInMapBounds(Globals.MAX_MAP_SIZE, entity.Position, endEntity.Position);

                        var line2 = new Line()
                        {
                            StartPoint = new Point(entity.Position.X * Globals.SQUARE_SIZE, entity.Position.Y * Globals.SQUARE_SIZE),
                            EndPoint = new Point(endPoint.X * Globals.SQUARE_SIZE, endPoint.Y * Globals.SQUARE_SIZE),
                            Stroke = brush,
                            StrokeThickness = Globals.SQUARE_SIZE,
                            ZIndex = 99
                        };
                        _cvEntity.Children.Add(line2);
                        shapes.Add(line2);
                    }
                }
            }

            _entityShapes.Add(entity.Id, shapes);
        }
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
                        var entities = _mapService.GetEntitiesByCoords(x, y);

                        if (entities?.Count() > 0)
                        {
                            foreach (var entity in entities)
                            {
                                AddEntity(entity);
                            }
                        }
                    }
                }
                this.Log().Debug("Entities refreshed");
            }
        }
    }
}
