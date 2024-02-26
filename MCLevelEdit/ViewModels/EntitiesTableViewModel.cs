using Avalonia.Collections;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesTableViewModel : ViewModelBase
    {
        protected bool _IsSelected;
        protected IList<EntityViewModel>? _selectedEntityViewModels;
        protected int _entityFilter;

        public List<KeyValuePair<int, string>> TypeIds { get; } =
            Enum.GetValues(typeof(TypeId))
            .Cast<int>()
            .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
            .ToList();

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
            RefreshData();

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                var entityViewModel = new EntityViewModel()
                {
                    Type = (int)TypeId.Scenery,
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
                }
                RefreshData();
            });

            TypeIds.Insert(0, new KeyValuePair<int, string>(0, ""));
            _entityFilter = 0;
        }

        public void OnSelectedItemsChanged(object sender, IList<EntityViewModel>? entityViewModels)
        {
            IsSelected = entityViewModels?.Count > 0;
            _selectedEntityViewModels = entityViewModels;
        }

        public void OnCboEntityTypeSelectionChanged(int index)
        {
            _entityFilter = index;
            OnSelectedItemsChanged(this, null);
            RefreshData();
        }

        public void OnUnload()
        {
            UpdateEntities();
        }

        public void RefreshData()
        {
            Entities.Clear();
            var map = _mapService.GetMap();
            Entities.AddRange(map.Entities.Where(e => (_entityFilter > 0 ? (int)e.EntityType.TypeId == _entityFilter : true)).ToEntityViewModels());
        }

        private void UpdateEntities()
        {
            foreach (var entityViewModel in Entities)
            {
                var entity = _mapService.GetEntity((ushort)entityViewModel.Id);

                if (entity is not null)
                    _mapService.UpdateEntity(entityViewModel.ToEntity());          
            }

            _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
        }
    }
}
