using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using MCLevelEdit.Abstractions;
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
    public class SelectEntitiesTableViewModel
    {
        public class SelectableEntityViewModel : EntityViewModel
        {
            protected bool _IsSelected;

            public bool IsSelected
            {
                get { return _IsSelected; }
                set { SetProperty(ref _IsSelected, value); }
            }

            public SelectableEntityViewModel(EntityViewModel entityViewModel) : base (
                entityViewModel.Id, 
                entityViewModel.Type,
                entityViewModel.Model,
                entityViewModel.ModelIdx,
                entityViewModel.X,
                entityViewModel.Y,
                entityViewModel.DisId,
                entityViewModel.SwitchSize,
                entityViewModel.SwitchId,
                entityViewModel.Parent,
                entityViewModel.Child)
            {

            }
        }

        protected readonly IMapService _mapService;
        protected EventAggregator<object> _eventAggregator;
        protected int _entityFilter;

        public new List<KeyValuePair<int, string>> TypeIds { get; } =
            Enum.GetValues(typeof(TypeId))
            .Cast<int>()
            .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
            .ToList();

        public IAvaloniaList<SelectableEntityViewModel> Entities { get; } = new AvaloniaList<SelectableEntityViewModel>();
        public IList<EntityViewModel> SelectedEntities {
            get
            {
                return Entities.Where(e => e.IsSelected).Select(e => (EntityViewModel)e).ToList();
            }
        }

        public SelectEntitiesTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService, IList<EntityViewModel> selectedEntityViewModels)
        {
            _eventAggregator = eventAggregator;
            _mapService = mapService;
            var entityViewModels = _mapService.GetEntities().ToEntityViewModels()?.Select(e => new SelectableEntityViewModel(e)).ToList();

            if (entityViewModels is not null)
            {
                Entities.AddRange(entityViewModels);

                var selectedIds = selectedEntityViewModels?.Select(e => e.Id);

                foreach (var entity in Entities)
                {
                    entity.IsSelected = selectedIds is not null && selectedIds.Contains(entity.Id);
                }
            }

            TypeIds.Insert(0, new KeyValuePair<int, string>(0, ""));
            _entityFilter = 0;
        }

        public void OnCboEntityTypeSelectionChanged(int index)
        {
            _entityFilter = index;
        }
    }
}
