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
    /// <summary>
    /// Draws the map's terrain in the colours of its game (and, for MC2, its map type and fog), viewed from
    /// <paramref name="yawDegrees"/> about the vertical and <paramref name="pitchDegrees"/> down from the horizon (1-90).
    /// The defaults are the isometric view (IsometricTerrainRender.DefaultYawDegrees / DefaultPitchDegrees).
    /// </summary>
    Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Domain.Map map, Layer layer,
        float yawDegrees = 45.0f, float pitchDegrees = 35.264389f);
    /// <summary>
    /// As <see cref="DrawBitmapAsync"/>, with the <see cref="Domain.TerrainView"/> of the image: how map points project
    /// into it and which tile is at each pixel.
    /// </summary>
    Task<(WriteableBitmap Bitmap, Domain.TerrainView View)> DrawTerrainViewAsync(WriteableBitmap bitmap, Domain.Map map, Layer layer,
        float yawDegrees = 45.0f, float pitchDegrees = 35.264389f);
    WriteableBitmap DrawIsometric(byte[] heightMap, WriteableBitmap texture, int outputWidth, int outputHeight,
        float yawDegrees = 45.0f, float pitchDegrees = 35.264389f);
    WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Domain.Map map, Layer layer);
    GenerationParameters GetRandomGeneratorParamters();
}