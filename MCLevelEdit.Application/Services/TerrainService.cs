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
        {0  ,   3    },
        {1  ,   40   },
        {2  ,   168  },
        {3  ,   76   },
        {4  ,   139  },
        {5  ,   33   },
        {6  ,   16   },
        {10 ,   172  },
        {11 ,   170  },
        {18 ,   129  },
        {19 ,   149  },
        {20 ,   150  },
        {24 ,   171  },
        {26 ,   170  },
        {27 ,   109  },
        {32 ,   110  },
        {33 ,   171  },
        {34 ,   49   },
        {35 ,   79   },
        {36 ,   108  },
        {37 ,   49   },
        {38 ,   109  },
        {39 ,   40   },
        {40 ,   181  },
        {42 ,   44   },
        {43 ,   78   },
        {44 ,   118  },
        {45 ,   49   },
        {46 ,   137  },
        {47 ,   109  },
        {48 ,   65   },
        {50 ,   130  },
        {51 ,   252  },
        {52 ,   169  },
        {53 ,   150  },
        {55 ,   36   },
        {56 ,   168  },
        {57 ,   168  },
        {58 ,   54   },
        {59 ,   54   },
        {60 ,   76   },
        {61 ,   74   },
        {62 ,   74   },
        {63 ,   135  },
        {64 ,   36   },
        {65 ,   136  },
        {66 ,   54   },
        {67 ,   33   },
        {69 ,   110  },
        {70 ,   141  },
        {71 ,   150  },
        {72 ,   54   },
        {73 ,   252  },
        {74 ,   135  },
        {75 ,   150  },
        {76 ,   170  },
        {77 ,   22   },
        {79 ,   82   },
        {80 ,   82   },
        {81 ,   170  },
        {82 ,   16   },
        {83 ,   170  },
        {85 ,   150  },
        {86 ,   36   },
        {87 ,   150  },
        {88 ,   143  },
        {89 ,   33   },
        {90 ,   54   },
        {91 ,   135  },
        {92 ,   131  },
        {93 ,   32   },
        {94 ,   54   },
        {95 ,   54   },
        {96 ,   144  },
        {97 ,   170  },
        {98 ,   172  },
        {99 ,   142  },
        {100,   109  },
        {101,   22   },
        {102,   54   },
        {103,   169  },
        {104,   49   },
        {105,   82   },
        {106,   18   },
        {107,   97   },
        {109,   109  },
        {110,   172  },
        {111,   97   },
        {114,   22   },
        {115,   149  },
        {116,   110  },
        {117,   131  },
        {118,   54   },
        {119,   22   },
        {120,   32   },
        {121,   132  },
        {122,   33   },
        {125,   33   },
        {132,   144  },
        {133,   82   },
        {134,   16   },
        {135,   77   },
        {142,   54   },
        {143,   81   },
        {144,   169  },
        {145,   32   },
        {146,   170  },
        {147,   22   }
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
