using Avalonia.Media.Imaging;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Model.Abstractions;

public interface IMapService
{
    void DrawEntity(Entity entity, WriteableBitmap bitmap);
    bool CreateNewMap(ushort size = Globals.MAX_MAP_SIZE);
    Task<bool> LoadMapFromFileAsync(string filePath);
    Task<WriteableBitmap> GenerateBitmapAsync(IList<Entity> entities);
    Task<WriteableBitmap> DrawBitmapAsync(IList<Entity> entities, WriteableBitmap bitmap);
    bool AddEntity(Entity entity);
    bool UpdateEntity(Entity entity);
    bool DeleteEntity(Entity entity);
}
