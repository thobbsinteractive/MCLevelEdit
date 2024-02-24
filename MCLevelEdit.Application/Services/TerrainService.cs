using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Abstractions;
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
    public Color[] Pallet;

    public Dictionary<int, int> terrainTypeToPalletMapping = new Dictionary<int, int>()
    {
        {0  , 3},
        {1  , 108},
        {2  , 168},
        {3  , 74},
        {4  , 139},
        {5  , 168},
        {6  , 46},
        {8  , 78},
        {9  , 82},
        {10 , 171},
        {11 , 170},
        {12 , 170},
        {18 , 149},
        {19 , 149},
        {20 , 149},
        {21 , 118},
        {22 , 118},
        {23 , 171},
        {24 , 171},
        {25 , 171},
        {26 , 172},
        {27 , 171},
        {32 , 108},
        {33 , 170},
        {34 , 108},
        {35 , 79},
        {36 , 108},
        {37 , 108},
        {38 , 22},
        {39 , 108},
        {40 , 181},
        {42 , 16},
        {43 , 143},
        {44 , 54},
        {45 , 170},
        {46 , 32},
        {47 , 170},
        {48 , 74},
        {49 , 86},
        {50 , 86},
        {51 , 86},
        {52 , 170},
        {53 , 78},
        {54 , 33},
        {55 , 36},
        {56 , 168},
        {57 , 168},
        {58 , 168},
        {59 , 168},
        {60 , 76},
        {61 , 76},
        {62 , 76},
        {63 , 32},
        {64 , 36},
        {65 , 140},
        {66 , 141},
        {67 , 141},
        {68 , 54},
        {69 , 170},
        {70 , 35},
        {71 , 78},
        {72 , 54},
        {73 , 54},
        {74 , 33},
        {75 , 54},
        {76 , 170},
        {77 , 98},
        {79 , 82},
        {80 , 16},
        {81 , 82},
        {82 , 41},
        {83 , 99},
        {84 , 33},
        {85 , 78},
        {86 , 35},
        {87 , 35},
        {88 , 142},
        {89 , 36},
        {90 , 86},
        {91 , 32},
        {92 , 54},
        {93 , 54},
        {94 , 86},
        {95 , 54},
        {96 , 144},
        {97 , 170},
        {98 , 81},
        {99 , 142},
        {100, 170},
        {101, 79},
        {102, 34},
        {103, 181},
        {104, 172},
        {105, 82},
        {106, 18},
        {107, 16},
        {109, 171},
        {110, 97},
        {111, 97},
        {113, 97},
        {114, 108},
        {115, 54},
        {116, 170},
        {117, 54},
        {118, 54},
        {119, 170},
        {120, 34},
        {121, 33},
        {122, 33},
        {123, 35},
        {124, 54},
        {125, 33},
        {128, 181},
        {130, 41},
        {131, 38},
        {132, 144},
        {133, 82},
        {134, 16},
        {135, 76},
        {136, 143},
        {138, 79},
        {140, 170},
        {141, 82},
        {142, 78},
        {143, 80},
        {144, 79},
        {145, 54},
        {146, 170},
        {147, 81}
    };

    public TerrainService()
    {
        Pallet = new Color[256];

        var writeableBitmap = WriteableBitmap.Decode(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/day-palette.bmp")));

        using (var fb = writeableBitmap.Lock())
        {
            for (int x = 0; x < 256; x++)
            {
                var bytes = fb.GetPixel(x, 0);

                Pallet[x] = new Color(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
        }
    }

    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Terrain terrain, Layer layer)
    {
        return Task.Run(() =>
        {
            return DrawBitmap(bitmap, terrain, layer);
        });
    }

    public WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Terrain terrain, Layer layer)
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
                            if (terrainTypeToPalletMapping.ContainsKey((int)terrain.MapTerrainType_10B4E0[index]))
                            {
                                baseColour = Pallet[terrainTypeToPalletMapping[(int)terrain.MapTerrainType_10B4E0[index]]];
                            }

                            fb.SetPixel(x, y, new Color(255, (byte)Math.Max(baseColour.R - terrain.MapShading_12B4E0[index], byte.MinValue),
                                        (byte)Math.Max(baseColour.G - terrain.MapShading_12B4E0[index], byte.MinValue),
                                        (byte)Math.Max(baseColour.B - terrain.MapShading_12B4E0[index], byte.MinValue)));
                        }
                    }
                    if (layer == Layer.Height)
                    {
                        fb.SetPixel(x, y, new Color(255, terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index]));
                    }
                }
            }
        }
            //BitmapUtils.SaveBitmap(bitmap);
        return bitmap;
    }

    public async Task<Terrain> CalculateMc2Terrain(GenerationParameters genParams, byte stage = 18)
    {
        ITerrainGenerator terrainGenerator = new TerrainGenerator();
        return await terrainGenerator.CalculateTerrainAsync(genParams, stage);
    }

    public GenerationParameters GetRandomGeneratorParamters()
    {
        Random rnd = new Random();
        return new GenerationParameters()
        {
            Seed = (ushort)rnd.Next(ushort.MaxValue),
            Offset = 0,
            Raise = (ushort)rnd.Next(ushort.MaxValue),
            Gnarl = (ushort)rnd.Next(50),
            River = (byte)rnd.Next(byte.MaxValue),
            Source = (byte)rnd.Next(byte.MaxValue),
            SnLin = 200,
            SnFlt = (byte)rnd.Next(50),
            BhLin = (byte)rnd.Next(byte.MaxValue),
            BhFlt = (byte)rnd.Next(20),
            RkSte = (byte)rnd.Next(40)
        };
    }
}
