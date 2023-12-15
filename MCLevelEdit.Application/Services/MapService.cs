using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Repository;

namespace MCLevelEdit.Application.Services;

public class MapService : IMapService
{
    private readonly IFilePort _filePort;
    private readonly ITerrainService _terrainService;

    public MapService(ITerrainService terrainService, IFilePort filePort)
    {
        _filePort = filePort;
        _terrainService = terrainService;
    }

    public Task<bool> LoadMapFromFileAsync(string filePath)
    {
        return Task.Run(() =>
        {
            MapRepository.Map = _filePort.LoadMap(filePath).Result;
            MapRepository.Map.Terrain = _terrainService.CalculateMc2Terrain(MapRepository.Map.Terrain.GenerationParameters).Result;
            return true;
        });
    }

    public Task<WriteableBitmap> GenerateBitmapAsync(IList<Entity> entities)
    {
        return Task.Run(() =>
        {
            WriteableBitmap bitmap = new WriteableBitmap(
                new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                new Vector(96, 96), // DPI (dots per inch)
                PixelFormat.Rgba8888);

            BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), bitmap);

            return DrawBitmapAsync(entities, bitmap);
        });
    }

    public Task<WriteableBitmap> DrawBitmapAsync(IList<Entity> entities, WriteableBitmap bitmap)
    {
        return Task.Run(() =>
        {
            foreach (var entity in entities)
            {
                DrawEntity(entity, bitmap);
            }

            //var result = SaveBitmap(bitmap).Result;

            return bitmap;
        });
    }

    public void DrawEntity(Entity entity, WriteableBitmap bitmap)
    {
        using (var fb = bitmap.Lock())
        {
            fb.SetPixel(entity.Position.X, entity.Position.Y, entity.EntityType.Colour);
        }
    }

    public bool CreateNewMap(ushort size = Globals.MAX_MAP_SIZE)
    {
        MapRepository.Map = new Map();
        return true;
    }

    public Map GetMap()
    {
        return MapRepository.Map;
    }

    public bool AddEntity(Entity entity)
    {
        MapRepository.Map.AddEntity(entity);
        return true;
    }

    public bool UpdateEntity(Entity entity)
    {
        throw new NotImplementedException();
    }

    public bool DeleteEntity(Entity entity)
    {
        MapRepository.Map.RemoveEntity(entity);
        return true;
    }
}
