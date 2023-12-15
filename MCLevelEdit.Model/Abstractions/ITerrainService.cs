using Avalonia.Media.Imaging;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    public Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    public Task<WriteableBitmap> GenerateBitmapAsync(Terrain terrain, Layer layer);
    public Task<WriteableBitmap> DrawBitmapAsync(Terrain terrain, Layer layer, WriteableBitmap bitmap);
}