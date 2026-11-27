namespace MCLevelEdit.Application.Services;

/// <summary>
/// The colour lookup the game uses to draw a terrain square on the minimap
/// (remc2 DrawMapRowsLowRes: ColourLookupTable_F6EE0 loaded from DATA/TABLESD|N|C.DAT).
/// The editor's asset holds the two parts of that table it needs:
/// <list type="bullet">
/// <item>0x00000-0x0FFFF: shading table, <c>[(shading &lt;&lt; 8) | colour]</c> gives the shaded palette index</item>
/// <item>0x10000-0x100FF: terrain type to base palette index (offset 0x14000 in the game's table)</item>
/// </list>
/// </summary>
public class TerrainColourTable
{
    public const int SHADING_TABLE_SIZE = 0x10000;
    public const int TERRAIN_TYPE_TABLE_OFFSET = 0x10000;
    public const int SIZE = TERRAIN_TYPE_TABLE_OFFSET + 0x100;

    private readonly byte[] _data;

    public TerrainColourTable(byte[] data)
    {
        if (data.Length != SIZE)
            throw new ArgumentException($"A terrain colour table must be {SIZE} bytes, but this one is {data.Length}.");
        _data = data;
    }

    public static TerrainColourTable FromStream(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return new TerrainColourTable(memory.ToArray());
    }

    /// <summary>The palette index of a terrain type before shading.</summary>
    public byte GetBaseIndex(byte terrainType) => _data[TERRAIN_TYPE_TABLE_OFFSET + terrainType];

    /// <summary>The palette index the game draws for a terrain square with this type and shading.</summary>
    public byte GetPalletIndex(byte terrainType, byte shading) => _data[(shading << 8) | GetBaseIndex(terrainType)];
}
