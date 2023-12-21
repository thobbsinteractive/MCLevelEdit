using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCLevelEdit.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;

    public static TerrainGenerationParamsViewModel GenerationParameters { get; } = new TerrainGenerationParamsViewModel();
    public static IAvaloniaList<EntityViewModel> Entities { get; } = new AvaloniaList<EntityViewModel>();
    public static WriteableBitmap Preview { get; } = new WriteableBitmap(
                new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                new Vector(96, 96), // DPI (dots per inch)
                PixelFormat.Rgba8888);

    public static KeyValuePair<int, string>[] TypeIds { get; } =
        Enum.GetValues(typeof(TypeId))
        .Cast<int>()
        .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
        .ToArray();

    public ViewModelBase(IMapService mapService, ITerrainService terrainService)
    {
        _mapService = mapService;
        _terrainService = terrainService;
    }

    protected async Task RefreshPreviewAsync()
    {
        await Task.Run(async () =>
        {
            this.Log().Debug("Refreshing Preview...");
            BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), Preview);

            var map = _mapService.GetMap();
            if (map.Terrain is not null)
            {
                this.Log().Debug("Drawing Terrain...");
                await _terrainService.DrawBitmapAsync(Preview, map.Terrain, Layer.Game);
            }

            this.Log().Debug("Drawing Entities...");
            await _mapService.DrawBitmapAsync(Preview, map.Entities);

            this.RaisePropertyChanging(nameof(Preview));
            this.Log().Debug("Preview refreshed");
        });
    }

    protected void AddEntity(EntityViewModel entityView)
    {
        Entities.Add(entityView.Copy());
        _mapService.AddEntity(entityView.ToEntity());
        //this.RaisePropertyChanged(nameof(Entities));
        RefreshPreviewAsync();
    }

    protected void DeleteEntity(EntityViewModel entityView)
    {
        Entities.Remove(entityView);
        _mapService.DeleteEntity(entityView.ToEntity());
        RefreshPreviewAsync();
    }

    protected void LoadEntityViewModels(IEnumerable<EntityViewModel> entitiesViewModels)
    {
        Entities.Clear();
        Entities.AddRange(entitiesViewModels);
    }

    protected void LoadEntities(IEnumerable<Entity> entities)
    {
        foreach(var entity in entities)
        {
            _mapService.UpdateEntity(entity);
        }
    }
}
