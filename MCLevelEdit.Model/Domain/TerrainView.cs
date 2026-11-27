namespace MCLevelEdit.Model.Domain;

/// <summary>
/// How a terrain preview was drawn: an orthographic view turned about the vertical axis (yaw) and looking down
/// (pitch), fitted into the output image. Projects map points into the image, so what is drawn over the preview
/// lines up with it, and says which map tile is visible at each pixel of the image, for picking with the mouse.
/// </summary>
public class TerrainView
{
    private readonly byte[] _heightMap;
    private readonly int[] _tileAt;
    private readonly float _sinYaw, _cosYaw, _sinPitch, _cosPitch;

    public TerrainView(byte[] heightMap, int mapWidth, int mapHeight, int outputWidth, int outputHeight,
        float heightScale, float yawDegrees, float pitchDegrees)
    {
        _heightMap = heightMap;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        OutputWidth = outputWidth;
        OutputHeight = outputHeight;
        HeightScale = heightScale;
        YawDegrees = yawDegrees;
        PitchDegrees = Math.Clamp(pitchDegrees, 1.0f, 90.0f);

        float yaw = YawDegrees * MathF.PI / 180.0f;
        float pitch = PitchDegrees * MathF.PI / 180.0f;
        _sinYaw = MathF.Sin(yaw);
        _cosYaw = MathF.Cos(yaw);
        _sinPitch = MathF.Sin(pitch);
        _cosPitch = MathF.Cos(pitch);

        _tileAt = new int[outputWidth * outputHeight];
        Array.Fill(_tileAt, -1);
    }

    public int MapWidth { get; }
    public int MapHeight { get; }
    public int OutputWidth { get; }
    public int OutputHeight { get; }
    public float HeightScale { get; }
    public float YawDegrees { get; }
    public float PitchDegrees { get; }

    /// <summary>The fit of the view into the image: image = view * Scale + Offset.</summary>
    public float Scale { get; set; } = 1.0f;
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }

    /// <summary>The map tile drawn at each pixel of the image (y * OutputWidth + x), -1 where none is.</summary>
    public int[] TileAt => _tileAt;

    /// <summary>The height of the terrain at a map point (wrapped round the map, as the game's map wraps).</summary>
    public float HeightAt(int x, int y)
    {
        x = ((x % MapWidth) + MapWidth) % MapWidth;
        y = ((y % MapHeight) + MapHeight) % MapHeight;
        return _heightMap[(y * MapWidth) + x] * HeightScale;
    }

    /// <summary>
    /// A map point at height z in the view before the fit: x along the screen, y down the screen, and the depth
    /// (larger is nearer the camera). The view turns about the middle of the map.
    /// </summary>
    public (float X, float Y, float Depth) ProjectRaw(float x, float y, float z)
    {
        float cx = x - (MapWidth - 1) * 0.5f;
        float cy = y - (MapHeight - 1) * 0.5f;
        float ground = cx * _sinYaw + cy * _cosYaw;
        return (cx * _cosYaw - cy * _sinYaw,
                ground * _sinPitch - z * _cosPitch,
                ground * _cosPitch + z * _sinPitch);
    }

    /// <summary>A map point at height z in the image.</summary>
    public (float X, float Y) Project(float x, float y, float z)
    {
        var (px, py, _) = ProjectRaw(x, y, z);
        return (px * Scale + OffsetX, py * Scale + OffsetY);
    }

    /// <summary>A map point on the terrain in the image (the height of the tile it is on).</summary>
    public (float X, float Y) Project(float x, float y)
    {
        return Project(x, y, HeightAt((int)MathF.Floor(x + 0.5f), (int)MathF.Floor(y + 0.5f)));
    }

    /// <summary>The map tile visible at a pixel of the image, or null where the image shows no terrain.</summary>
    public (int X, int Y)? TileAtPixel(double pixelX, double pixelY)
    {
        int px = (int)Math.Floor(pixelX);
        int py = (int)Math.Floor(pixelY);
        if (px < 0 || py < 0 || px >= OutputWidth || py >= OutputHeight)
            return null;
        int tile = _tileAt[(py * OutputWidth) + px];
        if (tile < 0)
            return null;
        return (tile % MapWidth, tile / MapWidth);
    }
}
