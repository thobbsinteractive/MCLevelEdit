using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Services;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Test
{
    public class TerrainTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(1, @"Resources\mapEntityIndex_decompress_15B4E0.bin", @"Resources\mapAngle_decompress_13B4E0.bin", @"Resources\mapHeightmap_decompress_11B4E0.bin", @"Resources\mapShading_decompress_12B4E0.bin", @"Resources\mapTerrainType_decompress_10B4E0.bin")]
        [TestCase(4, @"Resources\mapEntityIndex_river_15B4E0.bin", @"Resources\mapAngle_river_13B4E0.bin", @"Resources\mapHeightmap_river_11B4E0.bin", @"Resources\mapShading_river_12B4E0.bin", @"Resources\mapTerrainType_river_10B4E0.bin")]
        [TestCase(15, @"Resources\mapEntityIndex_river2_15B4E0.bin", @"Resources\mapAngle_river2_13B4E0.bin", @"Resources\mapHeightmap_river2_11B4E0.bin", @"Resources\mapShading_river2_12B4E0.bin", @"Resources\mapTerrainType_river2_10B4E0.bin")]
        [TestCase(16, @"Resources\mapEntityIndex_set_angle_15B4E0.bin", @"Resources\mapAngle_set_angle_13B4E0.bin", @"Resources\mapHeightmap_set_angle_11B4E0.bin", @"Resources\mapShading_set_angle_12B4E0.bin", @"Resources\mapTerrainType_set_angle_10B4E0.bin")]
        [TestCase(17, @"Resources\mapEntityIndex_shade_15B4E0.bin", @"Resources\mapAngle_shade_13B4E0.bin", @"Resources\mapHeightmap_shade_11B4E0.bin", @"Resources\mapShading_shade_12B4E0.bin", @"Resources\mapTerrainType_shade_10B4E0.bin")]
        [TestCase(18, @"Resources\mapEntityIndex_final_15B4E0.bin", @"Resources\mapAngle_final_13B4E0.bin", @"Resources\mapHeightmap_final_11B4E0.bin", @"Resources\mapShading_final_12B4E0.bin", @"Resources\mapTerrainType_final_10B4E0.bin")]
        public void LoadMapFromFile(byte stage, string mapEntityIndexPath, string mapAnglePath, string mapHeightmapPath, string mapShadingPath, string mapTerrainTypePath)
        {
            var service = new TerrainService();

            var mapEntityIndexFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapEntityIndexPath);
            var mapAngleFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapAnglePath);
            var mapHeightmapFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapHeightmapPath);
            var mapShadingFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapShadingPath);
            var mapTerrainTypeFullPath = Path.Combine(Directory.GetCurrentDirectory(), mapTerrainTypePath);

            var entityBytes = File.ReadAllBytes(mapEntityIndexFullPath);
            short[] mapEntitesShorts = new short[entityBytes.Length / 2];
            Buffer.BlockCopy(entityBytes, 0, mapEntitesShorts, 0, entityBytes.Length);

            var mapAngleBytes = File.ReadAllBytes(mapAngleFullPath);
            var mapHeightmapBytes = File.ReadAllBytes(mapHeightmapFullPath);
            var mapShadingBytes = File.ReadAllBytes(mapShadingFullPath);
            var mapTerrainTypeBytes = File.ReadAllBytes(mapTerrainTypeFullPath);


            var terrain = service.CalculateMc2Terrain(new GenerationParameters()
            {
                MapType = MapType.Night,
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
            }, stage).Result;

            for (int i = 0; i < 65536; i++)
            {
                Assert.That(terrain.MapEntityIndex_15B4E0[i], Is.EqualTo(mapEntitesShorts[i]));
            }

            for (int i = 0; i < 65536; i++)
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
