using Avalonia;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;

    public static TerrainGenerationParamsViewModel GenerationParameters { get; } = new TerrainGenerationParamsViewModel();
    public static IAvaloniaList<EntityViewModel> Entities { get; } = new AvaloniaList<EntityViewModel>();
    public static WriteableBitmap Preview { get; } = new WriteableBitmap(
                new PixelSize(Globals.MAX_MAP_SIZE * Globals.SQUARE_SIZE, Globals.MAX_MAP_SIZE * Globals.SQUARE_SIZE),
                new Vector(96, 96), // DPI (dots per inch)
                PixelFormat.Rgba8888);

    public static KeyValuePair<int, string>[] TypeIds { get; } =
        Enum.GetValues(typeof(TypeId))
        .Cast<int>()
        .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
        .ToArray();

    public ViewModelBase(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        _terrainService = terrainService;
    }

    protected void AddEntity(EntityViewModel entityView)
    {
        Entities.Add(entityView.Copy());
        _mapService.AddEntity(entityView.ToEntity());
        _eventAggregator.RaiseEvent("RefreshData", this, new PubSubEventArgs<object>("RefreshData"));
    }

    protected void DeleteEntity(EntityViewModel entityView)
    {
        Entities.Remove(entityView);
        _mapService.DeleteEntity(entityView.ToEntity());
        _eventAggregator.RaiseEvent("RefreshData", this, new PubSubEventArgs<object>("RefreshData"));
    }

    protected void LoadEntityViewModels(IEnumerable<EntityViewModel> entitiesViewModels)
    {
        Entities.Clear();
        Entities.AddRange(entitiesViewModels);
        _eventAggregator.RaiseEvent("RefreshData", this, new PubSubEventArgs<object>("RefreshData"));
    }

    protected void LoadEntities(IEnumerable<Entity> entities)
    {
        foreach(var entity in entities)
        {
            _mapService.UpdateEntity(entity);
        }
        _eventAggregator.RaiseEvent("RefreshData", this, new PubSubEventArgs<object>("RefreshData"));
    }
}
