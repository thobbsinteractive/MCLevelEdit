using MCLevelEdit.Application.Services;

namespace MCLevelEdit.Test
{
    public class TerrainColourTableTests
    {
        private static string AssetPath(string name)
        {
            // Walk up from the test output folder to the repository, where the editor's assets live
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "MCLevelEdit.sln")))
                dir = dir.Parent;
            Assert.That(dir, Is.Not.Null, "Could not find the repository root");
            return Path.Combine(dir!.FullName, "MCLevelEdit", "Assets", name);
        }

        private static TerrainColourTable Load(string name) => new TerrainColourTable(File.ReadAllBytes(AssetPath(name)));

        [TestCase("terrain-colours-day.dat")]
        [TestCase("terrain-colours-night.dat")]
        [TestCase("terrain-colours-cave.dat")]
        [TestCase("terrain-colours-mc1.dat")]
        [TestCase("terrain-colours-hidden-worlds.dat")]
        public void ColourTableLoads(string name)
        {
            Assert.That(new FileInfo(AssetPath(name)).Length, Is.EqualTo(TerrainColourTable.SIZE));
            Assert.DoesNotThrow(() => Load(name));
        }

        // Base colours recorded by hand from the in-game minimap that agree with the game's table
        [TestCase(4, 139)]
        [TestCase(6, 46)]
        [TestCase(8, 78)]
        [TestCase(9, 82)]
        [TestCase(10, 171)]
        public void DayBaseIndexMatchesRecordedMinimapColour(int terrainType, int expectedIndex)
        {
            Assert.That(Load("terrain-colours-day.dat").GetBaseIndex((byte)terrainType), Is.EqualTo(expectedIndex));
        }

        [Test]
        public void PalletIndexIsShadedBaseIndex()
        {
            var data = new byte[TerrainColourTable.SIZE];
            data[TerrainColourTable.TERRAIN_TYPE_TABLE_OFFSET + 7] = 100; // type 7 -> colour 100
            data[(40 << 8) | 100] = 123;                                  // colour 100 at shading 40 -> 123
            var table = new TerrainColourTable(data);

            Assert.That(table.GetBaseIndex(7), Is.EqualTo(100));
            Assert.That(table.GetPalletIndex(7, 40), Is.EqualTo(123));
        }

        [Test]
        public void WrongSizeIsRejected()
        {
            Assert.Throws<ArgumentException>(() => new TerrainColourTable(new byte[100]));
        }
    }
}
