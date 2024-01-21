using Avalonia.Collections;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Collections.Generic;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesTableViewModel : ViewModelBase
    {
        private bool _IsSelected;
        private IList<EntityViewModel>? _selectedEntityViewModels;

        public IAvaloniaList<EntityViewModel> Entities { get; } = new AvaloniaList<EntityViewModel>();
        public ICommand AddNewEntityCommand { get; }
        public ICommand DeleteEntityCommand { get; }

        public bool IsSelected
        {
            set { this.RaiseAndSetIfChanged(ref _IsSelected, value); }
            get { return _IsSelected; }
        }

        public EntitiesTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
        {
            _eventAggregator.RegisterEvent("RefreshEntities", RefreshDataHandler);
            RefreshData();

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                var entityViewModel = new EntityViewModel()
                {
                    Type = (int)TypeId.Scenary,
                    X = 0,
                    Y = 0,
                    DisId = 0,
                    SwitchSize = 0,
                    SwitchId = 0,
                    Parent = 0,
                    Child = 0
                };

                entityViewModel.Id = AddEntity(entityViewModel);
                Entities.Add(entityViewModel);
            });

            DeleteEntityCommand = ReactiveCommand.Create<Entity>((entity) =>
            {
                if (_selectedEntityViewModels?.Count > 0)
                {
                    foreach (var entityViewModel in _selectedEntityViewModels)
                        DeleteEntity(entityViewModel);

                    _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
                }
            });
        }

        public void OnSelectedItemsChanged(object sender, IList<EntityViewModel>? entityViewModels)
        {
            IsSelected = entityViewModels?.Count > 0;
            _selectedEntityViewModels = entityViewModels;
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
