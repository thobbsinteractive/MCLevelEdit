using MagicCarpet2Terrain.Model;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Generates Magic Carpet 2 cave levels. The terrain is made as for Day and Night levels, except that
    /// instead of marking water edges, a cave ceiling is built down from the level's basic height and squares
    /// where it does not clear the floor become solid rock (angle bit 8). See remc2 GenerateLevelMap_43830.
    /// </summary>
    public class CaveTerrainGenerator : TerrainGenerator
    {
        protected override byte[] ChangeAngleOfTerrain(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, GenerationParameters genParams)
        {
            var secondHeightmap_14B4E0 = new byte[mapHeightmap_11B4E0.Length];
            sub_43B40_generate_cave_ceiling(mapHeightmap_11B4E0, mapAngle_13B4E0, secondHeightmap_14B4E0, genParams.BasicHeight);//224b40
            return secondHeightmap_14B4E0;
        }

        /// <summary>
        /// Builds the ceiling down from the basic height and marks squares where the ceiling
        /// does not clear the floor (solid rock) with angle bit 8 (remc2 GenerateCaveCeiling_43B40).
        /// </summary>
        private void sub_43B40_generate_cave_ceiling(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] secondHeightmap_14B4E0, byte basicHeight)//224b40
        {
            for (int i = 0; i < mapHeightmap_11B4E0.Length; i++)
            {
                int locHeight = mapHeightmap_11B4E0[i];
                if (locHeight > basicHeight)
                    locHeight = basicHeight;
                secondHeightmap_14B4E0[i] = (byte)(basicHeight - locHeight);
                if (basicHeight - locHeight > mapHeightmap_11B4E0[i])
                {
                    mapAngle_13B4E0[i] &= 0xF7;
                }
                else
                {
                    secondHeightmap_14B4E0[i] = unchecked((byte)(mapHeightmap_11B4E0[i] - 1));
                    mapAngle_13B4E0[i] |= 8;
                }
            }
            sub_43BB0_randomise_ceiling_height(mapHeightmap_11B4E0, mapAngle_13B4E0, secondHeightmap_14B4E0);
        }

        /// <summary>Roughens the cave ceiling, then re-marks solid rock (remc2 RandomiseCeilingHeight_43BB0).</summary>
        private void sub_43BB0_randomise_ceiling_height(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] secondHeightmap_14B4E0)//224bb0
        {
            uint randSeed = 37487429;
            for (int i = 0; i < mapHeightmap_11B4E0.Length; i++)
            {
                if ((mapAngle_13B4E0[i] & 8) == 0)
                {
                    randSeed = unchecked(9377 * randSeed + 9439);
                    int fuzzyHeight = (int)(randSeed % 7) - 3 + secondHeightmap_14B4E0[i];
                    if (fuzzyHeight < 0)
                        fuzzyHeight = 0;
                    if (fuzzyHeight > 254)
                        fuzzyHeight = 254;
                    secondHeightmap_14B4E0[i] = (byte)fuzzyHeight;
                }
            }
            for (int i = 0; i < mapHeightmap_11B4E0.Length; i++)
            {
                if (secondHeightmap_14B4E0[i] > mapHeightmap_11B4E0[i])
                {
                    mapAngle_13B4E0[i] &= 0xF7;
                }
                else
                {
                    secondHeightmap_14B4E0[i] = unchecked((byte)(mapHeightmap_11B4E0[i] - 1));
                    mapAngle_13B4E0[i] |= 8;
                }
            }
        }
    }
}
