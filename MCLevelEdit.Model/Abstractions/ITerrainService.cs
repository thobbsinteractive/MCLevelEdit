using Avalonia.Media.Imaging;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    public Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer);
}