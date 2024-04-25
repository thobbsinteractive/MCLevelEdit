using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using MCLevelEdit.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EditSwitchViewModel : ViewModelBase
    {
        private EntityViewModel _entityView;
        private IList<EntityViewModel> _connectedEntityViews = new List<EntityViewModel>();
        private bool _selectingConnectedEntities = false;

        public ICommand SelectTriggeredEntityCommand { get; }
        public ICommand PickItemsFromMapCommand { get; }

        public EntityViewModel EntityView
        {
            get => _entityView;
            set => this.RaiseAndSetIfChanged(ref _entityView, value);
        }

        public IList<EntityViewModel> ConnectedEntityViews
        {
            get => _connectedEntityViews;
            set => this.RaiseAndSetIfChanged(ref _connectedEntityViews, value);
        }

        public bool SelectingConnectedEntities
        {
            get => _selectingConnectedEntities;
        }

        public EditSwitchViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, EntityViewModel entityView) : base(eventAggregator, mapService, terrainService)
        {
            _selectingConnectedEntities = false;
            _eventAggregator.RaiseEvent("OnEntitySelectionModeChanged", this, new PubSubEventArgs<object>(_selectingConnectedEntities));
            _eventAggregator.RegisterEvent("OnCursorSelectionClicked", CursorClickedHandler);
            _eventAggregator.RegisterEvent("OnToolSelected", ToolSelectedHandler);
            EntityView = entityView;
            EntityView.PropertyChanged += EntityView_PropertyChanged;
            RefreshConnectedEntities();

            SelectTriggeredEntityCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectConnectedEntities(entityView);
            });

            PickItemsFromMapCommand = ReactiveCommand.Create(() =>
            {
                _selectingConnectedEntities = !_selectingConnectedEntities;
                this.RaisePropertyChanged(nameof(SelectingConnectedEntities));
                _eventAggregator.RaiseEvent("OnEntitySelectionModeChanged", this, new PubSubEventArgs<object>(_selectingConnectedEntities));
            });
        }

        private void RefreshConnectedEntities()
        {
            ConnectedEntityViews = _mapService.GetEntitiesBySwitchId(EntityView.SwitchId, EntityView.Id).ToEntityViewModels();
        }

        private async Task SelectConnectedEntities(EntityViewModel entityView)
        {
            foreach (var view in _connectedEntityViews)
            {
                view.SwitchId = 0;
                view.DisId = 0;
            }
            var newConnectedEntityViews = await MainWindow.I?.MainViewModel.OnSelectEntitiesButtonClickedAsync(_connectedEntityViews);
            foreach (var view in newConnectedEntityViews)
            {
                view.SwitchId = entityView.SwitchId;
                view.DisId = entityView.SwitchId;
            }
            foreach (var view in _connectedEntityViews.Union(newConnectedEntityViews))
            {
                _mapService.UpdateEntity(view.ToEntity());
            }
            UpdateEntity(entityView);
            RefreshConnectedEntities();
        }

        public EditSwitchViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService, TypeId typeId) : base(eventAggregator, mapService, terrainService)
        {
            EntityView = new EntityViewModel()
            {
                Id = 0,
                Type = (int)typeId, 
                ModelIdx = 0,
                X = 128,
                Y = 128,
                DisId = 0,
                SwitchSize = 0,
                SwitchId = 0,
                Parent = 0,
                Child = 0
            };
            EntityView.PropertyChanged += EntityView_PropertyChanged;
        }

        private void CursorClickedHandler(object sender, PubSubEventArgs<object> arg)
        {
            if (arg.Item is not null && _selectingConnectedEntities)
            {
                var cursorEvent = ((Point, bool, bool))arg.Item;

                if (cursorEvent.Item2 || cursorEvent.Item3)
                {
                    var existingEntities = _mapService.GetEntitiesByCoords((int)cursorEvent.Item1.X, (int)cursorEvent.Item1.Y);

                    if (existingEntities is not null && existingEntities.Any())
                    {
                        var view = existingEntities.FirstOrDefault().ToEntityViewModel();
                        if (view.Id != EntityView.Id)
                        {
                            if (cursorEvent.Item2)
                            {
                                view.SwitchId = EntityView.SwitchId;
                                view.DisId = EntityView.SwitchId;
                            }
                            else if (cursorEvent.Item3)
                            {
                                view.SwitchId = 0;
                                view.DisId = 0;
                            }
                            _mapService.UpdateEntity(view.ToEntity());
                            RefreshConnectedEntities();
                        }
                    }
                    UpdateEntity(EntityView);
                }
            }
        }

        private void ToolSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            _selectingConnectedEntities = false;
            this.RaisePropertyChanged(nameof(SelectingConnectedEntities));
        }

        private void EntityView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ModelTypes" && EntityView.ModelIdx >= 0)
                this.UpdateEntity(EntityView);
        }
    }
}
