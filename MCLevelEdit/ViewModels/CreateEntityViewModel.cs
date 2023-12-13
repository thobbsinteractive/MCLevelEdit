using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using ReactiveUI;
using System.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class CreateEntityViewModel : ViewModelBase
    {
        private EntityView _entityView;

        public EntityView EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public ICommand AddNewEntityCommand { get; }

        public CreateEntityViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            EntityView = new EntityView()
            {
                Id = Map.Entities.Count(),
                Type = (int)TypeId.Spawn, 
                ModelIdx = 0,
                X = 128,
                Y = 128,
                Parent = 0,
                Child = 0
            };

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                //AddEntity(Entity.EntityType, Entity.Position, Entity.Parent, Entity.Child);
            });
        }
    }
}
