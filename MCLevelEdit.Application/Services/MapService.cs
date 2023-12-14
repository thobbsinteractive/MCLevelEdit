using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Application.Interfaces;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Application.Services;

public class MapService : IMapService
{
    public Task<WriteableBitmap> GenerateBitmapAsync(Map map)
    {
        return Task.Run(() =>
        {
            WriteableBitmap bitmap = new WriteableBitmap(
                new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                new Vector(96, 96), // DPI (dots per inch)
                PixelFormat.Rgba8888);

            BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(0, 0, 0, 0), bitmap);

            return DrawBitmapAsync(map, bitmap);
        });
    }

    public Task<WriteableBitmap> DrawBitmapAsync(Map map, WriteableBitmap bitmap)
    {
        return Task.Run(() =>
        {
            var Entities = map.Entities;

            foreach (var entity in Entities)
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

    public Map CreateNewMap(ushort size = Globals.MAX_MAP_SIZE)
    {
        var map = new Map();
        return map;
    }

    public bool AddEntity(Entity entity)
    {
        throw new System.NotImplementedException();
    }
}
