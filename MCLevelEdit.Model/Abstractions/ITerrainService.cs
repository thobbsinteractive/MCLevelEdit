using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Terrain terrain, Layer layer);
    GenerationParameters GetRandomGeneratorParamters();
}