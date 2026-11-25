namespace MCLevelEdit.Application.Utils;

using MCLevelEdit.Model.Domain;
using System;

/// <summary>
/// Orthographic view of the terrain from any direction: the camera turns about the vertical axis (yaw) and looks
/// down at an angle (pitch). The defaults, yaw 45 and pitch 35.26 (atan(1/sqrt 2)), are the classic isometric view.
/// The whole map is fitted into the output image; panning and zooming are left to the canvas. The projection is in
/// <see cref="TerrainView"/>, which the renderer returns, so what is drawn over the image can use it too.
/// </summary>
public static class IsometricTerrainRender
{
    public const float DefaultYawDegrees = 45.0f;
    public const float DefaultPitchDegrees = 35.264389f;

    private struct Vertex
    {
        public float X, Y, Depth;
        public float U, V;

        public Vertex(float x, float y, float depth,
                      float u, float v)
        {
            X = x;
            Y = y;
            Depth = depth;
            U = u;
            V = v;
        }
    }

    public static byte[] Render(
        byte[] heightMap,
        byte[] texture,
        int mapWidth,
        int mapHeight,
        int outputWidth = 256,
        int outputHeight = 256,
        float heightScale = 1.0f,
        float margin = 4.0f,
        float yawDegrees = DefaultYawDegrees,
        float pitchDegrees = DefaultPitchDegrees)
    {
        return Render(heightMap, texture, mapWidth, mapHeight, out _, outputWidth, outputHeight, heightScale, margin,
            yawDegrees, pitchDegrees);
    }

    /// <summary>The image, and in <paramref name="view"/> how it was projected and which tile is at each pixel.</summary>
    public static byte[] Render(
        byte[] heightMap,
        byte[] texture,
        int mapWidth,
        int mapHeight,
        out TerrainView view,
        int outputWidth = 256,
        int outputHeight = 256,
        float heightScale = 1.0f,
        float margin = 4.0f,
        float yawDegrees = DefaultYawDegrees,
        float pitchDegrees = DefaultPitchDegrees)
    {
        if (mapWidth < 2 || mapHeight < 2)
            throw new ArgumentException("Height map must be at least 2x2.");

        if (texture.Length < mapWidth * mapHeight * 4)
            throw new ArgumentException("Invalid RGBA texture.");

        view = new TerrainView(heightMap, mapWidth, mapHeight, outputWidth, outputHeight, heightScale, yawDegrees, pitchDegrees);

        // Project all terrain vertices (TerrainView.ProjectRaw) and find bounds.
        var vertices = new Vertex[mapHeight, mapWidth];

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var (px, py, depth) = view.ProjectRaw(x, y, view.HeightAt(x, y));

                // the texel of the tile at its vertex
                vertices[y, x] = new Vertex(px, py, depth, x, y);

                minX = Math.Min(minX, px);
                minY = Math.Min(minY, py);
                maxX = Math.Max(maxX, px);
                maxY = Math.Max(maxY, py);
            }
        }

        // Fit projection into output image.
        float rangeX = maxX - minX;
        float rangeY = maxY - minY;

        float scale = Math.Min(
            (outputWidth - 2 * margin) / rangeX,
            (outputHeight - 2 * margin) / rangeY);

        view.Scale = scale;
        view.OffsetX = (outputWidth - rangeX * scale) * 0.5f - minX * scale;
        view.OffsetY = (outputHeight - rangeY * scale) * 0.5f - minY * scale;

        // Transform vertices to screen coordinates.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var v = vertices[y, x];

                v.X = v.X * scale + view.OffsetX;
                v.Y = v.Y * scale + view.OffsetY;

                vertices[y, x] = v;
            }
        }

        // Output RGBA buffer and depth buffer.
        byte[] output = new byte[
            outputWidth * outputHeight * 4];

        float[] depthBuffer = new float[
            outputWidth * outputHeight];

        Array.Fill(depthBuffer, float.NegativeInfinity);

        // Rasterize terrain cells.
        for (int y = 0; y < mapHeight - 1; y++)
        {
            for (int x = 0; x < mapWidth - 1; x++)
            {
                Vertex a = vertices[y, x];
                Vertex b = vertices[y, x + 1];
                Vertex c = vertices[y + 1, x];
                Vertex d = vertices[y + 1, x + 1];

                // Two triangles per grid cell.
                DrawTriangle(a, b, c,
                    texture, mapWidth, mapHeight,
                    output, depthBuffer, view.TileAt, outputWidth, outputHeight);

                DrawTriangle(b, d, c,
                    texture, mapWidth, mapHeight,
                    output, depthBuffer, view.TileAt, outputWidth, outputHeight);
            }
        }

        return output;
    }

    private static void DrawTriangle(
        Vertex a, Vertex b, Vertex c,
        byte[] texture,
        int textureWidth,
        int textureHeight,
        byte[] output,
        float[] depthBuffer,
        int[] tileAt,
        int width,
        int height)
    {
        float area =
            (b.X - a.X) * (c.Y - a.Y) -
            (b.Y - a.Y) * (c.X - a.X);

        if (Math.Abs(area) < 0.00001f)
            return;

        int minX = Math.Max(0,
            (int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));

        int maxX = Math.Min(width - 1,
            (int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));

        int minY = Math.Max(0,
            (int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));

        int maxY = Math.Min(height - 1,
            (int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

        float invArea = 1.0f / area;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float wa =
                    ((b.X - px) * (c.Y - py) -
                     (b.Y - py) * (c.X - px)) * invArea;

                float wb =
                    ((c.X - px) * (a.Y - py) -
                     (c.Y - py) * (a.X - px)) * invArea;

                float wc = 1.0f - wa - wb;

                if (wa < -0.0001f ||
                    wb < -0.0001f ||
                    wc < -0.0001f)
                    continue;

                float depth =
                    wa * a.Depth +
                    wb * b.Depth +
                    wc * c.Depth;

                int index = y * width + x;

                // Larger depth is closer to the camera.
                if (depth <= depthBuffer[index])
                    continue;

                depthBuffer[index] = depth;

                // Interpolate texture coordinates.
                float u = wa * a.U + wb * b.U + wc * c.U;
                float v = wa * a.V + wb * b.V + wc * c.V;

                int tx = Math.Clamp(
                    (int)MathF.Round(u), 0, textureWidth - 1);

                int ty = Math.Clamp(
                    (int)MathF.Round(v), 0, textureHeight - 1);

                // the tile shown here, for picking it with the mouse
                tileAt[index] = ty * textureWidth + tx;

                int texIndex = (ty * textureWidth + tx) * 4;
                int outIndex = index * 4;

                output[outIndex + 0] = texture[texIndex + 0];
                output[outIndex + 1] = texture[texIndex + 1];
                output[outIndex + 2] = texture[texIndex + 2];
                output[outIndex + 3] = texture[texIndex + 3];
            }
        }
    }
}
