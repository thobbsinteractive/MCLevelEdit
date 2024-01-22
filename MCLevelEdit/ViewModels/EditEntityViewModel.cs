using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditEntityViewModel : ViewModelBase
    {
        private EntityViewModel _entityView;

        public ICommand AddNewEntityCommand { get; }

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public EditEntityViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, EntityViewModel entityView) : base(eventAggregator, mapService, terrainService)
        {
            EntityView = entityView;
            EntityView.PropertyChanged += EntityView_PropertyChanged;
        }

        public EditEntityViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, TypeId typeId) : base(eventAggregator, mapService, terrainService)
        {
            EntityView = new EntityViewModel()
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
            EntityView.PropertyChanged += EntityView_PropertyChanged;
        }

        private void EntityView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ModelTypes" && EntityView.ModelIdx >= 0)
                this.UpdateEntity(EntityView);
        }
    }
}
