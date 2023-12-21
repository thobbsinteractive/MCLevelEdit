using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesTableViewModel : ViewModelBase
    {
        public ICommand AddNewEntityCommand { get; }
        public ICommand DeleteEntityCommand { get; }

        public EntitiesTableViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                AddEntity(new EntityViewModel()
                {
                    Id = Entities.Count,
                    Type = (int)TypeId.Scenary,
                    X = 0,
                    Y = 0,
                    Parent = 0,
                    Child = 0
                });
            });

            DeleteEntityCommand = ReactiveCommand.Create<Entity>((entity) =>
            {
                DeleteEntity(entity.ToEntityViewModel());
            });
        }
    }
}
