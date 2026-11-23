namespace MagicCarpet2Terrain.Model
{
    public class Terrain
    {
        /// <summary>
        /// Size of <see cref="AngleTypes"/>: keys of the 4 corner marks are a * 343 + b * 49 + c * 7 + d. The generator
        /// marks 0-6 (2401 keys) but a mark can be 7, up to key 2800; those keys have no pattern (all 0).
        /// </summary>
        public const int ANGLE_TYPE_KEYS = 2801;

        public GenerationParameters GenerationParameters { get; set; } = new GenerationParameters();
        public short[] MapEntityIndex_15B4E0 { get; set; }
        public byte[] MapHeightmap_11B4E0 { get; set; }
        public byte[] MapAngle_13B4E0 { get; set; }
        public byte[] MapTerrainType_10B4E0 { get; set; }
        public byte[] MapShading_12B4E0 { get; set; }
        /// <summary>Cave levels only: ceiling height of each square (secondHeightmap_14B4E0). Null for other map types.</summary>
        public byte[] MapCeilingHeightmap_14B4E0 { get; set; }

        /// <summary>
        /// The terrain type and angle for each key of 4 corner marks (remc2 building_F2CD0x, remc1 angleTypes_B5D40):
        /// the first pattern that fits, built while the terrain is generated. The game uses it again when buildings
        /// change the terrain. Null if the terrain was not generated that far.
        /// </summary>
        public byte[,] AngleTypes { get; set; }

        /// <summary>The terrain random seed (remc2 rand2_17B4E0, remc1 randSeed_12C1E0) as generation left it.</summary>
        public ushort Seed { get; set; }

        /// <summary>A copy of the terrain with its own maps, to change without touching this one.</summary>
        public Terrain Clone()
        {
            return new Terrain
            {
                GenerationParameters = GenerationParameters,
                MapEntityIndex_15B4E0 = (short[])MapEntityIndex_15B4E0?.Clone(),
                MapHeightmap_11B4E0 = (byte[])MapHeightmap_11B4E0?.Clone(),
                MapAngle_13B4E0 = (byte[])MapAngle_13B4E0?.Clone(),
                MapTerrainType_10B4E0 = (byte[])MapTerrainType_10B4E0?.Clone(),
                MapShading_12B4E0 = (byte[])MapShading_12B4E0?.Clone(),
                MapCeilingHeightmap_14B4E0 = (byte[])MapCeilingHeightmap_14B4E0?.Clone(),
                AngleTypes = (byte[,])AngleTypes?.Clone(),
                Seed = Seed
            };
        }
    }
}
