using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    /// <summary>Generates terrain the way <paramref name="game"/> does (and, for Magic Carpet 2, the map type).</summary>
    Task<Terrain> CalculateTerrain(GenerationParameters genParams, TerrainGame game = TerrainGame.MagicCarpet2, byte stage = 18);
    /// <summary>Draws the map's terrain in the colours of its game (and, for MC2, its map type and fog).</summary>
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    GenerationParameters GetRandomGeneratorParamters();
}