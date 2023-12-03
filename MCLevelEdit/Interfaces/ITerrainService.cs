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

    public Task<Terrain> CalculateMc2Terrain(TerrainGenerationParameters genParams, byte stage = 18);
    public Task<WriteableBitmap> GenerateBitmapAsync(Terrain terrain, Layer layer);
    public Task<WriteableBitmap> DrawBitmapAsync(Terrain terrain, Layer layer, WriteableBitmap bitmap);
}