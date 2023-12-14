namespace MCLevelEdit.Model.Domain;

public class Terrain
{
    public TerrainGenerationParameters GenerationParameters { get; set; }
    public short[] MapEntityIndex_15B4E0 { get; set; }
    public byte[] MapHeightmap_11B4E0 { get; set; }
    public byte[] MapAngle_13B4E0 { get; set; }
    public byte[] MapTerrainType_10B4E0 { get; set; }
    public byte[] MapShading_12B4E0 { get; set; }
}
