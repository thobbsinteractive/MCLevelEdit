using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class EntityToolBarViewModel : ViewModelBase
{
    private EntityViewModel _addEntityViewModel;
    public EntityViewModel AddEntityViewModel 
    {
        get 
        {
            return _addEntityViewModel;
        }
        set 
        { 
            this.RaiseAndSetIfChanged(ref _addEntityViewModel, value);
        }
    }

    public EntityToolBarViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        _eventAggregator.RegisterEvent("OnCursorClicked", CursorClickedHandler);
    }

    private void CursorClickedHandler(object sender, PubSubEventArgs<object> arg)
    {
        if (arg.Item is not null && _addEntityViewModel is not null)
        {
            var cursorEvent = ((Point, bool, bool))arg.Item;

            var existingEntities = _mapService.GetEntitiesByCoords((int)cursorEvent.Item1.X, (int)cursorEvent.Item1.Y);

            if (existingEntities is null || !existingEntities.Any()) {
                _addEntityViewModel.X = (byte)cursorEvent.Item1.X;
                _addEntityViewModel.Y = (byte)cursorEvent.Item1.Y;
                this.AddEntity(_addEntityViewModel);
            }
        }
    }

    public void ClearSelection()
    {
        AddEntityViewModel = null;
    }

    public void OnEntityTypeSelected(TypeId typeId)
    {
        AddEntityViewModel = new EntityViewModel()
        {
            Id = 0,
            Type = (int)typeId,
            ModelIdx = 0,
            X = 128,
            Y = 128,
            DisId = 0,
            SwitchSize = 0,
            SwitchId = 0,
            Parent = 0,
            Child = 0
        };
        AddEntityViewModel.ModelIdx = 0;
    }
}
