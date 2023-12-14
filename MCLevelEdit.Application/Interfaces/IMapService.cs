using Avalonia.Media.Imaging;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Application.Interfaces;

public interface IMapService
{
    void DrawEntity(Entity entity, WriteableBitmap bitmap);
    Map CreateNewMap(ushort size = Globals.MAX_MAP_SIZE);
    Task<WriteableBitmap> GenerateBitmapAsync(Map map);
    Task<WriteableBitmap> DrawBitmapAsync(Map map, WriteableBitmap bitmap);
    bool AddEntity(Entity entity);
}
