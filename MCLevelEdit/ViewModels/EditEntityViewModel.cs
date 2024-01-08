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
        protected EventAggregator<object> _eventAggregator;

        public ICommand AddNewEntityCommand { get; }

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public EditEntityViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, TypeId typeId) : base(mapService, terrainService)
        {
            _typeId = typeId;
            _entityView = new EntityViewModel()
            {
                Id = 0,
                Type = (int)_typeId, 
                ModelIdx = 0,
                X = 128,
                Y = 128,
                Parent = 0,
                Child = 0
            };

            _eventAggregator = eventAggregator;

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                EntityView.Id = Entities.Count + 1;
                AddEntity(EntityView);
                _eventAggregator.RaiseEvent("RefreshData", this, new PubSubEventArgs<object>("RefreshData"));
            });
        }
    }
}
