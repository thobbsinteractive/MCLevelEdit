using MCLevelEdit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCLevelEdit.Test
{
    public class TerrainTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(@"Resources\mapAngle_13B4E0.bin", @"Resources\mapHeightmap_11B4E0.bin", @"Resources\mapShading_12B4E0.bin", @"Resources\mapShading_12B4E0.bin")]
        public void LoadMapFromFile(string mapAnglePath, 
            string mapHeightmapPath,
            string mapShadingPath,
            string mapTerrainTypePath)
        {
            var service = new TerrainService();

            var mapAngleFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapAnglePath);
            var mapHeightmapFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapHeightmapPath);
            var mapShadingFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapShadingPath);
            var mapTerrainTypeFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapTerrainTypePath);

            var mapAngleBytes = File.ReadAllBytes(mapAngleFullPath);
            var mapHeightmapBytes = File.ReadAllBytes(mapHeightmapFullPath);
            var mapShadingBytes = File.ReadAllBytes(mapShadingFullPath);
            var mapTerrainTypeBytes = File.ReadAllBytes(mapTerrainTypeFullPath);


            var terrain = service.CalculateTerrain(new DataModel.TerrainGenerationParameters()
            {
                Seed = 49098,
                Offset = 48953,
                Raise = 1140,
                Gnarl = 98,
                River = 49,
                LRiver = 84,
                Source = 136,
                SnLin = 13,
                SnFlt = 97,
                BhLin = 31,
                BhFlt = 35,
                RkSte = 10
            }).Result;

            for(int i = 0; i < 65536; i++)
            {
                Assert.That(terrain.MapAngle_13B4E0[i], Is.EqualTo(mapAngleBytes[i]));
            }

            for (int i = 0; i < 65536; i++)
            {
                Assert.That(terrain.MapHeightmap_11B4E0[i], Is.EqualTo(mapHeightmapBytes[i]));
            }

            for (int i = 0; i < 65536; i++)
            {
                Assert.That(terrain.MapShading_12B4E0[i], Is.EqualTo(mapShadingBytes[i]));
            }

            for (int i = 0; i < 65536; i++)
            {
                Assert.That(terrain.MapTerrainType_10B4E0[i], Is.EqualTo(mapTerrainTypeBytes[i]));
            }
        }
    }
}
