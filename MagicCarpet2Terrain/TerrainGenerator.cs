using MagicCarpet2Terrain.Interfaces;
using MagicCarpet2Terrain.Model;
using MagicCarpet2Terrain.Structs;
using System;
using System.Threading.Tasks;

namespace MCLevelEdit.Application.Services
{
    public class TerrainGenerator : ITerrainGenerator
    {
        private readonly sbyte[] unk_D47E0 = {
        0x00,0x00, 0x00,0x00, 0x01,0x01, 0x01,0x01, 0x02,0x02, 0x02,0x02, 0x03,0x03, 0x03,0x03,
        0x04,0x04, 0x04,0x04, 0x05,0x05, 0x05,0x05, 0x06,0x06, 0x06,0x06, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1,
        -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, -1,-1, 0x06,0x00, 0x01,0x04,
        0x01,0x01, 0x00,0x00, 0x01,0x00, 0x00,0x00, 0x01,0x00, 0x01,0x00, 0x00,0x01, 0x01,0x01,
        0x06,0x06, 0x04,0x04, 0x06,0x04, 0x06,0x04, 0x06,0x04, 0x06,0x06, 0x04,0x06, 0x04,0x04,
        0x04,0x04, 0x00,0x00, 0x04,0x00, 0x00,0x00, 0x00,0x04, 0x04,0x04, 0x00,0x04, 0x00,0x04,
        0x01,0x03, 0x03,0x03, 0x01,0x03, 0x01,0x03, 0x03,0x01, 0x01,0x01, 0x01,0x01, 0x03,0x03,
        0x05,0x01, 0x01,0x01, 0x01,0x01, 0x05,0x05, 0x01,0x05, 0x01,0x05, 0x01,0x05, 0x05,0x05,
        0x02,0x05, 0x02,0x05, 0x05,0x02, 0x02,0x02, 0x02,0x05, 0x05,0x05, 0x05,0x05, 0x02,0x02,
        0x04,0x04, 0x03,0x03, 0x04,0x03, 0x03,0x03, 0x03,0x04, 0x03,0x04, 0x03,0x04, 0x04,0x04,
        0x04,0x05, 0x05,0x05, 0x05,0x04, 0x04,0x04, 0x05,0x04, 0x05,0x04, 0x04,0x04, 0x05,0x05,
        0x01,0x02, 0x01,0x02, 0x02,0x01, 0x01,0x01, 0x01,0x02, 0x02,0x02, 0x01,0x01, 0x02,0x02,
        0x04,0x01, 0x01,0x01, 0x01,0x04, 0x01,0x04, 0x01,0x04, 0x04,0x04, 0x01,0x01, 0x04,0x04,
        0x01,0x06, 0x01,0x01, 0x06,0x06, 0x01,0x01, 0x06,0x01, 0x06,0x01, 0x06,0x01, 0x06,0x06,
        0x06,0x06, 0x00,0x00, 0x06,0x00, 0x06,0x00, 0x06,0x00, 0x06,0x06, 0x00,0x06, 0x00,0x00,
        0x02,0x01, 0x05,0x01, 0x01,0x01, 0x05,0x02, 0x05,0x01, 0x05,0x02, 0x02,0x01, 0x02,0x05,
        0x02,0x02, 0x01,0x05, 0x05,0x05, 0x01,0x02, 0x03,0x03, 0x04,0x01, 0x04,0x03, 0x04,0x01,
        0x01,0x01, 0x04,0x03, 0x01,0x04, 0x04,0x03, 0x03,0x04, 0x03,0x01, 0x01,0x03, 0x01,0x04,
        0x01,0x06, 0x04,0x06, 0x01,0x06, 0x01,0x04, 0x01,0x06, 0x06,0x04, 0x01,0x04, 0x06,0x04,
        0x01,0x06, 0x04,0x01, 0x01,0x06, 0x04,0x04, 0x06,0x04, 0x00,0x04, 0x00,0x04, 0x06,0x06,
        0x00,0x04, 0x00,0x06, 0x00,0x00, 0x04,0x06, 0x00,0x06, 0x04,0x04, 0x06,0x00, 0x06,0x04,
        0x06,0x00, 0x06,0x01, 0x01,0x00, 0x06,0x00, 0x01,0x06, 0x00,0x00, 0x01,0x06, 0x06,0x00,
        0x01,0x06, 0x01,0x00, 0x01,0x01, 0x00,0x06, 0x01,0x00, 0x04,0x00, 0x01,0x04, 0x00,0x04,
        0x01,0x04, 0x00,0x00, 0x01,0x01, 0x04,0x00, 0x04,0x01, 0x00,0x04, 0x01,0x04, 0x01,0x00,
        0x01,0x05, 0x05,0x04, 0x04,0x05, 0x04,0x01, 0x01,0x01, 0x04,0x05, 0x01,0x05, 0x04,0x05,
        0x01,0x04, 0x01,0x05, 0x01,0x04, 0x04,0x05, 0x01,0x06, 0x00,0x04, 0x06,0x01, 0x00,0x04,
        0x06,0x06, 0x05,0x05, 0x06,0x05, 0x06,0x05, 0x06,0x05, 0x06,0x06, 0x05,0x06, 0x05,0x05,
        0x06,0x06, 0x03,0x03, 0x06,0x03, 0x06,0x03, 0x06,0x03, 0x06,0x06, 0x03,0x06, 0x03,0x03,
        0x01,0x05, 0x05,0x06, 0x06,0x05, 0x06,0x01, 0x01,0x01, 0x06,0x05, 0x01,0x05, 0x06,0x05,
        0x01,0x06, 0x01,0x05, 0x01,0x06, 0x06,0x05, 0x01,0x03, 0x03,0x06, 0x06,0x03, 0x06,0x01,
        0x01,0x01, 0x06,0x03, 0x01,0x03, 0x06,0x03, 0x01,0x06, 0x01,0x03, 0x01,0x06, 0x06,0x03
        }; // weak

        private int MaxMapSize = 256;

        public Terrain CalculateTerrain(GenerationParameters genParams, byte stage = 18)
        {
            MaxMapSize = genParams.MapMapSize;

            short[] mapEntityIndex_15B4E0 = new short[MaxMapSize * MaxMapSize];
            byte[] mapHeightmap_11B4E0 = new byte[MaxMapSize * MaxMapSize];
            byte[] mapAngle_13B4E0 = new byte[MaxMapSize * MaxMapSize];
            byte[] mapTerrainType_10B4E0 = new byte[MaxMapSize * MaxMapSize];
            byte[] mapShading_12B4E0 = new byte[MaxMapSize * MaxMapSize];
            byte[,] x_BYTE_F2CD0x = new byte[7 * 7 * 7 * 7, 2]; // 233cd0//4802 //4816

            ushort seed_17B4E0 = genParams.Seed;
            if (stage > 0)
                sub_B5E70_decompress_terrain_map_level(mapEntityIndex_15B4E0, (short)genParams.Seed, genParams.Offset, genParams.Raise, genParams.Gnarl);
            if (stage > 1)
                sub_44DB0_truncTerrainHeight(mapEntityIndex_15B4E0, mapHeightmap_11B4E0);//225db0 //trunc and create
            if (stage > 2)
            {
                for (int i = 0; i < mapEntityIndex_15B4E0.Length; i++)
                {
                    mapEntityIndex_15B4E0[i] = 0;
                }
            }
            if (stage > 3)
                sub_44E40_generate_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.River, genParams.LRiver, ref seed_17B4E0);//225e40 //add any fields
            if (stage > 4)
                sub_45AA0_setMax4Tiles(mapHeightmap_11B4E0, mapAngle_13B4E0);
            if (stage > 5)
                sub_440D0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.SnLin);//2250d0
            if (stage > 6)
                sub_45060(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226060
            if (stage > 7)
                sub_44320(mapAngle_13B4E0);//225320
            if (stage > 8)
                sub_45210(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226210
            if (stage > 9)
                sub_454F0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.Source, genParams.RkSte);//2264f0
            if (stage > 10)
                sub_45600(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.BhFlt);//226600
            if (stage > 11)
                sub_43FC0(mapAngle_13B4E0);//224fc0
            if (stage > 12)
            {
                for (int i = 0; i < mapTerrainType_10B4E0.Length; i++)
                {
                    mapTerrainType_10B4E0[i] = 0;
                }
            }
            if (stage > 13)
                sub_43970_smooth_terrain(mapHeightmap_11B4E0, mapAngle_13B4E0);//224970 // smooth terrain
            if (stage > 14)
                sub_43EE0_add_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0);//224ee0 // add rivers
            if (stage > 15)
                sub_44580(mapAngle_13B4E0, mapTerrainType_10B4E0, x_BYTE_F2CD0x, ref seed_17B4E0);//225580 //set angle of terrain
            if (stage > 16)
            {
                if (genParams.MapType == MapType.Cave)
                {
                    //sub_43B40_change_angle_of_terrain
                }
                else
                {
                    sub_43D50_change_angle_of_terrain(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0);//224d50
                }
            }
            if (stage > 17)
                sub_44D00_shade_terrain(mapHeightmap_11B4E0, mapShading_12B4E0, genParams.MapType, ref seed_17B4E0);//225d00

            return new Terrain()
            {
                MapEntityIndex_15B4E0 = mapEntityIndex_15B4E0,
                MapTerrainType_10B4E0 = mapTerrainType_10B4E0,
                MapHeightmap_11B4E0 = mapHeightmap_11B4E0,
                MapAngle_13B4E0 = mapAngle_13B4E0,
                MapShading_12B4E0 = mapShading_12B4E0,
                GenerationParameters = genParams
            };
        }

        private void sub_B5E70_decompress_terrain_map_level(short[] mapEntityIndex_15B4E0, short seed, ushort offset, ushort raise, ushort gnarl)
        {
            UAxis2D sumEnt = new UAxis2D();

            mapEntityIndex_15B4E0[offset] = (short)raise;//32c4e0 //first seed
            for (short i = 7; i >= 0; i--)
            {
                sumEnt.Word = offset;
                for (int j = 1 << (7 - i); j > 0; j--)
                {
                    for (int k = 1 << (7 - i); k > 0; k--)
                    {
                        sub_B5EFA(mapEntityIndex_15B4E0, (short)(1 << i), ref sumEnt, gnarl, ref seed);//355220
                    }
                    sumEnt.Word += (ushort)((2 * (1 << i)) << 8);
                }
                for (int j = 1 << (7 - i); j > 0; j--)
                {
                    for (int k = 1 << (7 - i); k > 0; k--)
                    {
                        sub_B5F8F(mapEntityIndex_15B4E0, (short)(1 << i), ref sumEnt, gnarl, ref seed);
                    }
                    sumEnt.Word += (ushort)((2 * (1 << i)) << 8);
                }
            }
        }

        private void sub_B5EFA(short[] mapEntityIndex_15B4E0, short a1, ref UAxis2D indexx, ushort gnarl, ref short nextRand)//296EFA
        {
            //  X-.-X
            //   \  |
            //    E .
            //      |
            //  B---X

            short sumEnt;
            ushort srandNumber;

            sumEnt = mapEntityIndex_15B4E0[indexx.Word];
            indexx.X += (byte)(2 * a1);
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            indexx.Y += (byte)(2 * a1);
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            indexx.X -= (byte)(2 * a1);
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            indexx.X += (byte)a1;
            indexx.Y -= (byte)a1;
            srandNumber = (ushort)(9377 * nextRand + 9439);
            nextRand = (short)srandNumber;
            if (mapEntityIndex_15B4E0[indexx.Word] == 0)
                mapEntityIndex_15B4E0[indexx.Word] = (short)(srandNumber % (ushort)(2 * gnarl + 1)
                + srandNumber % (ushort)((a1 << 6) + 1) + (sumEnt >> 2) - 32 * a1 - gnarl);
            indexx.X += (byte)a1;
            indexx.Y -= (byte)a1;
        }

        //----- (000B5F8F) --------------------------------------------------------
        private void sub_B5F8F(short[] mapEntityIndex_15B4E0, short a1, ref UAxis2D indexx, ushort gnarl, ref short nextRand)//296f8f
        {

            //   X
            //   |\
            // B E X
            //  \ /
            //   X

            short sumEnt;
            short sumEnt2;
            ushort srandNumber;

            sumEnt = mapEntityIndex_15B4E0[indexx.Word];
            sumEnt2 = sumEnt;
            indexx.X += (byte)a1;
            indexx.Y -= (byte)a1;
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            indexx.X += (byte)a1;
            indexx.Y += (byte)a1;
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            indexx.X -= (byte)a1;
            indexx.Y += (byte)a1;
            sumEnt += mapEntityIndex_15B4E0[indexx.Word];
            srandNumber = (ushort)(9377 * nextRand + 9439);
            sumEnt2 += mapEntityIndex_15B4E0[indexx.Word];
            indexx.Y -= (byte)a1;
            if (mapEntityIndex_15B4E0[indexx.Word] == 0)
                mapEntityIndex_15B4E0[indexx.Word] = (short)(srandNumber % (ushort)(2 * gnarl + 1)
                + srandNumber % (ushort)((a1 << 6) + 1) + (ushort)(sumEnt >> 2) - 32 * a1 - gnarl);

            //   X
            //  /|
            // X E-.
            //  \   \
            //   .-B R

            indexx.X -= (byte)(2 * a1);
            indexx.Y += (byte)a1;
            sumEnt2 += mapEntityIndex_15B4E0[indexx.Word];
            indexx.X += (byte)a1;
            indexx.Y += (byte)a1;
            sumEnt2 += mapEntityIndex_15B4E0[indexx.Word];
            indexx.Y -= (byte)a1;
            srandNumber = (ushort)(9377 * srandNumber + 9439);
            nextRand = (short)srandNumber;
            if (mapEntityIndex_15B4E0[indexx.Word] == 0)
                mapEntityIndex_15B4E0[indexx.Word] = (short)(srandNumber % (ushort)(2 * gnarl + 1)
                + srandNumber % (ushort)((a1 << 6) + 1) + (ushort)(sumEnt2 >> 2) - 32 * a1 - gnarl);
            indexx.X += (byte)(2 * a1);
            indexx.Y -= (byte)a1;
        }

        private void sub_44DB0_truncTerrainHeight(short[] mapEntityIndex_15B4E0, byte[] mapHeightmap_11B4E0)//225db0 // map to heightmap
        {
            int revMaxEnt = 0;
            uint weightedVar;
            int maxEnt = -32000;
            int minEnt = 32000;
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                if (mapEntityIndex_15B4E0[i] > maxEnt)
                    maxEnt = mapEntityIndex_15B4E0[i];
                if (mapEntityIndex_15B4E0[i] < minEnt)
                    minEnt = mapEntityIndex_15B4E0[i];
            }
            if (maxEnt > 0)
                revMaxEnt = 0xC40000 / maxEnt;
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                weightedVar = (uint)(revMaxEnt * mapEntityIndex_15B4E0[i] >> 16);
                mapEntityIndex_15B4E0[i] = 0;
                if ((weightedVar & 0x8000u) != 0)//water level trunc
                    weightedVar = 0;
                if (weightedVar > 196)//trunc max height
                    weightedVar = 196;
                mapHeightmap_11B4E0[i] = (byte)weightedVar;
            }
        }

        private void sub_44E40_generate_rivers(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, int riverCount, ushort minSmooth, ref ushort seed_17B4E0)//225e40 rivers?
        {
            UAxis2D index = new UAxis2D();
            int locCount = riverCount;
            int i = 0;
            for (i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                if (mapHeightmap_11B4E0[i] > 0)
                    mapAngle_13B4E0[i] = 5;
                else
                    mapAngle_13B4E0[i] = 0;
            }
            i = 0;
            while ((locCount > 0) && (i < 1000))
            {
                for (i = 0; i < 1000; i++)
                {
                    seed_17B4E0 = (ushort)(9377 * seed_17B4E0 + 9439);
                    index.Word = (ushort)(seed_17B4E0 % 0xffffu);
                    if ((mapHeightmap_11B4E0[index.Word] > minSmooth) && mapAngle_13B4E0[index.Word] > 0)
                    {
                        sub_44EE0_smooth_tiles(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, index);
                        locCount--;
                        break;
                    }
                }
            }
            for (i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                mapTerrainType_10B4E0[i] = 255;
            }
        }

        private void sub_44EE0_smooth_tiles(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, UAxis2D axis)//225ee0
        {
            //  X-X-X
            //  |   |
            //  X B X
            //  | | |
            //  X X-X

            UAxis2D tempAxis2 = new UAxis2D();
            UAxis2D tempAxis1 = new UAxis2D();
            byte centralHeight;
            byte minHeight;

            tempAxis1.Word = axis.Word;
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                mapTerrainType_10B4E0[i] = 3;
            }

            centralHeight = mapHeightmap_11B4E0[axis.Word];

            do
            {
                mapTerrainType_10B4E0[tempAxis1.Word] = 0;
                tempAxis1.Y--;
                minHeight = 255;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && mapHeightmap_11B4E0[tempAxis1.Word] < 255)
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.X++;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.Y++;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.Y++;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.X--;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.X--;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.Y--;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                tempAxis1.Y--;
                if (mapTerrainType_10B4E0[tempAxis1.Word] > 0 && minHeight > mapHeightmap_11B4E0[tempAxis1.Word])
                {
                    minHeight = mapHeightmap_11B4E0[tempAxis1.Word];
                    tempAxis2.Word = tempAxis1.Word;
                }
                if (mapAngle_13B4E0[tempAxis2.Word] == 0 || minHeight == 255)
                    break;

                if (minHeight > centralHeight)//if near tile is higger then central tile set central as near tile
                    mapHeightmap_11B4E0[tempAxis2.Word] = centralHeight;

                centralHeight = mapHeightmap_11B4E0[tempAxis2.Word];
                tempAxis1.Word = tempAxis2.Word;
            } while (centralHeight > 0);

            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                if (mapTerrainType_10B4E0[i] == 0)
                {
                    mapAngle_13B4E0[i] = 0;
                }
            }
        }

        private void sub_45AA0_setMax4Tiles(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0)//226aa0
        {
            //  X-X
            //  | |
            //  B-X

            UAxis2D indexx = new UAxis2D();
            byte angleIndex;
            byte minHeight;
            byte maxHeight;
            bool runAgain;

            do
            {
                runAgain = false;
                for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
                {
                    indexx.Word = (ushort)i;
                    angleIndex = 0;
                    if (mapAngle_13B4E0[indexx.Word] == 0)
                        angleIndex = 1;
                    minHeight = mapHeightmap_11B4E0[indexx.Word];
                    maxHeight = minHeight;
                    indexx.X++;
                    if (mapAngle_13B4E0[indexx.Word] == 0)
                        angleIndex++;
                    if (minHeight > mapHeightmap_11B4E0[indexx.Word])
                        minHeight = mapHeightmap_11B4E0[indexx.Word];
                    if (maxHeight < mapHeightmap_11B4E0[indexx.Word])
                        maxHeight = mapHeightmap_11B4E0[indexx.Word];
                    indexx.Y++;
                    if (mapAngle_13B4E0[indexx.Word] == 0)
                        angleIndex++;
                    if (minHeight > mapHeightmap_11B4E0[indexx.Word])
                        minHeight = mapHeightmap_11B4E0[indexx.Word];
                    if (maxHeight < mapHeightmap_11B4E0[indexx.Word])
                        maxHeight = mapHeightmap_11B4E0[indexx.Word];
                    indexx.X--;
                    if (mapAngle_13B4E0[indexx.Word] == 0)
                        angleIndex++;
                    if (minHeight > mapHeightmap_11B4E0[indexx.Word])
                        minHeight = mapHeightmap_11B4E0[indexx.Word];
                    if (maxHeight < mapHeightmap_11B4E0[indexx.Word])
                        maxHeight = mapHeightmap_11B4E0[indexx.Word];
                    indexx.Y--;
                    if (maxHeight != minHeight && angleIndex == 4)
                    {
                        runAgain = true;
                        mapHeightmap_11B4E0[indexx.Word] = minHeight;
                        indexx.X++;
                        mapHeightmap_11B4E0[indexx.Word] = minHeight;
                        indexx.Y++;
                        mapHeightmap_11B4E0[indexx.Word] = minHeight;
                        indexx.X--;
                        mapHeightmap_11B4E0[indexx.Word] = minHeight;
                        indexx.Y--;
                    }
                }
            } while (runAgain);
        }

        private void sub_440D0(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, ushort snLin)//2250d0
        {
            //    X
            //   / \
            //  X B X
            //   \|/
            //    X

            byte maxHeight;
            byte minHeight;
            int diffHeight;
            byte ang3;
            byte ang2;
            byte ang5;
            UAxis2D index = new UAxis2D();

            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                if (mapAngle_13B4E0[index.Word] == 5)
                {
                    maxHeight = 0;
                    minHeight = 255;
                    if (mapHeightmap_11B4E0[index.Word] > 0)
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (mapHeightmap_11B4E0[index.Word] < 255)
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.Y--;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X++;
                    index.Y++;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X--;
                    index.Y++;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X--;
                    index.Y--;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    diffHeight = maxHeight - minHeight;
                    index.X++;
                    if (diffHeight <= snLin)
                    {
                        if (diffHeight == snLin)
                            mapAngle_13B4E0[index.Word] = 4;
                        else
                            mapAngle_13B4E0[index.Word] = 3;
                    }
                }
            }

            //  X-X
            //  | |
            //  B-X

            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                ang3 = 0;
                ang2 = 0;
                ang5 = 0;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3 = 1;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2 = 1;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5 = 1;
                index.X++;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                index.Y++;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                index.X--;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                index.Y--;
                if (ang2 == 0 && ang3 > 0 && ang5 > 0)
                {
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y--;
                }
            }
        }

        private void sub_45060(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, byte maxHeightCut, byte maxHeightDiffCut)//226060
        {
            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            byte maxHeight;
            byte minHeight;
            UAxis2D index = new UAxis2D();
            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, MaxMapSize * MaxMapSize);
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                maxHeight = 0;
                minHeight = 255;
                if (mapHeightmap_11B4E0[index.Word] > 0)
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (mapHeightmap_11B4E0[index.Word] < 255)
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X++;
                index.Y++;
                if (maxHeight < maxHeightCut && maxHeight - minHeight <= maxHeightDiffCut)
                {
                    if (mapAngle_13B4E0[index.Word] > 0)
                        mapAngle_13B4E0[index.Word] = 5;
                }
            }
        }

        private void sub_44320(byte[] mapAngle_13B4E0)//225320
        {
            //  X-X
            //  | |
            //  B-X

            byte ang0;
            byte ang3;
            byte ang5;

            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                ang0 = 0;
                ang3 = 0;
                ang5 = 0;
                if (mapAngle_13B4E0[index.Word] == 0)
                    ang0 = 1;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5 = 1;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3 = 1;
                index.X++;
                if (mapAngle_13B4E0[index.Word] == 0)
                    ang0++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                index.Y++;
                if (mapAngle_13B4E0[index.Word] == 0)
                    ang0++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                index.X--;
                if (mapAngle_13B4E0[index.Word] == 0)
                    ang0++;
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 3)
                    ang3++;
                index.Y--;
                if (ang3 > 0 && ang5 > 0)
                {
                    if (mapAngle_13B4E0[index.Word] == 5)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y--;
                }
                if (ang3 > 0 && ang0 > 0)
                {
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y--;
                }
                if (ang0 > 0 && ang5 > 0)
                {
                    if (mapAngle_13B4E0[index.Word] > 0)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] > 0)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] > 0)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] > 0)
                        mapAngle_13B4E0[index.Word] = 4;
                    index.Y--;
                }
            }
        }

        private void sub_45210(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, byte maxHeightCut, byte maxHeightDiffCut)//226210
        {
            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            byte ang2;
            byte ang5;
            byte maxHeight;
            byte minHeight;

            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, MaxMapSize * MaxMapSize);
            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                minHeight = 255;
                maxHeight = 0;
                ang2 = 0;
                ang5 = 0;
                if (mapHeightmap_11B4E0[index.Word] > 0)
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (mapHeightmap_11B4E0[index.Word] < 255)
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5 = 1;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2 = 1;
                index.X++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.X--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.X--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                if (mapAngle_13B4E0[index.Word] == 5)
                    ang5++;
                if (mapAngle_13B4E0[index.Word] == 2)
                    ang2++;
                index.X++;
                index.Y++;
                if (maxHeight < maxHeightCut)
                {
                    if (maxHeight - minHeight <= maxHeightDiffCut && mapAngle_13B4E0[index.Word] == 5)
                    {
                        if (ang5 + ang2 == 8)
                            mapAngle_13B4E0[index.Word] = 2;
                    }
                }
            }
        }

        private void sub_454F0(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte maxHeightCut, byte maxHeightDiffCut)//2264f0
        {
            //    X
            //	 / \
            //  X-B X
            //    |/
            //    X

            byte maxHeight;
            byte minHeight;
            UAxis2D index = new UAxis2D();
            for (int i = 0; i < 256 * 256; i++)
            {
                index.Word = (ushort)i;
                if (mapHeightmap_11B4E0[index.Word] > maxHeightCut)
                {
                    maxHeight = 0;
                    minHeight = 255;
                    if (mapHeightmap_11B4E0[index.Word] > 0)
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (mapHeightmap_11B4E0[index.Word] < 255)
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.Y--;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X++;
                    index.Y++;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X--;
                    index.Y++;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X--;
                    index.Y--;
                    if (maxHeight < mapHeightmap_11B4E0[index.Word])
                        maxHeight = mapHeightmap_11B4E0[index.Word];
                    if (minHeight > mapHeightmap_11B4E0[index.Word])
                        minHeight = mapHeightmap_11B4E0[index.Word];
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] > 0)
                    {
                        if (maxHeight - minHeight < maxHeightDiffCut)
                            mapAngle_13B4E0[index.Word] = 6;
                    }
                }
            }
        }

        private void sub_45600(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, byte a1)//226600
        {
            byte ang4;
            byte ang2;
            byte ang3;
            byte ang5;
            byte maxHeight;
            byte minHeight;

            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, MaxMapSize * MaxMapSize);
            UAxis2D index = new UAxis2D();

            //    X
            //	 / \
            //  X-B X
            //    |/
            //    X

            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                maxHeight = 0;
                minHeight = 255;
                if (mapHeightmap_11B4E0[index.Word] > 0)
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (mapHeightmap_11B4E0[index.Word] < 255)
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X++;
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X--;
                index.Y++;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X--;
                index.Y--;
                if (maxHeight < mapHeightmap_11B4E0[index.Word])
                    maxHeight = mapHeightmap_11B4E0[index.Word];
                if (minHeight > mapHeightmap_11B4E0[index.Word])
                    minHeight = mapHeightmap_11B4E0[index.Word];
                index.X++;
                if (mapAngle_13B4E0[index.Word] > 0 && maxHeight - minHeight >= a1)
                    mapAngle_13B4E0[index.Word] = 1;
            }

            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                if (mapAngle_13B4E0[index.Word] == 6)
                {
                    ang4 = 0;
                    ang2 = 0;
                    ang3 = 0;
                    ang5 = 0;
                    index.Y--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3 = 1;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2 = 1;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5 = 1;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4 = 1;
                    index.X++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.Y++;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.X--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.Y--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.Y--;
                    if (mapAngle_13B4E0[index.Word] == 3)
                        ang3++;
                    if (mapAngle_13B4E0[index.Word] == 2)
                        ang2++;
                    if (mapAngle_13B4E0[index.Word] == 5)
                        ang5++;
                    if (mapAngle_13B4E0[index.Word] == 4)
                        ang4++;
                    index.X++;
                    index.Y++;
                    if (ang3 > 0)
                    {
                        if (ang2 > 0 || ang5 > 0 || ang4 > 0)
                            mapAngle_13B4E0[index.Word] = 1;
                    }
                    else if (ang2 > 0 || (ang5 > 0 && ang4 > 0))
                    {
                        mapAngle_13B4E0[index.Word] = 1;
                    }
                }
            }
        }

        private void sub_43FC0(byte[] mapAngle_13B4E0)//224fc0
        {
            int sameAngle;
            byte centerAngle;

            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                index.Y--;
                centerAngle = mapAngle_13B4E0[index.Word];
                index.X++;
                sameAngle = (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.Y++;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.Y++;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.X--;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.X--;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.Y--;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.Y--;
                sameAngle += (centerAngle == mapAngle_13B4E0[index.Word] ? 1 : 0);
                index.X++;
                index.Y++;
                if (centerAngle > 0)
                {
                    if (sameAngle == 7)
                        mapAngle_13B4E0[index.Word] = centerAngle;
                }
            }
        }

        private void sub_43970_smooth_terrain(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0)//224970
        {
            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                mapHeightmap_11B4E0[index.Word] = sub_439A0(mapHeightmap_11B4E0, mapAngle_13B4E0, index.Word);
            }
        }

        private byte sub_439A0(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, ushort index)//2249a0
        {
            //    X
            //    |
            //  X-B-X
            //    |
            //    X

            byte maxHeight;
            byte minHeight;
            byte centerPoint;
            uint modSumaPoint;
            uint sumaPoint = 0;
            uint result = mapHeightmap_11B4E0[index];
            UAxis2D uindex = new UAxis2D();
            uindex.Word = index;
            if ((mapAngle_13B4E0[uindex.Word] & 7) != 0)
            {
                maxHeight = mapHeightmap_11B4E0[uindex.Word];
                minHeight = maxHeight;
                centerPoint = mapHeightmap_11B4E0[uindex.Word];
                uindex.Y--;
                sumaPoint = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.X++;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.Y++;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.Y++;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.X--;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.X--;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.Y--;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                uindex.Y--;
                sumaPoint += mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] > maxHeight)
                    maxHeight = mapHeightmap_11B4E0[uindex.Word];
                if (mapHeightmap_11B4E0[uindex.Word] < minHeight)
                    minHeight = mapHeightmap_11B4E0[uindex.Word];
                modSumaPoint = sumaPoint >> 3;
                if ((byte)(centerPoint - minHeight) <= 4)
                {
                    if ((byte)(maxHeight - centerPoint) <= 4)
                        return (byte)result;
                    if ((byte)(maxHeight - centerPoint) <= 10)
                        modSumaPoint = (centerPoint + modSumaPoint) >> 1;
                }
                else if ((byte)(centerPoint - minHeight) <= 10)
                {
                    return (byte)((modSumaPoint + centerPoint) >> 1);
                }
                result = modSumaPoint;
            }
            return (byte)result;
        }

        private void sub_43EE0_add_rivers(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0)//224ee0
        {
            //  X-X
            //  | |
            //  B-X

            byte ang0;
            byte ang4;
            byte heightM1;
            byte heightM2;
            byte angleM1;
            byte angleM2;
            byte angleM3;
            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                ang4 = 0;
                ang0 = 0;
                index.Word = (ushort)i;
                heightM1 = mapHeightmap_11B4E0[index.Word];
                index.X++;
                angleM1 = mapAngle_13B4E0[index.Word];
                if (angleM1 > 0)
                {
                    if (angleM1 == 4)
                        ang4 = 1;
                }
                else
                {
                    heightM2 = mapHeightmap_11B4E0[index.Word];
                    ang0 = 1;
                    if (heightM2 < heightM1)
                        heightM1 = heightM2;
                }
                index.Y++;
                angleM2 = mapAngle_13B4E0[index.Word];
                if (angleM2 > 0)
                {
                    if (angleM2 == 4)
                        ang4++;
                }
                else
                {
                    ang0++;
                    if (mapHeightmap_11B4E0[index.Word] < heightM1)
                        heightM1 = mapHeightmap_11B4E0[index.Word];
                }
                index.X--;
                angleM3 = mapAngle_13B4E0[index.Word];
                if (angleM3 > 0)
                {
                    if (angleM3 == 4)
                        ang4++;
                }
                else
                {
                    ang0++;
                    if (mapHeightmap_11B4E0[index.Word] < heightM1)
                        heightM1 = mapHeightmap_11B4E0[index.Word];
                }
                index.Y--;
                if (ang4 > 0 && ang0 > 0 && heightM1 == 0)
                {
                    mapHeightmap_11B4E0[index.Word] = 0;
                    index.X++;
                    mapHeightmap_11B4E0[index.Word] = 0;
                    index.Y++;
                    mapHeightmap_11B4E0[index.Word] = 0;
                    index.X--;
                    mapHeightmap_11B4E0[index.Word] = 0;
                    index.Y--;
                }
            }
        }

        private void sub_44580(byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, byte[,] x_BYTE_F2CD0x, ref ushort seed_17B4E0)//225580
        {
            byte point1;
            byte point2;
            byte point3;
            byte point4;
            byte actBufEnt;

            byte[] pdwScreenBuffer_351628 = new byte[2401 * 25];

            for (int i = 0; i < 148; i++)
            {
                if (unk_D47E0[4 * i + 0] >= 0)
                {
                    if (unk_D47E0[4 * i + 1] >= 0)
                    {
                        if (unk_D47E0[4 * i + 2] >= 0)
                        {
                            if (unk_D47E0[4 * i + 3] >= 0)
                            {
                                int idx = 25 * (49 * unk_D47E0[4 * i + 1] + 7 * unk_D47E0[4 * i + 2] + unk_D47E0[4 * i + 3] + 343 * unk_D47E0[4 * i]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 0;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i] + unk_D47E0[4 * i + 2] + 7 * unk_D47E0[4 * i + 3] + 343 * unk_D47E0[4 * i + 1]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 16;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i + 3] + unk_D47E0[4 * i + 1] + 7 * unk_D47E0[4 * i] + 343 * unk_D47E0[4 * i + 2]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 48;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i + 2] + unk_D47E0[4 * i] + 7 * unk_D47E0[4 * i + 1] + 343 * unk_D47E0[4 * i + 3]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 32;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i + 2] + 8 * unk_D47E0[4 * i + 3] - unk_D47E0[4 * i + 3] + unk_D47E0[4 * i] + 343 * unk_D47E0[4 * i + 1]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 96;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i + 1] + 8 * unk_D47E0[4 * i] - unk_D47E0[4 * i] + unk_D47E0[4 * i + 3] + 343 * unk_D47E0[4 * i + 2]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 112;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (49 * unk_D47E0[4 * i] + 7 * unk_D47E0[4 * i + 1] + unk_D47E0[4 * i + 2] + 343 * unk_D47E0[4 * i + 3]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 80;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }

                                idx = 25 * (343 * unk_D47E0[4 * i] + 7 * unk_D47E0[4 * i + 2] + unk_D47E0[4 * i + 1] + 49 * unk_D47E0[4 * i + 3]);
                                if (pdwScreenBuffer_351628[idx] < 12)
                                {
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 13] = 64;
                                    pdwScreenBuffer_351628[idx + pdwScreenBuffer_351628[idx] + 1] = (byte)i;
                                    pdwScreenBuffer_351628[idx]++;
                                }
                            }
                        }
                    }
                }
            }
            int index = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        for (int l = 0; l < 7; l++)
                        {
                            int idx = 25 * (49 * j + 7 * k + l + 343 * i);
                            if (pdwScreenBuffer_351628[idx] != 0)
                            {
                                x_BYTE_F2CD0x[index, 0] = pdwScreenBuffer_351628[idx + 1];
                                x_BYTE_F2CD0x[index, 1] = pdwScreenBuffer_351628[idx + 13];
                            }
                            else
                            {
                                x_BYTE_F2CD0x[index, 0] = 1;
                                x_BYTE_F2CD0x[index, 1] = 0;
                            }
                            index++;
                        }
                    }
                }
            }
            //adress 225bbd

            //  X-X
            //  | |
            //  B-X

            UAxis2D uindex = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                uindex.Word = (ushort)i;
                if (mapTerrainType_10B4E0[uindex.Word] == 0)
                {
                    point1 = (byte)(mapAngle_13B4E0[uindex.Word] & 7);
                    uindex.X++;
                    point2 = (byte)(mapAngle_13B4E0[uindex.Word] & 7);
                    uindex.Y++;
                    point3 = (byte)(mapAngle_13B4E0[uindex.Word] & 7);
                    uindex.X--;
                    point4 = (byte)(mapAngle_13B4E0[uindex.Word] & 7);
                    uindex.Y--;
                    actBufEnt = pdwScreenBuffer_351628[25 * (343 * point1 + 49 * point2 + point4 + 7 * point3)];
                    if (actBufEnt != 0)
                    {
                        seed_17B4E0 = (ushort)(9377 * seed_17B4E0 + 9439);
                        int idx = 0;
                        if (seed_17B4E0 % (actBufEnt + 1) >= actBufEnt)
                            idx = 25 * (343 * point1 + 49 * point2 + point4 + 7 * point3);
                        else
                            idx = seed_17B4E0 % (actBufEnt + 1) + 25 * (343 * point1 + 49 * point2 + point4 + 7 * point3);

                        mapTerrainType_10B4E0[uindex.Word] = pdwScreenBuffer_351628[idx + 1];
                        mapAngle_13B4E0[uindex.Word] = (byte)((mapAngle_13B4E0[uindex.Word] & 7) + pdwScreenBuffer_351628[idx + 13]);
                    }
                    else
                    {
                        mapTerrainType_10B4E0[uindex.Word] = 1;
                    }
                }
            }
        }

        private void sub_43D50_change_angle_of_terrain(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0)//224d50
        {

            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            byte point1;
            byte point2;
            UAxis2D index = new UAxis2D();
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                mapAngle_13B4E0[index.Word] &= 0xF7;
                if (mapHeightmap_11B4E0[index.Word] == 0)
                {
                    index.Y--;
                    point1 = (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.X++;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.Y++;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.Y++;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.X--;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.X--;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.Y--;
                    point1 += (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.Y--;
                    point2 = (byte)(mapHeightmap_11B4E0[index.Word] != 0 ? 1 : 0);
                    index.X++;
                    index.Y++;

                    //  X-X
                    //  | |
                    //  B-X

                    if ((point2 + point1) == 0)
                    {
                        point1 = (byte)(mapTerrainType_10B4E0[index.Word] != 0 ? 1 : 0);
                        index.X--;
                        point1 += (byte)(mapTerrainType_10B4E0[index.Word] != 0 ? 1 : 0);
                        index.Y--;
                        point1 += (byte)(mapTerrainType_10B4E0[index.Word] != 0 ? 1 : 0);
                        index.X++;
                        point1 += (byte)(mapTerrainType_10B4E0[index.Word] != 0 ? 1 : 0);
                        index.Y++;
                        if (point1 == 0)
                            mapAngle_13B4E0[index.Word] |= 8;
                    }
                }
            }
        }

        private void sub_44D00_shade_terrain(byte[] mapHeightmap_11B4E0, byte[] mapShading_12B4E0, MapType mapType, ref ushort seed_17B4E0)//225d00
        {

            //     X
            //    /
            //   B
            //  /
            // X

            UAxis2D tempIndex = new UAxis2D();
            UAxis2D index = new UAxis2D();
            seed_17B4E0 = 0;
            for (int i = 0; i < MaxMapSize * MaxMapSize; i++)
            {
                index.Word = (ushort)i;
                index.X++;
                index.Y++;
                tempIndex.Word = index.Word;
                index.X -= 2;
                index.Y -= 2;
                tempIndex.X = (byte)(mapHeightmap_11B4E0[index.Word] - mapHeightmap_11B4E0[tempIndex.Word] + 32);
                index.X++;
                index.Y++;
                if (tempIndex.X == 32)
                {
                    tempIndex.Word = (ushort)(9377 * seed_17B4E0 + 9439);
                    seed_17B4E0 = tempIndex.Word;
                    tempIndex.Y = (byte)((seed_17B4E0 / 9u) >> 8);
                    tempIndex.X = (byte)(seed_17B4E0 % 9 + 28);
                }
                else if ((sbyte)tempIndex.X >= 28)
                {
                    if ((sbyte)tempIndex.X > 40)
                        tempIndex.X = (byte)((tempIndex.X & 7) + 40);
                }
                else
                {
                    tempIndex.X = (byte)((tempIndex.X & 3) + 28);
                }
                if (mapType != MapType.Day)
                {
                    mapShading_12B4E0[index.Word] = (byte)(64 - tempIndex.X);
                }
                else
                {
                    mapShading_12B4E0[index.Word] = tempIndex.X;
                }
            }
        }

        public Task<Terrain> CalculateTerrainAsync(GenerationParameters genParams, byte stage = 18)
        {
            return Task.Run(() => {
                return CalculateTerrain(genParams, stage);
            });
        }
    }
}
