using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class CreateEntityViewModel : ViewModelBase
    {
        private EntityViewModel _entityView;
        private TypeId _typeId;

        public ICommand AddNewEntityCommand { get; }

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public CreateEntityViewModel(IMapService mapService, ITerrainService terrainService, TypeId typeId) : base(mapService, terrainService)
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

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                EntityView.Id = Entities.Count + 1;
                AddEntity(EntityView);
            });
        }
    }
}
