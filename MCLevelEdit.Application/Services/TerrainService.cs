using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Abstractions;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Application.Utils;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;

using Splat;
using System.Runtime.InteropServices;

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

    // What builds the levels' walls, paths and buildings into the terrain, with the building shapes:
    // DATA/BUILD0-0.TAB|DAT of Magic Carpet 2 (with its DATA/BLDGPRM.DAT) and of Magic Carpet (unpacked), and
    // Hidden Worlds' DATA/BUILD1-0.TAB|DAT.
    private readonly Mc2TerrainFeatureBuilder _mc2Features;
    private readonly Mc1TerrainFeatureBuilder _mc1Features;
    private readonly Mc1TerrainFeatureBuilder _hiddenWorldsFeatures;

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

        var search = SearchRings.Load(LoadAsset("search.dat"));
        _mc2Features = new Mc2TerrainFeatureBuilder(LoadBuildingShapes("mc2"), BuildingParams.Load(LoadAsset("building-params-mc2.dat")), search);
        _mc1Features = new Mc1TerrainFeatureBuilder(LoadBuildingShapes("mc1"), search);
        _hiddenWorldsFeatures = new Mc1TerrainFeatureBuilder(LoadBuildingShapes("hidden-worlds"), search);
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
    /// The level's Effects in its order, which the terrain features are among. The builder picks those that shape
    /// the terrain (walls, paths, buildings with no switch) and follows the links of the wall and path nodes.
    /// </summary>
    public static List<LevelFeature> GetLevelFeatures(Map map) => GetEntityFeatures(map).Concat(GetCastles(map)).ToList();

    private static IEnumerable<LevelFeature> GetEntityFeatures(Map map) => map.Entities
        .Where(e => e.EntityType?.Model is not null && (e.EntityType.TypeId == TypeId.Effect
            || e.EntityType.TypeId == TypeId.Special && (e.EntityType.Model.Id == TerrainFeatureBuilder.SPECIAL_CAVE_DOOR || e.EntityType.Model.Id == TerrainFeatureBuilder.SPECIAL_GATE)))
        .OrderBy(e => e.Id)
        .Select(e => new LevelFeature
        {
            Id = e.Id,
            Model = e.EntityType.TypeId == TypeId.Special ? 0x100 + e.EntityType.Model.Id : e.EntityType.Model.Id,
            X = e.Position.X,
            Y = e.Position.Y,
            DisId = e.DisId,
            SwitchSize = e.SwitchSize,
            SwitchId = e.SwitchId,
            Parent = e.Parent,
            Child = e.Child,
            Par3 = (e as MCLevelEdit.Model.Domain.MC2.Mc2Entity)?.Par3 ?? 0
        })
        .ToList();

    /// <summary>
    /// The wizards' castles at their start level (Magic Carpet 2): an active wizard with a castle level and the castle
    /// spell has one on its spawn point (Flyer 1 to 8 for the wizards in order), or the map's corner when there is none.
    /// </summary>
    private static IEnumerable<LevelFeature> GetCastles(Map map)
    {
        if (map.GameVersion != GameVersion.MagicCarpet2)
            yield break;
        for (int i = 0; i < map.Wizards.Length; i++)
        {
            var wizard = map.Wizards[i] as MCLevelEdit.Model.Domain.MC2.Mc2Wizard;
            if (wizard is null || !wizard.IsActive || wizard.CastleLevel == 0)
                continue;
            if (wizard.StartingSpells is { Length: > 2 } spells && spells[2] == 0)
                continue;
            var spawn = map.Entities.Where(e => e.EntityType?.TypeId == TypeId.Spawn && e.EntityType.Model?.Id == (int)Spawn.Flyer1 + i)
                .OrderBy(e => e.Id).FirstOrDefault();
            yield return new LevelFeature
            {
                Id = 0x10000 + i,
                Model = TerrainFeatureBuilder.CASTLE,
                X = spawn?.Position.X ?? 0,
                Y = spawn?.Position.Y ?? 0,
                Parent = wizard.CastleLevel
            };
        }
    }

    /// <summary>Whether the entity is one that can change the terrain when the level loads (or links to one).</summary>
    public static bool ShapesTerrain(Entity? entity) =>
        entity?.EntityType?.Model is not null && (entity.EntityType.TypeId == TypeId.Special && (entity.EntityType.Model.Id == TerrainFeatureBuilder.SPECIAL_CAVE_DOOR || entity.EntityType.Model.Id == TerrainFeatureBuilder.SPECIAL_GATE)
        || entity.EntityType.TypeId == TypeId.Effect
        && TerrainFeatureBuilder.ShapesTerrain(entity.EntityType.Model.Id));

    public Terrain BuildFeatures(Terrain generated, Map map)
    {
        TerrainFeatureBuilder builder = map.TerrainGame switch
        {
            TerrainGame.MagicCarpet1 => _mc1Features,
            TerrainGame.HiddenWorlds => _hiddenWorldsFeatures,
            _ => _mc2Features
        };
        return builder.Build(generated, GetLevelFeatures(map));
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

    public async Task<WriteableBitmap> DrawIsometricAsync(Map map, Layer layer, int outputWidth, int outputHeight,
        float yawDegrees = IsometricTerrainRender.DefaultYawDegrees)
    {
        return (await DrawIsometricViewAsync(map, layer, outputWidth, outputHeight, yawDegrees)).Bitmap;
    }

    public Task<(WriteableBitmap Bitmap, TerrainView View)> DrawIsometricViewAsync(Map map, Layer layer, int outputWidth, int outputHeight,
        float yawDegrees = IsometricTerrainRender.DefaultYawDegrees)
    {
        return Task.Run(() =>
        {
            var texture = new WriteableBitmap(
                new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                new Vector(96, 96),
                PixelFormat.Rgba8888,
                AlphaFormat.Unpremul);

            // the heights of the terrain as built (walls, paths, castles and buildings raised into it, as the colours
            // DrawBitmap draws), not of the terrain as generated
            DrawBitmap(texture, map, layer);
            return DrawIsometricView(map.Terrain.MapHeightmap_11B4E0, texture, outputWidth, outputHeight, yawDegrees,
                IsometricTerrainRender.DefaultPitchDegrees);
        });
    }

    public WriteableBitmap DrawIsometric(byte[] heightMap, WriteableBitmap texture, int outputWidth, int outputHeight,
        float yawDegrees = IsometricTerrainRender.DefaultYawDegrees, float pitchDegrees = IsometricTerrainRender.DefaultPitchDegrees)
    {
        return DrawIsometricView(heightMap, texture, outputWidth, outputHeight, yawDegrees, pitchDegrees).Bitmap;
    }

    private (WriteableBitmap Bitmap, TerrainView View) DrawIsometricView(byte[] heightMap, WriteableBitmap texture, int outputWidth,
        int outputHeight, float yawDegrees, float pitchDegrees)
    {
        byte[] rgba = IsometricTerrainRender.Render(
            heightMap,
            GetRgbaBytes(texture),
            256,
            256,
            out TerrainView view,
            outputWidth,
            outputHeight,
            heightScale: 0.10f,
            yawDegrees: yawDegrees,
            pitchDegrees: pitchDegrees
        );

        var bitmap = new WriteableBitmap(
            new PixelSize(outputWidth, outputHeight),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var fb = bitmap.Lock())
        {
            for (int y = 0; y < outputHeight; y++)
            {
                Marshal.Copy(
                    rgba,
                    y * outputWidth * 4,
                    IntPtr.Add(fb.Address, y * fb.RowBytes),
                    outputWidth * 4);
            }
        }

        return (bitmap, view);
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

    protected static byte[] GetRgbaBytes(WriteableBitmap bitmap)
    {
        int width = bitmap.PixelSize.Width;
        int height = bitmap.PixelSize.Height;

        byte[] rgba = new byte[width * height * 4];

        using (var fb = bitmap.Lock())
        {
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(
                    IntPtr.Add(fb.Address, y * fb.RowBytes),
                    rgba,
                    y * width * 4,
                    width * 4);
            }
        }

        return rgba;
    }
}
