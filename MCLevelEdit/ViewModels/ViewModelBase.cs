using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using MCLevelEdit.Utils;
using ReactiveUI;
using Splat;
using System.Linq;
using System.Threading.Tasks;

namespace MCLevelEdit.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;

    public AvaloniaList<Entity> Entities => new AvaloniaList<Entity>(Map.Instance.Entities);
    public Map Map { get; set; }

    public ViewModelBase(IMapService mapService, ITerrainService terrainService)
    {
        _mapService = mapService;
        _terrainService = terrainService;

        if (Map.Instance is null)
        {
            Map.Instance = _mapService.CreateNewMap();
        }
        Map = Map.Instance;
    }

    protected async Task RefreshPreviewAsync()
    {
        await Task.Run(async () =>
        {
            this.Log().Debug("Refreshing Preview...");
            if (Map.Preview is not null)
            {
                BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), Map.Preview);
            }

            if (Map.HeightMap is not null)
            {
                this.Log().Debug("Drawing Terrain...");
                Map.Preview = await _terrainService.GenerateBitmapAsync(Map.Instance.HeightMap);
                Map.Preview = await _mapService.DrawBitmapAsync(Map, Map.Preview);
            }
            else
            {
                this.Log().Debug("Drawing Entities...");
                Map.Preview = await _mapService.GenerateBitmapAsync(Map);
            }
            this.Log().Debug("Preview refreshed");
        });
    }

    protected Entity AddEntity(EntityType entityType, Position position, ushort parent = 0, ushort child = 0)
    {
        var newEntity = new Entity()
        {
            Id = Map.Entities.Count(),
            EntityType = entityType,
            Position = position,
            Parent = parent,
            Child = child
        };
        return AddEntity(newEntity);
    }

    protected Entity AddEntity(Entity entity)
    {
        var newEntity = entity.Copy();

        newEntity.PropertyChanged += Entity_PropertyChanged;
        newEntity.Position.PropertyChanged += Entity_PropertyChanged;
        newEntity.EntityType.PropertyChanged += Entity_PropertyChanged;
        newEntity.EntityType.Child.PropertyChanged += Entity_PropertyChanged;

        Map.AddEntity(newEntity);
        this.RaisePropertyChanged(nameof(Entities));
        RefreshPreviewAsync();
        return newEntity;
    }

    protected void DeleteEntity(Entity entity)
    {
        Map.RemoveEntity(entity);
        this.RaisePropertyChanged(nameof(Entities));
        RefreshPreviewAsync();
    }

    protected void Entity_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshPreviewAsync();
    }
}
