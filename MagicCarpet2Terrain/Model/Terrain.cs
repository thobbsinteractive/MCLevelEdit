namespace MagicCarpet2Terrain.Model
{
    public class Terrain
    {
        public GenerationParameters GenerationParameters { get; set; } = new GenerationParameters();
        public short[] MapEntityIndex_15B4E0 { get; set; }
        public byte[] MapHeightmap_11B4E0 { get; set; }
        public byte[] MapAngle_13B4E0 { get; set; }
        public byte[] MapTerrainType_10B4E0 { get; set; }
        public byte[] MapShading_12B4E0 { get; set; }
    }
}