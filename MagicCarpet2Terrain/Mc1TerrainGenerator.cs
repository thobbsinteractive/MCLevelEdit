using MagicCarpet2Terrain.Model;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Generates Magic Carpet 1 terrain, ported from remc1 GenerateLevelMap_31AA0 (Mc1Level.cpp), which was checked
    /// byte for byte against the original game in DOSBox. Most passes are the same as Magic Carpet 2's and are
    /// shared with <see cref="TerrainGenerator"/>; MC1 runs them in a different order and has its own versions of
    /// SmoothTerrain, MarkSteepTerrain, SetTerrainAngles, MarkWaterEdges and ComputeTerrainShading.
    /// MC1 has no caves and no Night levels, so the map type is not used.
    /// </summary>
    public class Mc1TerrainGenerator : TerrainGenerator
    {
        private static readonly int[,] Around = { { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } };
        private static readonly int[,] Cross = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };

        /// <summary>The mark MarkSteepTerrain gives steep tiles: 6 in Magic Carpet, 1 in Hidden Worlds.</summary>
        protected virtual byte SteepMark => 6;

        /// <summary>Hidden Worlds marks flat highland after the steep terrain; Magic Carpet does not.</summary>
        protected virtual void MarkFlatHighland(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, GenerationParameters genParams)
        {
        }

        /// <summary>The whole level map (remc1 GenerateLevelMap_31AA0). <paramref name="stage"/> is not used.</summary>
        public override Terrain CalculateTerrain(GenerationParameters genParams, byte stage = 18)
        {
            MaxMapSize = genParams.MapSize;
            int size = MaxMapSize * MaxMapSize;

            var mapEntityIndex_15B4E0 = new short[size];
            var mapHeightmap_11B4E0 = new byte[size];
            var mapAngle_13B4E0 = new byte[size];
            var mapTerrainType_10B4E0 = new byte[size];
            var mapShading_12B4E0 = new byte[size];

            ushort seed = genParams.Seed;
            sub_B5E70_decompress_terrain_map_level(mapEntityIndex_15B4E0, genParams.Seed, genParams.Offset, (ushort)genParams.Raise, genParams.Gnarl);
            sub_44DB0_truncTerrainHeight(mapEntityIndex_15B4E0, mapHeightmap_11B4E0);
            System.Array.Clear(mapEntityIndex_15B4E0, 0, size);
            sub_44E40_generate_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.River, (byte)genParams.LRiver, ref seed);
            sub_45AA0_setMax4Tiles(mapHeightmap_11B4E0, mapAngle_13B4E0);
            sub_440D0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.SnLin);
            sub_45060(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);
            sub_44320(mapAngle_13B4E0);
            sub_45210(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);
            MarkSteepTerrain_33180(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.BhFlt);
            MarkFlatHighland(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams);
            sub_43FC0(mapAngle_13B4E0);
            System.Array.Clear(mapTerrainType_10B4E0, 0, size);
            SmoothTerrain_31BB0(mapHeightmap_11B4E0, mapAngle_13B4E0);
            sub_43EE0_add_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0);
            var angleTypes = SetTerrainAngles_32560(mapAngle_13B4E0, mapTerrainType_10B4E0, ref seed);
            MarkWaterEdges_31D40(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0);
            ComputeTerrainShading_329C0(mapHeightmap_11B4E0, mapShading_12B4E0, out seed);

            return new Terrain()
            {
                MapEntityIndex_15B4E0 = mapEntityIndex_15B4E0,
                MapTerrainType_10B4E0 = mapTerrainType_10B4E0,
                MapHeightmap_11B4E0 = mapHeightmap_11B4E0,
                MapAngle_13B4E0 = mapAngle_13B4E0,
                MapShading_12B4E0 = mapShading_12B4E0,
                AngleTypes = angleTypes,
                Seed = seed,
                GenerationParameters = genParams
            };
        }

        protected static int Tile(int x, int y) => (x & 0xFF) | ((y & 0xFF) << 8);

        /// <summary>remc1 SmoothTerrain_31BB0: like MC2's, but the centre is not in the average of the 8 neighbours.</summary>
        private static void SmoothTerrain_31BB0(byte[] height, byte[] angle)
        {
            for (int i = 0; i < 0x10000; i++)
            {
                if ((angle[i] & 7) == 0)
                    continue;
                int x = i & 0xFF;
                int y = i >> 8;
                byte center = height[i];
                byte highest = center;
                byte lowest = center;
                uint sum = 0;
                for (int d = 0; d < 8; d++)
                {
                    byte h = height[Tile(x + Around[d, 0], y + Around[d, 1])];
                    sum += h;
                    if (h > highest)
                        highest = h;
                    if (h < lowest)
                        lowest = h;
                }
                byte average = (byte)(sum >> 3);
                byte below = (byte)(center - lowest);
                byte above = (byte)(highest - center);
                byte step = below > 4 ? below : above;
                if (step > 4)
                    height[i] = step > 10 ? average : (byte)((center + average) >> 1);
            }
        }

        /// <summary>
        /// remc1 MarkTerrainByRange (33180 / 31C10): marks tiles by the height range of the 4 neighbours, then sets 1
        /// on the marked tiles next to the classes 3, 2, 5 and 4. The terrain types are used as scratch (a copy of the angles).
        /// </summary>
        protected static void MarkTerrainByRange(byte[] height, byte[] angle, byte[] type, bool highland, byte minHeight, byte range, byte mark)
        {
            System.Array.Copy(angle, type, 0x10000);
            for (int i = 0; i < 0x10000; i++)
            {
                if (highland && height[i] <= minHeight)
                    continue;
                int x = i & 0xFF;
                int y = i >> 8;
                byte highest = height[i];
                byte lowest = height[i];
                for (int d = 0; d < 4; d++)
                {
                    byte h = height[Tile(x + Cross[d, 0], y + Cross[d, 1])];
                    if (h > highest)
                        highest = h;
                    if (h < lowest)
                        lowest = h;
                }
                if (angle[i] == 0)
                    continue;
                int difference = highest - lowest;
                if (highland ? difference < range : difference >= range)
                    angle[i] = mark;
            }
            for (int i = 0; i < 0x10000; i++)
            {
                if (angle[i] != mark)
                    continue;
                int x = i & 0xFF;
                int y = i >> 8;
                int count3 = 0, count2 = 0, count5 = 0, count4 = 0;
                for (int d = 0; d < 8; d++)
                {
                    byte a = angle[Tile(x + Around[d, 0], y + Around[d, 1])];
                    if (a == 3) count3++;
                    if (a == 2) count2++;
                    if (a == 5) count5++;
                    if (a == 4) count4++;
                }
                if (count3 != 0 ? (count2 != 0 || count5 != 0 || count4 != 0) : (count2 != 0 || (count5 != 0 && count4 != 0)))
                    angle[i] = 1;
            }
        }

        /// <summary>remc1 MarkSteepTerrain_33180.</summary>
        private void MarkSteepTerrain_33180(byte[] height, byte[] angle, byte[] type, byte minRange)
        {
            MarkTerrainByRange(height, angle, type, false, 0, minRange, SteepMark);
        }

        /// <summary>remc1 MarkWaterEdges_31D40: marks water tiles away from land and from typed tiles with angle bit 8.</summary>
        private static void MarkWaterEdges_31D40(byte[] height, byte[] angle, byte[] type)
        {
            for (int i = 0; i < 0x10000; i++)
            {
                angle[i] &= 0xF7;
                if (height[i] != 0)
                    continue;
                int x = i & 0xFF;
                int y = i >> 8;
                bool land = false;
                for (int d = 0; d < 8; d++)
                    land |= height[Tile(x + Around[d, 0], y + Around[d, 1])] != 0;
                if (land)
                    continue;
                if (type[i] != 0 || type[Tile(x - 1, y)] != 0 || type[Tile(x - 1, y - 1)] != 0 || type[Tile(x, y - 1)] != 0)
                    continue;
                angle[i] |= 8;
            }
        }

        private static int AngleKey(int a, int b, int c, int d) => a * 343 + b * 49 + c * 7 + d;

        // A record of 25 bytes: count, 12 terrain types, 12 angles
        private const int RECORD_SIZE = 25;

        private static void AddAngleType(byte[] records, int key, byte type, byte angle)
        {
            int record = key * RECORD_SIZE;
            byte count = records[record];
            if (count >= 12)
                return;
            records[record] = (byte)(count + 1);
            records[record + 1 + count] = type;
            records[record + 0x0D + count] = angle;
        }

        /// <summary>
        /// remc1 SetTerrainAngles_32560: the terrain type of a tile comes from the heights (0-6) of its 4 corners;
        /// where several of the 148 patterns (in their 8 rotations / mirrors) fit, one is picked at random.
        /// </summary>
        /// <returns>The first pattern of each key (remc1 angleTypes_B5D40), which buildings use again.</returns>
        private byte[,] SetTerrainAngles_32560(byte[] angle, byte[] type, ref ushort seed)
        {
            // The patterns are the same table as MC2's (remc1 anglePatterns_9075C = remc2 unk_D47E0)
            var records = new byte[2401 * RECORD_SIZE];
            for (int i = 0; i < 148; i++)
            {
                int a = unk_D47E0[i * 4], b = unk_D47E0[i * 4 + 1], c = unk_D47E0[i * 4 + 2], d = unk_D47E0[i * 4 + 3];
                if (a < 0 || b < 0 || c < 0 || d < 0)
                    continue;
                byte t = (byte)i;
                AddAngleType(records, AngleKey(a, b, c, d), t, 0x00);
                AddAngleType(records, AngleKey(b, a, d, c), t, 0x10);
                AddAngleType(records, AngleKey(c, d, a, b), t, 0x30);
                AddAngleType(records, AngleKey(d, c, b, a), t, 0x20);
                AddAngleType(records, AngleKey(b, c, d, a), t, 0x60);
                AddAngleType(records, AngleKey(c, b, a, d), t, 0x70);
                AddAngleType(records, AngleKey(d, a, b, c), t, 0x50);
                AddAngleType(records, AngleKey(a, d, c, b), t, 0x40);
            }
            var angleTypes = new byte[Terrain.ANGLE_TYPE_KEYS, 2];
            for (int key = 0; key < 2401; key++)
            {
                int record = key * RECORD_SIZE;
                angleTypes[key, 0] = records[record] != 0 ? records[record + 1] : (byte)1;
                angleTypes[key, 1] = records[record] != 0 ? records[record + 0x0D] : (byte)0;
            }
            for (int i = 0; i < 0x10000; i++)
            {
                if (type[i] != 0)
                    continue;
                int x = i & 0xFF;
                int y = i >> 8;
                int key = AngleKey(angle[i] & 7, angle[Tile(x + 1, y)] & 7, angle[Tile(x + 1, y + 1)] & 7, angle[Tile(x, y + 1)] & 7);
                int record = key * RECORD_SIZE;
                byte count = records[record];
                if (count == 0)
                {
                    type[i] = 1;
                    continue;
                }
                seed = unchecked((ushort)(seed * 0x24A1 + 0x24DF));
                byte pick = (byte)(seed % (count + 1));
                if (pick >= count)
                    pick = 0;
                type[i] = records[record + 1 + pick];
                angle[i] = (byte)((angle[i] & 7) + records[record + 0x0D + pick]);
            }
            return angleTypes;
        }

        /// <summary>remc1 ComputeTerrainShading_329C0: like MC2's Day shading. It starts the seed again from 0.</summary>
        private static void ComputeTerrainShading_329C0(byte[] height, byte[] shading, out ushort seed)
        {
            seed = 0;
            for (int i = 0; i < 0x10000; i++)
            {
                int x = i & 0xFF;
                int y = i >> 8;
                byte shade = unchecked((byte)(height[Tile(x - 1, y - 1)] - height[Tile(x + 1, y + 1)] + 0x20));
                if (shade == 0x20)
                {
                    seed = unchecked((ushort)(seed * 0x24A1 + 0x24DF));
                    shade = (byte)(seed % 9 + 0x1C);
                }
                else if ((sbyte)shade < 0x1C)
                    shade = (byte)((shade & 3) + 0x1C);
                else if ((sbyte)shade > 0x28)
                    shade = (byte)((shade & 7) + 0x28);
                shading[i] = shade;
            }
        }
    }

    /// <summary>
    /// Generates Magic Carpet: Hidden Worlds terrain (remc1, MC1_VARIANT_HW): Magic Carpet's, except that steep
    /// terrain is marked 1 instead of 6 and flat highland is marked after it (remc1 MarkFlatHighland_31C10).
    /// </summary>
    public class HiddenWorldsTerrainGenerator : Mc1TerrainGenerator
    {
        protected override byte SteepMark => 1;

        protected override void MarkFlatHighland(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, GenerationParameters genParams)
        {
            // remc1 MarkFlatHighland_31C10(source, snLin)
            MarkTerrainByRange(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, true, genParams.Source, (byte)genParams.SnLin, 6);
        }
    }
}
