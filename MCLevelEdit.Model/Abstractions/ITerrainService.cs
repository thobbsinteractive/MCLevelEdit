using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    /// <summary>Generates terrain the way <paramref name="game"/> does (and, for Magic Carpet 2, the map type).</summary>
    Task<Terrain> CalculateTerrain(GenerationParameters genParams, TerrainGame game = TerrainGame.MagicCarpet2, byte stage = 18);
    /// <summary>
    /// A copy of <paramref name="generated"/> with the map's buildings built into it, as its game does when the level
    /// loads. <paramref name="generated"/> itself is not changed (and is returned if there is nothing to build).
    /// </summary>
    Terrain BuildBuildings(Terrain generated, Domain.Map map);
    /// <summary>Draws the map's terrain in the colours of its game (and, for MC2, its map type and fog).</summary>
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    GenerationParameters GetRandomGeneratorParamters();
}