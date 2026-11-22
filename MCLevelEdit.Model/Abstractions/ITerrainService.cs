using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    /// <param name="fog">Use the fog palette for Night maps (MC2 levels with level type bit 2 set).</param>
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer, bool fog = false);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Terrain terrain, Layer layer, bool fog = false);
    GenerationParameters GetRandomGeneratorParamters();
}