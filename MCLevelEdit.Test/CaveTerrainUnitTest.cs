using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.Test
{
    public class CaveTerrainTests
    {
        // A Magic Carpet 2 Cave level (map type 2, basic height 114)
        private const string CAVE_LEVEL = "mc2_00169b.dat";

        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        private static Mc2Map LoadCave() => (Mc2Map)new FileAdapter().LoadMap(ResourcePath(CAVE_LEVEL));

        [Test]
        public void CaveLevelBasicHeightIsATerrainParameter()
        {
            var map = LoadCave();

            Assert.That(map.Terrain.GenerationParameters.MapType, Is.EqualTo(MapType.Cave));
            Assert.That(map.Terrain.GenerationParameters.BasicHeight, Is.EqualTo(114));
            Assert.That(map.BasicHeight, Is.EqualTo(114));
        }

        [Test]
        public void CaveTerrainHasCeilingAndRockMask()
        {
            var gp = LoadCave().Terrain.GenerationParameters;
            var terrain = TerrainGenerator.Create(gp.MapType).CalculateTerrain(gp);

            Assert.That(terrain.MapCeilingHeightmap_14B4E0, Is.Not.Null);

            int rock = 0;
            for (int i = 0; i < terrain.MapHeightmap_11B4E0.Length; i++)
            {
                bool masked = (terrain.MapAngle_13B4E0[i] & 8) != 0;
                if (masked)
                {
                    rock++;
                    // Solid rock: the ceiling is put just below the floor
                    Assert.That(terrain.MapCeilingHeightmap_14B4E0[i], Is.EqualTo(unchecked((byte)(terrain.MapHeightmap_11B4E0[i] - 1))));
                }
                else
                {
                    // Open cave: the ceiling clears the floor
                    Assert.That(terrain.MapCeilingHeightmap_14B4E0[i], Is.GreaterThan(terrain.MapHeightmap_11B4E0[i]));
                }
            }

            // A real cave has both open space and rock
            Assert.That(rock, Is.GreaterThan(0));
            Assert.That(rock, Is.LessThan(terrain.MapHeightmap_11B4E0.Length));
        }

        [Test]
        public void CaveWithNoRoomUnderTheRoofIsRockAboveHeightZero()
        {
            var gp = LoadCave().Terrain.GenerationParameters;
            gp.BasicHeight = 0;
            var terrain = TerrainGenerator.Create(gp.MapType).CalculateTerrain(gp);

            for (int i = 0; i < terrain.MapHeightmap_11B4E0.Length; i++)
            {
                // As in the game, a rock square's ceiling is a byte set to floor - 1, so at floor 0 it wraps to 255
                // and the square reads as open. Everything higher is rock.
                bool masked = (terrain.MapAngle_13B4E0[i] & 8) != 0;
                Assert.That(masked, Is.EqualTo(terrain.MapHeightmap_11B4E0[i] > 0), $"square {i}");
            }
        }

        [TestCase(MapType.Day, MapType.Night)]
        [TestCase(MapType.Day, MapType.Cave)]
        [TestCase(MapType.Night, MapType.Cave)]
        public void ConvertingMapTypeKeepsTheLandscape(MapType from, MapType to)
        {
            // Converting a level regenerates it with the same variables: only the map type changes
            var gp = LoadCave().Terrain.GenerationParameters;
            gp.MapType = from;
            var before = TerrainGenerator.Create(from).CalculateTerrain(gp);
            gp.MapType = to;
            var after = TerrainGenerator.Create(to).CalculateTerrain(gp);

            Assert.That(after.MapHeightmap_11B4E0, Is.EqualTo(before.MapHeightmap_11B4E0), "heights");
            Assert.That(after.MapTerrainType_10B4E0, Is.EqualTo(before.MapTerrainType_10B4E0), "terrain types");
            // Only the angle marker bit (water edges / cave rock) may differ
            Assert.That(after.MapAngle_13B4E0.Select(a => a & ~8), Is.EqualTo(before.MapAngle_13B4E0.Select(a => a & ~8)), "angles");
        }

        [TestCase(MapType.Day, typeof(TerrainGenerator))]
        [TestCase(MapType.Night, typeof(TerrainGenerator))]
        [TestCase(MapType.Cave, typeof(CaveTerrainGenerator))]
        public void GeneratorIsChosenByMapType(MapType mapType, Type expected)
        {
            Assert.That(TerrainGenerator.Create(mapType), Is.TypeOf(expected));
        }

        [TestCase(MapType.Day)]
        [TestCase(MapType.Night)]
        public void NonCaveTerrainHasNoCeiling(MapType mapType)
        {
            var gp = LoadCave().Terrain.GenerationParameters;
            gp.MapType = mapType;

            Assert.That(TerrainGenerator.Create(gp.MapType).CalculateTerrain(gp).MapCeilingHeightmap_14B4E0, Is.Null);
        }
    }
}
