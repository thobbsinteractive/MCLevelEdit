using MCLevelEdit.Application.Model;
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
        private bool _showEditSwitch;
        private EditWizardsViewModel _editWizardsViewModel;
        private EditWizardViewModel _editWizardViewModel;
        private EditWorldViewModel _editWorldViewModel;
        private EditEntityViewModel _editEntityViewModel;
        private EditSwitchViewModel _editSwitchViewModel;
        private bool _showEditEntities;
        private EditEntitiesViewModel? _editEntitiesViewModel;
        private bool _showEditObjectives;

        /// <summary>A Magic Carpet 2 level's objectives (the Objectives node under the World): one for the editor's life.</summary>
        public EditObjectivesViewModel EditObjectivesViewModel { get; }

        public bool ShowEditObjectives
        {
            get => _showEditObjectives;
            set => this.RaiseAndSetIfChanged(ref _showEditObjectives, value);
        }

        /// <summary>The properties of more than one entity selected together.</summary>
        public EditEntitiesViewModel? EditEntitiesViewModel
        {
            get => _editEntitiesViewModel;
            set => this.RaiseAndSetIfChanged(ref _editEntitiesViewModel, value);
        }

        public bool ShowEditEntities
        {
            get => _showEditEntities;
            set => this.RaiseAndSetIfChanged(ref _showEditEntities, value);
        }

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

        public EditSwitchViewModel EditSwitchViewModel
        {
            get => _editSwitchViewModel;
            set => this.RaiseAndSetIfChanged(ref _editSwitchViewModel, value);
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

        public bool ShowEditSwitch
        {
            get => _showEditSwitch;
            set => this.RaiseAndSetIfChanged(ref _showEditSwitch, value);
        }

        public NodePropertiesViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
        {
            EditWorldViewModel = new EditWorldViewModel(eventAggregator, mapService);
            EditObjectivesViewModel = new EditObjectivesViewModel(eventAggregator, mapService);
            _eventAggregator.RegisterEvent("NodeSelected", NodeSelectedHandler);
            _eventAggregator.RegisterEvent("EntitiesSelected", EntitiesSelectedHandler);
            _eventAggregator.RegisterEvent("UpdateEntity", (sender, args) =>
            {
                // one of the entities selected together moved on the map: its fields kept up to date
                if (ShowEditEntities && args.Item is EntityViewModel entity)
                    EditEntitiesViewModel?.OnEntityUpdated(entity);
            });
        }

        /// <summary>More than one entity selected (their ids): the properties they share, to change them all.</summary>
        public void EntitiesSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            if (arg.Item is not System.Collections.Generic.IList<int> ids)
                return;

            var entities = ids
                .Select(id => _mapService.GetEntity((ushort)id)?.ToEntityViewModel())
                .OfType<EntityViewModel>()
                .ToList();
            if (entities.Count < 2)
                return;

            ShowEditTerrain = false;
            ShowEditEntity = false;
            ShowEditWizard = false;
            ShowEditWizards = false;
            ShowEditSwitch = false;
            ShowEditObjectives = false;
            EditEntitiesViewModel = new EditEntitiesViewModel(_eventAggregator, _mapService, _terrainService, entities);
            ShowEditEntities = true;
        }

        public void NodeSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            ShowEditTerrain = false;
            ShowEditEntity = false;
            ShowEditWizard = false;
            ShowEditWizards = false;
            ShowEditSwitch = false;
            ShowEditEntities = false;
            var wizards = _mapService.GetActiveWizards();

            // the objectives shown, or put away (no picking on the map, no markers) for whatever else is selected
            var selected = arg.Item as Node;
            if (selected is not null && selected.GetType() == typeof(Node) && selected.Name == MapTreeViewModel.ObjectivesNodeName)
            {
                EditObjectivesViewModel.Show();
                ShowEditObjectives = EditObjectivesViewModel.Map is not null;
                return;
            }
            ShowEditObjectives = false;
            EditObjectivesViewModel.Hide();

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
                    ushort id = 0;
                    if (ushort.TryParse(node.Name, out id))
                    {
                        var entityViewModel = _mapService.GetEntity(id)?.ToEntityViewModel();
                        if (entityViewModel is not null)
                        {
                            if (entityViewModel.IsSwitch())
                            {
                                EditSwitchViewModel = new EditSwitchViewModel(_eventAggregator, _mapService, _terrainService, entityViewModel);
                                ShowEditSwitch = true;
                            }
                            else
                            {
                                EditEntityViewModel = new EditEntityViewModel(_eventAggregator, _mapService, _terrainService, entityViewModel);
                                ShowEditEntity = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
