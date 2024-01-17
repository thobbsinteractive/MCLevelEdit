using Avalonia.Media.Imaging;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Model.Abstractions;

public interface IMapService
{
    void DrawEntity(Entity entity, WriteableBitmap bitmap);
    Task<bool> CreateNewMap(ushort size = Globals.MAX_MAP_SIZE);
    Task<bool> LoadMapFromFileAsync(string filePath);
    Task<bool> RecalculateTerrain(GenerationParameters generationParameters);
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, IList<Entity> entities);
    Map GetMap();
    Entity? GetEntity(ushort id);
    List<Entity> GetEntitiesByCoords(int x, int y);
    bool AddEntity(Entity entity);
    bool UpdateEntity(Entity entity);
    bool DeleteEntity(Entity entity);
}
