using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;

namespace MCLevelEdit.ViewModels
{
    public class NodePropertiesViewModel : ViewModelBase
    {
        private bool _showEditTerrain;
        private bool _showEditEntity;
        private EditTerrainViewModel _editTerrainViewModel;
        private EditEntityViewModel _editEntityViewModel;

        public EditTerrainViewModel EditTerrainViewModel
        {
            get => _editTerrainViewModel;
            set => this.RaiseAndSetIfChanged(ref _editTerrainViewModel, value);
        }

        public EditEntityViewModel EditEntityViewModel
        {
            get => _editEntityViewModel;
            set => this.RaiseAndSetIfChanged(ref _editEntityViewModel, value);
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
