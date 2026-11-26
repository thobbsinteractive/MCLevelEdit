using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface ITerrainService
{
    /// <summary>Generates terrain the way <paramref name="game"/> does (and, for Magic Carpet 2, the map type).</summary>
    Task<Terrain> CalculateTerrain(GenerationParameters genParams, TerrainGame game = TerrainGame.MagicCarpet2, byte stage = 18);
    /// <summary>
    /// A copy of <paramref name="generated"/> with the map's walls, paths and buildings built into it, as its game does when the level
    /// loads. <paramref name="generated"/> itself is not changed (and is returned if there is nothing to build).
    /// </summary>
    Terrain BuildFeatures(Terrain generated, Domain.Map map);
    /// <summary>Draws the map's terrain from above in the colours of its game (and, for MC2, its map type and fog).</summary>
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    /// <summary>
    /// The same map as <see cref="DrawBitmapAsync"/>, isometric (turned <paramref name="yawDegrees"/> about the vertical, 45
    /// by default), in an image of the size given.
    /// </summary>
    Task<WriteableBitmap> DrawIsometricAsync(Domain.Map map, Layer layer, int outputWidth, int outputHeight, float yawDegrees = 45.0f);
    /// <summary>
    /// As <see cref="DrawIsometricAsync"/>, with the <see cref="Domain.TerrainView"/> of the image: where map points are in
    /// it and which tile is at each pixel (to draw entities over it and pick them).
    /// </summary>
    Task<(WriteableBitmap Bitmap, Domain.TerrainView View)> DrawIsometricViewAsync(Domain.Map map, Layer layer, int outputWidth,
        int outputHeight, float yawDegrees = 45.0f);
    WriteableBitmap DrawIsometric(byte[] heightMap, WriteableBitmap texture, int outputWidth, int outputHeight,
        float yawDegrees = 45.0f, float pitchDegrees = 35.264389f);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    GenerationParameters GetRandomGeneratorParamters();
}