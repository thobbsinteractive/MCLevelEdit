using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using ReactiveUI;

namespace MCLevelEdit.ViewModels
{
    public class NodePropertiesViewModel : ViewModelBase
    {
        private bool _showEditTerrain;
        private bool _showEditEntity;
        private EditTerrainViewModel _editTerrainViewModel;

        public EditTerrainViewModel EditTerrainViewModel
        {
            get => _editTerrainViewModel;
            set => this.RaiseAndSetIfChanged(ref _editTerrainViewModel, value);
        }

        public EditEntityViewModel EditEntityViewModel { get; }

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
            EditTerrainViewModel = new EditTerrainViewModel(eventAggregator, mapService);
            _eventAggregator.RegisterEvent("NodeSelected", NodeSelectedHandler);
        }

        public void NodeSelectedHandler(object sender, PubSubEventArgs<object> arg)
        {
            ShowEditTerrain = false;
            ShowEditEntity = false;

            if (arg.Item is not null)
            {
                var node = (Node)arg.Item;
                if (node.Name == "World")
                {
                    ShowEditTerrain = true;
                }
                else if(node.GetType() is CoordNode)
                {
                    ShowEditEntity = true;
                }
                else
                {
                    ShowEditEntity = true;
                }
            }
        }
    }
}
