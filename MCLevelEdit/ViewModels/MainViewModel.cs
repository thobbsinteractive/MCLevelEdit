using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using MCLevelEdit.Views;
using ReactiveUI;
using Splat;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ICommand OpenFileCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand EditEntitiesCommand { get; }
    public MapViewModel MapViewModel { get; }

    public Interaction<MapViewModel, MapViewModel?> ShowDialog { get; }

    public MainViewModel(IMapService mapService, ITerrainService terrainService, IFileService fileService) : base(mapService, terrainService)
    {
        MapViewModel = Locator.Current.GetService<MapViewModel>();

        ShowDialog = new Interaction<MapViewModel, MapViewModel?>();

        EditEntitiesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ShowDialog.Handle(MapViewModel);
        });

        OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(MainView.I);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Map File",
                AllowMultiple = false
            });

            if (files != null && files.Count == 1 && File.Exists(files[0].Path.AbsolutePath))
            {
                var map = await fileService.LoadMapFromFile(files[0].Path.AbsolutePath);

                Map.SetEntities(map.Entities);
                Map.TerrainGenerationParameters.SetParameters(map.TerrainGenerationParameters);
                Map.Terrain = await _terrainService.CalculateMc2Terrain(map.TerrainGenerationParameters);
                RefreshPreviewAsync();
            }
        });
    }
}
