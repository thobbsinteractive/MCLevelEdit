﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MCLevelEdit.ViewModels;

public class MapEditorViewModel : ViewModelBase, IEnableLogger
{
    private readonly object _lockPreview = new object();
    private Point _cursorPosition = new Point(0, 0);
    private WriteableBitmap _preview = new WriteableBitmap(
            new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
            new Vector(96, 96),
            PixelFormat.Rgba8888);

    private Canvas _cvEntity;

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

    public void OnCursorClicked(object sender, Point position, bool left, bool right)
    {
        if (sender is not null && CvEntity is null)
        {
            CvEntity = (Canvas)sender;
        }
        (Point, bool, bool) cursorEvent = (position, left, right);
        _eventAggregator.RaiseEvent("OnCursorClicked", this, new PubSubEventArgs<object>(cursorEvent));
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
        _eventAggregator.RegisterEvent("RefreshTerrain", RefreshDataHandler);
        RefreshPreviewAsync();
    }

    public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshPreviewAsync();
    }

    public void RefreshEntitiesHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshEntities();
    }

    protected async Task RefreshPreviewAsync()
    {
        lock (_lockPreview)
        {
            Task.Run(async () =>
            {
                this.Log().Debug("Refreshing Preview...");
                BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), Preview);

                var map = _mapService.GetMap();
                if (map.Terrain is not null)
                {
                    this.Log().Debug("Drawing Terrain...");
                    await _terrainService.DrawBitmapAsync(Preview, map.Terrain, Layer.Game);
                }

                this.Log().Debug("Preview refreshed");
            });
        }
    }

    protected void RefreshEntities()
    {
        lock (_lockPreview)
        {
            this.Log().Debug("Refreshing Entities Preview...");

            var children = _cvEntity.Children.Where(c => c.GetType() != typeof(Image)).DefaultIfEmpty();
            _cvEntity.Children.RemoveAll(children);

            for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
            {
                for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
                {
                    var entities = _mapService.GetEntitiesByCoords(x, y);

                    if (entities?.Count() > 0)
                    {
                        var rects = entities.Select(e => new Rect(e.Position.X * Globals.SQUARE_SIZE, e.Position.Y * Globals.SQUARE_SIZE, Globals.SQUARE_SIZE, Globals.SQUARE_SIZE)).ToList();

                        foreach (var rect in rects)
                        {
                            var rectangle = new Rectangle()
                            {
                                Width = 8,
                                Height = 8,
                                Fill = Brushes.Red,
                                ZIndex = 100
                            };

                            Canvas.SetLeft(rectangle, rect.X);
                            Canvas.SetTop(rectangle, rect.Y);
                            _cvEntity.Children.Add(rectangle);
                        }
                    }
                }
            }

            this.Log().Debug("Entities refreshed");
        }
    }
}
