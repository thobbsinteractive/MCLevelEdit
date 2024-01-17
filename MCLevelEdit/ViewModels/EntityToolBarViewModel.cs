using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;

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
            Parent = 0,
            Child = 0
        };
    }
}
