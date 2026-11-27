using System;
using System.Collections.Generic;

namespace MagicCarpet2Terrain.Model
{
    /// <summary>
    /// One building shape of the game's DATA/BUILD?-0 files: the footprint a building stamps into the terrain.
    /// Magic Carpet 2 stores 2 bytes a square (terrain type, height above the building's base; FFh: unchanged);
    /// Magic Carpet stores rows of run-length coded square codes (see <see cref="Mc1TerrainFeatureBuilder"/>).
    /// </summary>
    public class BuildingShape
    {
        public byte Width { get; set; }
        public byte Height { get; set; }
        /// <summary>The whole .DAT file the shape is in.</summary>
        public byte[] Data { get; set; }
        /// <summary>Where the shape starts in <see cref="Data"/>.</summary>
        public int Offset { get; set; }

        public bool IsEmpty => Width == 0 || Height == 0;
    }

    /// <summary>The building shapes of a game (bitmap_pos_struct_t index of BUILD?-0.TAB into BUILD?-0.DAT).</summary>
    public class BuildingShapes
    {
        // A .TAB entry: the offset into the .DAT (4 bytes), then the width and the height
        private const int TAB_ENTRY_SIZE = 6;

        private readonly List<BuildingShape> _shapes = new List<BuildingShape>();

        public int Count => _shapes.Count;

        /// <summary>The shape, or null if the index has none.</summary>
        public BuildingShape this[int index] => index >= 0 && index < _shapes.Count ? _shapes[index] : null;

        /// <summary>Reads the uncompressed .TAB and .DAT files.</summary>
        public static BuildingShapes Load(byte[] tab, byte[] dat)
        {
            var shapes = new BuildingShapes();
            for (int pos = 0; pos + TAB_ENTRY_SIZE <= tab.Length; pos += TAB_ENTRY_SIZE)
            {
                int offset = BitConverter.ToInt32(tab, pos);
                var shape = new BuildingShape { Width = tab[pos + 4], Height = tab[pos + 5] };
                if (offset < 0 || offset > dat.Length)
                {
                    shape.Width = 0;
                    shape.Height = 0;
                    offset = dat.Length;
                }
                shape.Data = dat;
                shape.Offset = offset;
                shapes._shapes.Add(shape);
            }
            return shapes;
        }
    }

    /// <summary>
    /// The Magic Carpet 2 building parameters (DATA/BLDGPRM.DAT, remc2 buildingParams_D93C0): 4 bytes per building shape.
    /// </summary>
    public class BuildingParams
    {
        /// <summary>Flag 4: the building does not raise the cave ceiling over it.</summary>
        public const byte FLAG_KEEPS_CEILING = 4;
        /// <summary>Flag 10h: the building is built before the others when the level loads.</summary>
        public const byte FLAG_BUILT_FIRST = 0x10;

        // The file has 76 entries; the game's index 76 runs on into the bytes after it (remc2 frameColors_D94F0).
        private static readonly byte[] Entry76 = { 0xAA, 0x00, 0x63, 0x0D };

        private readonly byte[] _flags;

        private BuildingParams(byte[] flags)
        {
            _flags = flags;
        }

        public byte GetFlags(int shape) => shape >= 0 && shape < _flags.Length ? _flags[shape] : (byte)0;

        public static BuildingParams Load(byte[] bldgprm)
        {
            int count = bldgprm.Length / 4;
            var flags = new byte[Math.Max(count, 77)];
            for (int i = 0; i < count; i++)
                flags[i] = bldgprm[i * 4 + 2];
            if (count <= 76)
                flags[76] = Entry76[2];
            return new BuildingParams(flags);
        }
    }

    /// <summary>
    /// An Effect (class 10) of the level, as the terrain features read it: the fields at 4-17 of a level entity
    /// (MC1 feature) by their editor names. Walls and paths are lines of nodes linked by Parent and Child; a
    /// building's shape is in Parent.
    /// </summary>
    public class LevelFeature
    {
        /// <summary>DisId of a feature that is built when the level loads (no switch makes it).</summary>
        public const ushort NO_SWITCH = 0xFFFF;

        /// <summary>The feature's index in the level, which Parent and Child refer to.</summary>
        public int Id { get; set; }
        /// <summary>The Effect model (subtype).</summary>
        public int Model { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ushort DisId { get; set; }
        /// <summary>The field at 10 (par0_10): the radius of a Magic Carpet 2 cave crater or cone.</summary>
        public ushort SwitchSize { get; set; }
        /// <summary>The stage tag: a line of nodes is made from the first node with one.</summary>
        public ushort SwitchId { get; set; }
        /// <summary>The node before (walls, paths, canyons, ridges), or the building's shape.</summary>
        public ushort Parent { get; set; }
        /// <summary>The node after (walls, paths, canyons, ridges).</summary>
        public ushort Child { get; set; }
        /// <summary>Magic Carpet 2 only (par3_18): how deep a canyon from this node is dug.</summary>
        public ushort Par3 { get; set; }

        /// <summary>Set once the feature has made its entities (the game sets its type to 0).</summary>
        internal bool Built { get; set; }

        public LevelFeature Copy() => (LevelFeature)MemberwiseClone();
    }
}
