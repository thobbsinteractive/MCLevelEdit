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
    // Palettes extracted from the Magic Carpet 2 data files (DATA/PALD|PALN|PALF|PALC-0.DAT, colours x4)
    public Color[] Pallet;
    public Color[] NightPallet;
    public Color[] FogPallet;
    public Color[] CavePallet;

    // Terrain type and shading lookups extracted from DATA/TABLESD|N|C.DAT
    public TerrainColourTable DayColours;
    public TerrainColourTable NightColours;
    public TerrainColourTable CaveColours;

    // Magic Carpet 1 (CD_Files/DATA), a palette and colour table each:
    // Magic Carpet PAL0-0.DAT and TABLES.DAT; Hidden Worlds (the data disk) PAL1-0.DAT and DTABLES.DAT.
    // The palettes go with the terrain textures TMAPS0 and TMAPS1.
    public Color[] Mc1Pallet;
    public Color[] HiddenWorldsPallet;
    public TerrainColourTable Mc1Colours;
    public TerrainColourTable HiddenWorldsColours;

    // The building shapes the levels build into the terrain: DATA/BUILD0-0.TAB|DAT of Magic Carpet 2 (with its
    // DATA/BLDGPRM.DAT) and of Magic Carpet (unpacked), and Hidden Worlds' DATA/BUILD1-0.TAB|DAT.
    private readonly Mc2BuildingTerrainBuilder _mc2Buildings;
    private readonly Mc1BuildingTerrainBuilder _mc1Buildings;
    private readonly Mc1BuildingTerrainBuilder _hiddenWorldsBuildings;

    public TerrainService()
    {
        Pallet = LoadPallet("day-palette.bmp");
        NightPallet = LoadPallet("night-palette.bmp");
        FogPallet = LoadPallet("fog-palette.bmp");
        CavePallet = LoadPallet("cave-palette.bmp");

        DayColours = LoadColourTable("terrain-colours-day.dat");
        NightColours = LoadColourTable("terrain-colours-night.dat");
        CaveColours = LoadColourTable("terrain-colours-cave.dat");

        Mc1Pallet = LoadPallet("mc1-palette.bmp");
        HiddenWorldsPallet = LoadPallet("hidden-worlds-palette.bmp");
        Mc1Colours = LoadColourTable("terrain-colours-mc1.dat");
        HiddenWorldsColours = LoadColourTable("terrain-colours-hidden-worlds.dat");

        _mc2Buildings = new Mc2BuildingTerrainBuilder(LoadBuildingShapes("mc2"), BuildingParams.Load(LoadAsset("building-params-mc2.dat")));
        _mc1Buildings = new Mc1BuildingTerrainBuilder(LoadBuildingShapes("mc1"));
        _hiddenWorldsBuildings = new Mc1BuildingTerrainBuilder(LoadBuildingShapes("hidden-worlds"));
    }

    private static byte[] LoadAsset(string fileName)
    {
        using var stream = AssetLoader.Open(new Uri($"avares://MCLevelEdit/Assets/{fileName}"));
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static BuildingShapes LoadBuildingShapes(string game) =>
        BuildingShapes.Load(LoadAsset($"building-shapes-{game}.tab"), LoadAsset($"building-shapes-{game}.dat"));

    /// <summary>
    /// The buildings the level builds before it starts: Effect 2Dh (VillagerBuilding) with no switch (DisId FFFFh),
    /// in the order of the level, on their square, with the shape in Parent.
    /// </summary>
    public static List<BuildingPlacement> GetBuildingPlacements(Map map) => map.Entities
        .Where(e => e.IsBuilding() && e.DisId == ushort.MaxValue)
        .OrderBy(e => e.Id)
        .Select(e => new BuildingPlacement { X = e.Position.X, Y = e.Position.Y, Shape = e.Parent })
        .ToList();

    public Terrain BuildBuildings(Terrain generated, Map map)
    {
        BuildingTerrainBuilder builder = map.TerrainGame switch
        {
            TerrainGame.MagicCarpet1 => _mc1Buildings,
            TerrainGame.HiddenWorlds => _hiddenWorldsBuildings,
            _ => _mc2Buildings
        };
        return builder.Build(generated, GetBuildingPlacements(map));
    }

    /// <summary>
    /// The palette and colour table the map's game draws its minimap with: Magic Carpet or Hidden Worlds for
    /// Magic Carpet format levels, otherwise Magic Carpet 2's for the map type (Night levels may use fog).
    /// </summary>
    public (Color[] Pallet, TerrainColourTable Colours) GetColours(Map map)
    {
        if (map.GameVersion == GameVersion.MagicCarpet1)
            return map.IsHiddenWorlds ? (HiddenWorldsPallet, HiddenWorldsColours) : (Mc1Pallet, Mc1Colours);

        var mapType = map.Terrain.GenerationParameters?.MapType ?? MapType.Day;
        bool fog = (map as MCLevelEdit.Model.Domain.MC2.Mc2Map)?.UsesFogPalette ?? false;
        return (GetPallet(mapType, fog), GetColourTable(mapType));
    }

    private static TerrainColourTable LoadColourTable(string fileName)
    {
        using var stream = AssetLoader.Open(new Uri($"avares://MCLevelEdit/Assets/{fileName}"));
        return TerrainColourTable.FromStream(stream);
    }

    /// <summary>The colour lookup the game uses for this map type (see remc2 ReadAndDecompressTables_54800). Fog levels use the night table.</summary>
    public TerrainColourTable GetColourTable(MapType mapType) => mapType switch
    {
        MapType.Night => NightColours,
        MapType.Cave => CaveColours,
        _ => DayColours
    };

    private static Color[] LoadPallet(string fileName)
    {
        var pallet = new Color[256];

        var writeableBitmap = WriteableBitmap.Decode(AssetLoader.Open(new Uri($"avares://MCLevelEdit/Assets/{fileName}")));

        using (var fb = writeableBitmap.Lock())
        {
            for (int x = 0; x < 256; x++)
            {
                var bytes = fb.GetPixel(x, 0);

                pallet[x] = new Color(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
        }
        return pallet;
    }

    /// <summary>The palette the game uses for this map type (see remc2 Level.cpp).</summary>
    public Color[] GetPallet(MapType mapType, bool fog = false) => mapType switch
    {
        MapType.Night => fog ? FogPallet : NightPallet,
        MapType.Cave => CavePallet,
        _ => Pallet
    };

    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, Map map, Layer layer)
    {
        return Task.Run(() =>
        {
            return DrawBitmap(bitmap, map, layer);
        });
    }

    public WriteableBitmap DrawBitmap(WriteableBitmap bitmap, Map map, Layer layer)
    {
        var terrain = map.Terrain;
        var mapType = terrain.GenerationParameters?.MapType ?? MapType.Day;
        var (pallet, colours) = GetColours(map);
        // Only terrain generated as a cave has a ceiling, and with it the rock mask (angle bit 8).
        // Day/Night terrain uses bit 8 for water edges, so switching the type without regenerating must not mask.
        var isCave = mapType == MapType.Cave && terrain.MapCeilingHeightmap_14B4E0 is not null && terrain.MapAngle_13B4E0 is not null;

        using (var fb = bitmap.Lock())
        {
            for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
            {
                for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
                {
                    int index = (y * Globals.MAX_MAP_SIZE) + x;

                    if (layer == Layer.Game)
                    {
                        if (terrain.MapTerrainType_10B4E0 != null)
                        {
                            // As the game's minimap: terrain type -> base colour, then shaded through the lookup table.
                            // In caves, squares where the ceiling meets the floor (solid rock) are drawn as colour 0.
                            var palletIndex = isCave && (terrain.MapAngle_13B4E0[index] & 8) != 0
                                ? (byte)0
                                : colours.GetPalletIndex(terrain.MapTerrainType_10B4E0[index], terrain.MapShading_12B4E0[index]);
                            var colour = pallet[palletIndex];
                            fb.SetPixel(x, y, new Color(255, colour.R, colour.G, colour.B));
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

    public async Task<Terrain> CalculateTerrain(GenerationParameters genParams, TerrainGame game = TerrainGame.MagicCarpet2, byte stage = 18)
    {
        ITerrainGenerator terrainGenerator = TerrainGenerator.Create(genParams.MapType, game);
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
