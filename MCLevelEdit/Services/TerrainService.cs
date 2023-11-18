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
                                switch (terrain.MapTerrainType_10B4E0[index])
                                {
                                    case 0:
                                        fb.SetPixel(x, y, WATER_COLOUR);
                                        break;
                                    case 3:
                                        fb.SetPixel(x, y, GRASS_COLOUR);
                                        break;
                                    case 4:
                                        fb.SetPixel(x, y, COAST_COLOUR);
                                        break;
                                    default:
                                        fb.SetPixel(x, y, SAND_COLOUR);
                                        break;
                                }
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

            ushort seed_17B4E0 = genParams.Seed;
            sub_B5E70_decompress_terrain_map_level(mapEntityIndex_15B4E0, (short)genParams.Seed, genParams.Offset, genParams.Raise, genParams.Gnarl);
            sub_44DB0_truncTerrainHeight(mapEntityIndex_15B4E0, mapHeightmap_11B4E0);//225db0 //trunc and create
            sub_44E40_generate_rivers(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.River, genParams.LRiver, ref seed_17B4E0);//225e40 //add any fields
            sub_45AA0_setMax4Tiles(mapHeightmap_11B4E0, mapAngle_13B4E0);
            sub_440D0(mapHeightmap_11B4E0, mapAngle_13B4E0, genParams.SnLin);//2250d0
            sub_45060(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226060
            sub_44320(mapAngle_13B4E0);//225320
            sub_45210(mapHeightmap_11B4E0, mapAngle_13B4E0, mapTerrainType_10B4E0, genParams.SnFlt, genParams.BhLin);//226210

            return new Terrain()
            {
                MapTerrainType_10B4E0 = mapTerrainType_10B4E0,
                MapHeightmap_11B4E0 = mapHeightmap_11B4E0,
                MapAngle_13B4E0 = mapAngle_13B4E0,
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
    }
}
