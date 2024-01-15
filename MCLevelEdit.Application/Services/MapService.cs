using Avalonia.Media.Imaging;
using MCLevelEdit.Application.Extensions;
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

    public async Task<bool> LoadMapFromFileAsync(string filePath)
    {
        MapRepository.Map = _filePort.LoadMap(filePath).Result;
        MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(MapRepository.Map.Terrain.GenerationParameters);
        return true;
    }

    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, IList<Entity> entities)
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < entities.Count; i++)
            {
                DrawEntity(entities[i], bitmap);
            }

            //var result = SaveBitmap(bitmap).Result;

            return bitmap;
        });
    }

    public void DrawEntity(Entity entity, WriteableBitmap bitmap)
    {
        using (var fb = bitmap.Lock())
        {
            for (int sx = 0; sx < Globals.SQUARE_SIZE; sx++)
            {
                for (int sy = 0; sy < Globals.SQUARE_SIZE; sy++)
                {
                    fb.SetPixel((entity.Position.X * Globals.SQUARE_SIZE) + sx, (entity.Position.Y * Globals.SQUARE_SIZE) + sy, entity.EntityType.Colour);
                }
            }
        }
    }

    public async Task<bool> CreateNewMap(ushort size = Globals.MAX_MAP_SIZE)
    {
        MapRepository.Map = new Map();
        await RecalculateTerrain(MapRepository.Map.Terrain.GenerationParameters);
        return true;
    }
    
    public async Task<bool> RecalculateTerrain(GenerationParameters generationParameters)
    {
        MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(generationParameters);
        return true;
    }

    public Map GetMap()
    {
        return MapRepository.Map;
    }

    public Entity? GetEntity(ushort id)
    {
        return MapRepository.Map.Entities.First(e => e.Id == id);
    }

    public bool AddEntity(Entity entity)
    {
        MapRepository.Map.AddEntity(entity);
        return true;
    }

    public bool UpdateEntity(Entity entity)
    {
        MapRepository.Map.UpdateEntity(entity);
        return true;
    }

    public bool DeleteEntity(Entity entity)
    {
        MapRepository.Map.DeleteEntity(entity);
        return true;
    }
}
