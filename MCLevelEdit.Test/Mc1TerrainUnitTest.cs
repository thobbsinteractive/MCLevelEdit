using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;
using Mc2Map = MCLevelEdit.Model.Domain.MC2.Mc2Map;

namespace MCLevelEdit.Test
{
    public class Mc1TerrainTests
    {
        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        [Test]
        public void Mc1TerrainFieldsAreReadFromTheirOwnOffsets()
        {
            // remc1 Type_Mc1Level_2F503: river 0x14, lriver 0x18, source 0x1C, snLin 0x20, snFlt 0x24, bhLin 0x28, bhFlt 0x2C
            var bytes = File.ReadAllBytes(ResourcePath("data_014eb1.dat"));
            ushort[] values = { 11, 22, 33, 44, 55, 66, 77 };
            for (int i = 0; i < values.Length; i++)
                BitConverter.GetBytes(values[i]).CopyTo(bytes, 0x14 + i * 4);

            var path = Path.Combine(Path.GetTempPath(), $"MCLevelEdit.Test.{Guid.NewGuid():N}.dat");
            try
            {
                File.WriteAllBytes(path, bytes);
                var gp = new FileAdapter().LoadMap(path).Terrain.GenerationParameters;

                Assert.That(new[] { gp.River, gp.LRiver, gp.Source, gp.SnLin, gp.SnFlt, gp.BhLin, gp.BhFlt },
                    Is.EqualTo(new[] { 11, 22, 33, 44, 55, 66, 77 }));
                Assert.That(gp.RkSte, Is.EqualTo(0), "MC1 levels have no RkSte");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Mc1TerrainFieldsSaveBackToTheirOwnOffsets()
        {
            var original = File.ReadAllBytes(ResourcePath("data_014eb1.dat"));
            var map = new FileAdapter().LoadMap(ResourcePath("data_014eb1.dat"));
            var path = Path.Combine(Path.GetTempPath(), $"MCLevelEdit.Test.{Guid.NewGuid():N}.dat");
            try
            {
                new FileAdapter().SaveMap(map, path);
                var saved = File.ReadAllBytes(path);
                // The terrain fields, 0x04 to 0x2E, come back unchanged
                Assert.That(saved.Skip(4).Take(0x2A), Is.EqualTo(original.Skip(4).Take(0x2A)));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestCase(TerrainGame.MagicCarpet1, MapType.Day, typeof(Mc1TerrainGenerator))]
        [TestCase(TerrainGame.MagicCarpet1, MapType.Cave, typeof(Mc1TerrainGenerator))]
        [TestCase(TerrainGame.HiddenWorlds, MapType.Day, typeof(HiddenWorldsTerrainGenerator))]
        [TestCase(TerrainGame.MagicCarpet2, MapType.Day, typeof(TerrainGenerator))]
        [TestCase(TerrainGame.MagicCarpet2, MapType.Cave, typeof(CaveTerrainGenerator))]
        public void GeneratorIsChosenByGameAndMapType(TerrainGame game, MapType mapType, Type expected)
        {
            Assert.That(TerrainGenerator.Create(mapType, game), Is.TypeOf(expected));
        }

        [Test]
        public void EachLevelKnowsItsTerrainGame()
        {
            var map = new Map();
            Assert.That(map.TerrainGame, Is.EqualTo(TerrainGame.MagicCarpet1));
            map.SetHiddenWorlds(true);
            Assert.That(map.TerrainGame, Is.EqualTo(TerrainGame.HiddenWorlds));
            Assert.That(new Mc2Map().TerrainGame, Is.EqualTo(TerrainGame.MagicCarpet2));
        }

        [Test]
        public void Mc1AndHiddenWorldsShareTheLandscapeButNotTheMarks()
        {
            var gp = new FileAdapter().LoadMap(ResourcePath("data_014eb1.dat")).Terrain.GenerationParameters;
            var mc1 = TerrainGenerator.Create(MapType.Day, TerrainGame.MagicCarpet1).CalculateTerrain(gp);
            var hw = TerrainGenerator.Create(MapType.Day, TerrainGame.HiddenWorlds).CalculateTerrain(gp);
            var mc2 = TerrainGenerator.Create(MapType.Day).CalculateTerrain(gp);

            // The heights come from the same passes before the terrain classes are marked
            Assert.That(hw.MapHeightmap_11B4E0, Is.EqualTo(mc1.MapHeightmap_11B4E0));
            // Hidden Worlds marks steep terrain 1 instead of 6, so the terrain types differ
            Assert.That(hw.MapTerrainType_10B4E0, Is.Not.EqualTo(mc1.MapTerrainType_10B4E0));
            // MC1 has its own passes, so it differs from MC2's generator
            Assert.That(mc1.MapTerrainType_10B4E0, Is.Not.EqualTo(mc2.MapTerrainType_10B4E0));
        }

        [Test]
        public void Mc1GenerationIsRepeatable()
        {
            var gp = new FileAdapter().LoadMap(ResourcePath("data_014eb1.dat")).Terrain.GenerationParameters;
            var a = new Mc1TerrainGenerator().CalculateTerrain(gp);
            var b = new Mc1TerrainGenerator().CalculateTerrain(gp);

            Assert.That(b.MapHeightmap_11B4E0, Is.EqualTo(a.MapHeightmap_11B4E0));
            Assert.That(b.MapTerrainType_10B4E0, Is.EqualTo(a.MapTerrainType_10B4E0));
            Assert.That(b.MapAngle_13B4E0, Is.EqualTo(a.MapAngle_13B4E0));
            Assert.That(b.MapShading_12B4E0, Is.EqualTo(a.MapShading_12B4E0));
        }
    }
}
