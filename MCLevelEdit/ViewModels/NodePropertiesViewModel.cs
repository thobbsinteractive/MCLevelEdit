using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;

namespace MCLevelEdit.ViewModels
{
    public class NodePropertiesViewModel : ViewModelBase
    {
        private bool _showEditWizards;
        private bool _showEditTerrain;
        private bool _showEditEntity;
        private EditWizardsViewModel _editWizardsViewModel;
        private EditWorldViewModel _editWorldViewModel;
        private EditEntityViewModel _editEntityViewModel;

        public EditWizardsViewModel EditWizardsViewModel
        {
            get => _editWizardsViewModel;
            set => this.RaiseAndSetIfChanged(ref _editWizardsViewModel, value);
        }

        public EditWorldViewModel EditWorldViewModel
        {
            get => _editWorldViewModel;
            set => this.RaiseAndSetIfChanged(ref _editWorldViewModel, value);
        }

        public EditEntityViewModel EditEntityViewModel
        {
            get => _editEntityViewModel;
            set => this.RaiseAndSetIfChanged(ref _editEntityViewModel, value);
        }

        public bool ShowEditWizards
        {
            get => _showEditWizards;
            set => this.RaiseAndSetIfChanged(ref _showEditWizards, value);
        }

        public bool ShowEditTerrain
        {
            get => _showEditTerrain;
            set => this.RaiseAndSetIfChanged(ref _showEditTerrain, value);
        }

        public bool ShowEditEntity
        {
            get => _showEditEntity;
            set => this.RaiseAndSetIfChanged(ref _showEditEntity, value);
        }

        public NodePropertiesViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
        {
            EditWizardsViewModel = new EditWizardsViewModel(eventAggregator, mapService);
            EditWorldViewModel = new EditWorldViewModel(eventAggregator, mapService);
            _eventAggregator.RegisterEvent("NodeSelected", NodeSelectedHandler);
        }

        public void NodeSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            ShowEditTerrain = false;
            ShowEditEntity = false;
            ShowEditWizards = false;

            if (arg.Item is not null)
            {
                var node = (Node)arg.Item;
                if (node.Name == "World")
                {
                    ShowEditTerrain = true;
                }
                if (node.Name == "Wizards")
                {
                    ShowEditWizards = true;
                }
                else if(node.GetType() == typeof(EntityNode))
                {
                    ShowEditEntity = true;
                    ushort id = 0;
                    if (ushort.TryParse(node.Name, out id))
                    {
                        var entity = _mapService.GetEntity(id);
                        if (entity is not null)
                        {
                            EditEntityViewModel = new EditEntityViewModel(_eventAggregator, _mapService, _terrainService, entity.ToEntityViewModel());
                            ShowEditEntity = true;
                        }
                    }
                }
            }
        }
    }
}
