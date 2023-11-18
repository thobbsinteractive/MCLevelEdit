using Avalonia.Media.Imaging;
using MCLevelEdit.DataModel;
using System.Threading.Tasks;

namespace MCLevelEdit.Interfaces;

public interface ITerrainService
{
    public enum Layer
    {
        Game,
        Height
    }

    public Task<Terrain> CalculateTerrain(TerrainGenerationParameters genParams);
    public Task<WriteableBitmap> GenerateBitmapAsync(Terrain terrain, Layer layer);
    public Task<WriteableBitmap> DrawBitmapAsync(Terrain terrain, Layer layer, WriteableBitmap bitmap);
}