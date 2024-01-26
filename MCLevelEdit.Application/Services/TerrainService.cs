using Avalonia.Media;
using Avalonia.Media.Imaging;
using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;
using Splat;

namespace MCLevelEdit.Application.Services;

public class TerrainService : ITerrainService, IEnableLogger
{
    public Color WATER_COLOUR = new Color(255, 46, 78, 166);
    public Color COAST_COLOUR = new Color(255, 136, 99, 66);
    public Color SAND_COLOUR = new Color(255, 186, 150, 101);
    public Color STONE_COLOUR = new Color(255, 186, 150, 101);
    public Color GRASS_COLOUR = new Color(255, 117, 105, 40);

    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer)
    {
        return Task.Run(() =>
        {
            using (var fb = bitmap.Lock())
            {
                for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
                {
                    for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
                    {
                        int index = (y * Globals.MAX_MAP_SIZE) + x;

                        if (layer == Layer.Game)
                        {
                            Color baseColour = WATER_COLOUR;
                            if (terrain.MapTerrainType_10B4E0 != null)
                            {
                                switch (terrain.MapTerrainType_10B4E0[index])
                                {
                                    case 82:
                                        baseColour = new Color(255, 168, 152, 148);
                                        break;
                                    case 83:
                                        baseColour = new Color(255, 188, 160, 156);
                                        break;
                                    case 6:
                                        baseColour = new Color(255, 164, 116, 92);
                                        break;
                                    case 80:
                                        baseColour = new Color(255, 128, 136, 140);
                                        break;
                                    case 79:
                                        baseColour = new Color(255, 104, 128, 132);
                                        break;
                                    case 76:
                                        baseColour = new Color(255, 40, 104, 116);
                                        break;
                                    case 78:
                                        baseColour = new Color(255, 84, 120, 128);
                                        break;
                                    case 81:
                                        baseColour = new Color(255, 148, 144, 144);
                                        break;
                                    case 77:
                                        baseColour = new Color(255, 64, 112, 124);
                                        break;
                                    case 108:
                                        baseColour = new Color(255, 88, 84, 92);
                                        break;
                                    case 111:
                                        baseColour = new Color(255, 16, 76, 88);
                                        break;
                                    case 110:
                                        baseColour = new Color(255, 48, 52, 60);
                                        break;
                                    case 1:
                                        baseColour = new Color(255, 140, 96, 68);
                                        break;
                                    case 113:
                                        baseColour = new Color(255, 196, 172, 168);
                                        break;

                                        //case 0:
                                        //    baseColour = new Color();
                                        //    break;
                                        //case 2:
                                        //    baseColour = COAST_COLOUR;
                                        //    break;
                                        //case 3:
                                        //    baseColour = SAND_COLOUR;
                                        //    break;
                                        //case 4:
                                        //    baseColour = STONE_COLOUR;
                                        //    break;
                                        //case 5:
                                        //    baseColour = GRASS_COLOUR;
                                        //    break;
                                        //case 6:
                                        //    baseColour = SAND_COLOUR;
                                        //    break;
                                }

                                fb.SetPixel(x , y, new Color(255, (byte)Math.Max(baseColour.R - terrain.MapShading_12B4E0[index], byte.MinValue),
                                            (byte)Math.Max(baseColour.G - terrain.MapShading_12B4E0[index], byte.MinValue),
                                            (byte)Math.Max(baseColour.B - terrain.MapShading_12B4E0[index], byte.MinValue)));
                            }
                        }
                        if (layer == Layer.Height)
                        {
                            fb.SetPixel(x , y, new Color(255, terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index]));
                        }
                    }
                }
            }
            //BitmapUtils.SaveBitmap(bitmap);

            return bitmap;
        });
    }

    public async Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18)
    {
        TerrainGenerator terrainGenerator = new TerrainGenerator();
        return await terrainGenerator.CalculateTerrainAsync(genParams, stage);
    }
}
