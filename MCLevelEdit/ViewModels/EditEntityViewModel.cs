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
        private TypeId _typeId;

        public ICommand AddNewEntityCommand { get; }

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public EditEntityViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, EntityViewModel entityView) : base(eventAggregator, mapService, terrainService)
        {
            Set(entityView);

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                EntityView.Id = Entities.Count + 1;
                AddEntity(EntityView);
                _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            });
        }

        public EditEntityViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, TypeId typeId) : base(eventAggregator, mapService, terrainService)
        {
            var entityView = new EntityViewModel()
            {
                Id = 0,
                Type = (int)typeId, 
                ModelIdx = 0,
                X = 128,
                Y = 128,
                Parent = 0,
                Child = 0
            };

            Set(entityView);

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                EntityView.Id = Entities.Count + 1;
                AddEntity(EntityView);
                _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            });
        }

        public void Set(EntityViewModel entityViewModel)
        {
            _typeId = (TypeId)entityViewModel.Type;
            _entityView = entityViewModel;
        }
    }
}
