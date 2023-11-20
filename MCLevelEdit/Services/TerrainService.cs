using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Avalonia;
using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using MCLevelEdit.Utils;
using Splat;
using System;
using System.Threading.Tasks;
using static MCLevelEdit.Interfaces.ITerrainService;

namespace MCLevelEdit.Services
{
    public class TerrainService : ITerrainService, IEnableLogger
    {
        public Color WATER_COLOUR = new Color(255, 46, 78, 166);
        public Color COAST_COLOUR = new Color(255, 136, 99, 66);
        public Color SAND_COLOUR = new Color(255, 186, 150, 101);
        public Color GRASS_COLOUR = new Color(255, 117, 105, 40);

        public Task<WriteableBitmap> GenerateBitmapAsync(Terrain terrain, Layer layer)
        {
            return Task.Run(() =>
            {
                WriteableBitmap bitmap = new WriteableBitmap(
                    new PixelSize(Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE),
                    new Vector(96, 96), // DPI (dots per inch)
                    PixelFormat.Rgba8888);

                if (layer == Layer.Game)
                    BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(255, 0, 0, 0), bitmap);
                if (layer == Layer.Height)
                    BitmapUtils.SetBackground(new Rect(0, 0, Globals.MAX_MAP_SIZE, Globals.MAX_MAP_SIZE), new Color(255, 46, 78, 166), bitmap);

                return DrawBitmapAsync(terrain, layer, bitmap);
            });
        }

        public Task<WriteableBitmap> DrawBitmapAsync(Terrain terrain, Layer layer, WriteableBitmap bitmap)
        {
            return Task.Run(() =>
            {
                using (var fb = bitmap.Lock())
                {
                    for (int y = 0; y < Globals.MAX_MAP_SIZE; y++)
                    {
                        for (int x = 0; x < Globals.MAX_MAP_SIZE; x++)
                        {
                            int index = (y * Globals.MAX_MAP_SIZE) + x;

                            if (layer == Layer.Game)
                            {
                                Color baseColour = SAND_COLOUR;
                                switch (terrain.MapTerrainType_10B4E0[index])
                                {
                                    case 0:
                                        baseColour = WATER_COLOUR;
                                        break;
                                    case 3:
                                        baseColour = GRASS_COLOUR;
                                        break;
                                    case 4:
                                        baseColour = COAST_COLOUR;
                                        break;
                                    default:
                                        baseColour = SAND_COLOUR;
                                        break;
                                }

                                fb.SetPixel(x, y, new Color(255, (byte)Math.Max(baseColour.R - terrain.MapShading_12B4E0[index], byte.MinValue),
                                    (byte)Math.Max(baseColour.G - terrain.MapShading_12B4E0[index], byte.MinValue),
                                    (byte)Math.Max(baseColour.B - terrain.MapShading_12B4E0[index], byte.MinValue)));
                            }
                            if (layer == Layer.Height)
                                fb.SetPixel(x, y, new Color(255, terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index], terrain.MapHeightmap_11B4E0[index]));
                        }
                    }
                }
                //BitmapUtils.SaveBitmap(bitmap);

                return bitmap;
            });
        }

        public async Task<Terrain> CalculateTerrain(TerrainGenerationParameters genParams)
        {
            short[] mapEntityIndex_15B4E0 = new short[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];
            byte[] mapHeightmap_11B4E0 = new byte[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];
            byte[] mapAngle_13B4E0 = new byte[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];
            byte[] mapTerrainType_10B4E0 = new byte[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];
            byte[] mapShading_12B4E0 = new byte[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];

            ushort seed_17B4E0 = genParams.Seed;
            sub_B5E70_decompress_terrain_map_level(mapEntityIndex_15B4E0, (short)genParams.Seed, genParams.Offset, genParams.Raise, genParams.Gnarl);
            sub_44DB0_truncTerrainHeight(mapEntityIndex_15B4E0, mapHeightmap_11B4E0);//225db0 //trunc and create
            sub_44E40_generate_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.River, genParams.LRiver, ref seed_17B4E0);//225e40 //add any fields
            sub_45AA0_setMax4Tiles(mapHeightmap_11B4E0, mapAngle_13B4E0);
            sub_440D0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.SnLin);//2250d0
            sub_45060(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226060
            sub_44320(mapAngle_13B4E0);//225320
            sub_45210(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226210
            sub_454F0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.Source, genParams.RkSte);//2264f0
            sub_45600(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.BhFlt);//226600
            sub_43FC0(mapAngle_13B4E0);//224fc0

            //Array.Fill<byte>(mapTerrainType_10B4E0, 0);

            sub_43970_smooth_terrain(mapHeightmap_11B4E0, mapAngle_13B4E0);//224970 // smooth terrain
            sub_43EE0_add_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0);//224ee0 // add rivers
            sub_43D50_change_angle_of_terrain(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0);//224d50
            sub_44D00_shade_terrain(mapHeightmap_11B4E0, mapShading_12B4E0, ref seed_17B4E0);//225d00

            return new Terrain()
            {
                MapTerrainType_10B4E0 = mapTerrainType_10B4E0,
                MapHeightmap_11B4E0 = mapHeightmap_11B4E0,
                MapAngle_13B4E0 = mapAngle_13B4E0,
                MapShading_12B4E0 = mapShading_12B4E0
            };
        }

        private void sub_B5E70_decompress_terrain_map_level(short[] mapEntityIndex_15B4E0, short seed, ushort offset, ushort raise, ushort gnarl)
        {
            UAxis2d sumEnt = new UAxis2d();

            mapEntityIndex_15B4E0[offset] = (short)raise;//32c4e0 //first seed
            for (short i = 7; i >= 0; i--)
            {
                sumEnt.Word = offset;
                for (int j = 1 << (7 - i); j > 0; j--)
                {
                    for (int k = 1 << (7 - i); k > 0; k--)
                    {
                        sub_B5EFA(mapEntityIndex_15B4E0, (short)(1 << i), ref sumEnt, gnarl, ref seed);//355220
                        //this.Log().Debug($"sub_B5EFA Seed:{seed} offset:{offset} raise:{raise} gnarl:{gnarl}");
                    }
                    sumEnt.Word += (ushort)((2 * (1 << i)) << 8);
                }
                for (int j = 1 << (7 - i); j > 0; j--)
                {
                    for (int k = 1 << (7 - i); k > 0; k--)
                    {
                        sub_B5F8F(mapEntityIndex_15B4E0, (short)(1 << i), ref sumEnt, gnarl, ref seed);
                        //this.Log().Debug($"sub_B5F8F Seed:{seed} offset:{offset} raise:{raise} gnarl:{gnarl}");
                    }
                    sumEnt.Word += (ushort)((2 * (1 << i)) << 8);
                }
            }
        }

        private void sub_B5EFA(short[] mapEntityIndex_15B4E0, short a1, ref UAxis2d indexx, ushort gnarl, ref short nextRand)//296EFA
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
            //if (mapEntityIndex_15B4E0[indexx.Word] <= 0)
                mapEntityIndex_15B4E0[indexx.Word] = (short)(srandNumber % (ushort)(2 * gnarl + 1)
                + srandNumber % (ushort)((a1 << 6) + 1) + (sumEnt >> 2) - 32 * a1 - gnarl);
            indexx.X += (byte)a1;
            indexx.Y -= (byte)a1;
        }

        //----- (000B5F8F) --------------------------------------------------------
        private void sub_B5F8F(short[] mapEntityIndex_15B4E0, short a1, ref UAxis2d indexx, ushort gnarl, ref short nextRand)//296f8f
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
            //if (mapEntityIndex_15B4E0[indexx.Word] <= 0)
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
            //if (mapEntityIndex_15B4E0[indexx.Word] <= 0)
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
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
            {
                if (mapEntityIndex_15B4E0[i] > maxEnt)
                    maxEnt = mapEntityIndex_15B4E0[i];
                if (mapEntityIndex_15B4E0[i] < minEnt)
                    minEnt = mapEntityIndex_15B4E0[i];
            }
            if (maxEnt > 0)
                revMaxEnt = 0xC40000 / maxEnt;
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d index = new UAxis2d();
            int locCount = riverCount;
            int i = 0;
            for (i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            for (i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
            {
                mapTerrainType_10B4E0[i] = 255;
            }
        }

        private void sub_44EE0_smooth_tiles(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0, UAxis2d axis)//225ee0
        {
            //  X-X-X
            //  |   |
            //  X B X
            //  | | |
            //  X X-X

            UAxis2d tempAxis2 = new UAxis2d();
            UAxis2d tempAxis1 = new UAxis2d();
            byte centralHeight;
            byte minHeight;

            tempAxis1.Word = axis.Word;
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            UAxis2d indexx = new UAxis2d();
            byte angleIndex;
            byte minHeight;
            byte maxHeight;
            bool runAgain;

            do
            {
                runAgain = false;
                for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d index = new UAxis2d();

            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d index = new UAxis2d();
            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE);
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE);
            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d index = new UAxis2d();
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

            Array.Copy(mapAngle_13B4E0, mapTerrainType_10B4E0, Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE);
            UAxis2d index = new UAxis2d();

            //    X
            //	 / \
            //  X-B X
            //    |/
            //    X

            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
                        if (ang2 > 0 || ang5 > 0  || ang4 > 0)
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

            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
            UAxis2d uindex = new UAxis2d();
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
            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

        void sub_43D50_change_angle_of_terrain(byte[] mapHeightmap_11B4E0, byte[] mapAngle_13B4E0, byte[] mapTerrainType_10B4E0)//224d50
        {

            //  X-X-X
            //  |   |
            //  X B X
            //  |/| |
            //  X X-X

            byte point1;
            byte point2;
            UAxis2d index = new UAxis2d();
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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

        private void sub_44D00_shade_terrain(byte[] mapHeightmap_11B4E0, byte[] mapShading_12B4E0, ref ushort seed_17B4E0)//225d00
        {

            //     X
            //    /
            //   B
            //  /
            // X

            UAxis2d tempIndex = new UAxis2d();
            UAxis2d index = new UAxis2d();
            seed_17B4E0 = 0;
            for (int i = 0; i < Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE; i++)
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
                mapShading_12B4E0[index.Word] = tempIndex.X;
            }
        }
    }
}
