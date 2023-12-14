using Avalonia.Media.Imaging;
using MCLevelEdit.Application.Enums;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Application.Interfaces;

public interface ITerrainService
{
    public Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18);
    public Task<WriteableBitmap> GenerateBitmapAsync(Terrain terrain, Layer layer);
    public Task<WriteableBitmap> DrawBitmapAsync(Terrain terrain, Layer layer, WriteableBitmap bitmap);
}