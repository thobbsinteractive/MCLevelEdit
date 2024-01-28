using MCLevelEdit.Abstractions;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Application.Services;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Linq;

namespace MCLevelEdit.ViewModels
{
    public class NodePropertiesViewModel : ViewModelBase
    {
        private bool _showEditWizards;
        private bool _showEditWizard;
        private bool _showEditTerrain;
        private bool _showEditEntity;
        private EditWizardsViewModel _editWizardsViewModel;
        private EditWizardViewModel _editWizardViewModel;
        private EditWorldViewModel _editWorldViewModel;
        private EditEntityViewModel _editEntityViewModel;

        public EditWizardsViewModel EditWizardsViewModel
        {
            get => _editWizardsViewModel;
            set => this.RaiseAndSetIfChanged(ref _editWizardsViewModel, value);
        }

        public EditWizardViewModel EditWizardViewModel
        {
            get => _editWizardViewModel;
            set => this.RaiseAndSetIfChanged(ref _editWizardViewModel, value);
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

        public bool ShowEditWizard
        {
            get => _showEditWizard;
            set => this.RaiseAndSetIfChanged(ref _showEditWizard, value);
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
            EditWorldViewModel = new EditWorldViewModel(eventAggregator, mapService);
            _eventAggregator.RegisterEvent("NodeSelected", NodeSelectedHandler);
        }

        public void NodeSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            ShowEditTerrain = false;
            ShowEditEntity = false;
            ShowEditWizard = false;
            ShowEditWizards = false;
            var wizards = _mapService.GetActiveWizards();

            if (arg.Item is not null)
            {
                var node = (Node)arg.Item;
                if (node.Name == "World")
                {
                    ShowEditTerrain = true;
                }
                if (node.Name == "Wizards")
                {
                    EditWizardsViewModel = new EditWizardsViewModel(_eventAggregator, _mapService);
                    ShowEditWizards = true;
                }
                if (wizards.Select(w => w.Name).ToList().Contains(node.Name))
                {
                    var wizard = wizards.Where(w => w.Name.Equals(node.Name)).First();
                    EditWizardViewModel = new EditWizardViewModel(_eventAggregator, _mapService, wizard.Name);
                    ShowEditWizard = true;
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
