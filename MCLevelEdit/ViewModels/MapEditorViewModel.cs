using Avalonia;
using Avalonia.Media;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using ReactiveUI;
using Splat;
using System;
using System.Threading.Tasks;

namespace MCLevelEdit.ViewModels;

public class MapEditorViewModel : ViewModelBase, IEnableLogger
{
    private readonly object _lockPreview = new object();
    private Point _cursorPosition = new Point(0, 0);
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
        _eventAggregator.RegisterEvent("RefreshData", RefreshDataHandler);
        RefreshPreviewAsync();
    }

    public void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
    {
        RefreshPreviewAsync();
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

                this.Log().Debug("Drawing Entities...");
                await _mapService.DrawBitmapAsync(Preview, map.Entities);

                this.RaisePropertyChanged(nameof(Preview));
                this.Log().Debug("Preview refreshed");
            });
        }
    }
}
