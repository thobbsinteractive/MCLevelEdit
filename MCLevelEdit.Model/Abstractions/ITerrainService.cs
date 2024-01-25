using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    public Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer);
}