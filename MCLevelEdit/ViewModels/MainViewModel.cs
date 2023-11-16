using MCLevelEdit.Interfaces;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ICommand EditEntitiesCommand { get; }
    public MapViewModel MapViewModel { get; }

    public Interaction<MapViewModel, MapViewModel?> ShowDialog { get; }

    public MainViewModel(IMapService mapService, ITerrainService terrainService, IFileService fileService) : base(mapService, terrainService, fileService)
    {
        MapViewModel = Locator.Current.GetService<MapViewModel>();

        ShowDialog = new Interaction<MapViewModel, MapViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowDialog.Handle(MapViewModel);
        });
    }
}
