using Avalonia;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;
using System;

namespace MCLevelEdit.ViewModels;

public class MapEditorViewModel : ViewModelBase
{
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

    public MapEditorViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
    {
    }
}
