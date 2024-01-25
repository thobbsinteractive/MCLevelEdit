namespace MagicCarpet2Terrain.Model
{
    public class GenerationParameters
    {
        public int MapMapSize { get; set; } = 256;
        public MapType MapType { get; set; } = MapType.Day;
        public ushort Seed { get; set; }
        public ushort Offset { get; set; }
        public ushort Raise { get; set; }
        public ushort Gnarl { get; set; }
        public ushort River { get; set; }
        public ushort LRiver { get; set; }
        public byte Source { get; set; }
        public ushort SnLin { get; set; }
        public byte SnFlt { get; set; }
        public byte BhLin { get; set; }
        public byte BhFlt { get; set; }
        public byte RkSte { get; set; }
    }
}


