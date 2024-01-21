using Avalonia.Collections;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesTableViewModel : ViewModelBase
    {
        public static IAvaloniaList<EntityViewModel> Entities { get; } = new AvaloniaList<EntityViewModel>();
        public ICommand AddNewEntityCommand { get; }
        public ICommand DeleteEntityCommand { get; }

        public EntitiesTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
        {
            _eventAggregator.RegisterEvent("RefreshEntities", RefreshDataHandler);
            RefreshData();

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                AddEntity(new EntityViewModel()
                {
                    Type = (int)TypeId.Scenary,
                    X = 0,
                    Y = 0,
                    DisId = 0,
                    SwitchSize = 0,
                    SwitchId = 0,
                    Parent = 0,
                    Child = 0
                });
            });

            DeleteEntityCommand = ReactiveCommand.Create<Entity>((entity) =>
            {
                DeleteEntity(entity.ToEntityViewModel());
            });
        }

        private void RefreshDataHandler(object sender, PubSubEventArgs<object> args)
        {
            RefreshData();
        }

        public void RefreshData()
        {
            Entities.Clear();
            var map = _mapService.GetMap();
            Entities.AddRange(map.Entities.ToEntityViewModels());
        }
    }
}
