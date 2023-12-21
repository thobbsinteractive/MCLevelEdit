using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class CreateEntityViewModel : ViewModelBase
    {
        private EntityViewModel _entityView;

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public ICommand AddNewEntityCommand { get; }

        public CreateEntityViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            EntityView = new EntityViewModel()
            {
                Id = Entities.Count + 1,
                Type = (int)TypeId.Spawn, 
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
