using System.Runtime.InteropServices;

namespace MagicCarpet2Terrain.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UAxis2D
    {
        [FieldOffset(0)]
        public byte X;
        [FieldOffset(1)]
        public byte Y;
        [FieldOffset(0)]
        public ushort Word;
    }
}

