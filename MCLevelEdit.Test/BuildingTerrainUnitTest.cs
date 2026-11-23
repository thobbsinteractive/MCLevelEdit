using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Test
{
    public class BuildingTerrainTests
    {
        private static string RepositoryPath(params string[] parts)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "MCLevelEdit.sln")))
                dir = dir.Parent;
            Assert.That(dir, Is.Not.Null, "Could not find the repository root");
            return Path.Combine(new[] { dir!.FullName }.Concat(parts).ToArray());
        }

        private static byte[] Asset(string name) => File.ReadAllBytes(RepositoryPath("MCLevelEdit", "Assets", name));

        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        private static BuildingShapes Shapes(string game) =>
            BuildingShapes.Load(Asset($"building-shapes-{game}.tab"), Asset($"building-shapes-{game}.dat"));

        private static Mc2BuildingTerrainBuilder Mc2Builder() =>
            new Mc2BuildingTerrainBuilder(Shapes("mc2"), BuildingParams.Load(Asset("building-params-mc2.dat")));

        private static Mc1BuildingTerrainBuilder Mc1Builder() => new Mc1BuildingTerrainBuilder(Shapes("mc1"));

        private static Terrain Generate(Map map)
        {
            var genParams = map.Terrain.GenerationParameters;
            return TerrainGenerator.Create(genParams.MapType, map.TerrainGame).CalculateTerrain(genParams);
        }

        [TestCase("mc2", 77)]
        [TestCase("mc1", 69)]
        [TestCase("hidden-worlds", 78)]
        public void BuildingShapesLoad(string game, int count)
        {
            var shapes = Shapes(game);
            Assert.That(shapes.Count, Is.EqualTo(count));
            Assert.That(shapes[0]!.IsEmpty, Is.True);
            Assert.That(shapes[count], Is.Null);
        }

        [Test]
        public void Mc2ShapesHaveTwoBytesASquare()
        {
            var shapes = Shapes("mc2");
            Assert.That(shapes[1]!.Width, Is.EqualTo(8));
            Assert.That(shapes[1]!.Height, Is.EqualTo(8));
            Assert.That(shapes[2]!.Offset, Is.EqualTo(shapes[1]!.Offset + 2 * 8 * 8));
        }

        /// <summary>
        /// The terrain of the second Magic Carpet 2 level (types, heights, shading and angles) as the game has it
        /// after its first turn (remc2 regression memory at 2DC4E0): the generated terrain with its 22 buildings.
        /// </summary>
        [Test]
        public void Mc2BuildingsMatchTheGame()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var game = File.ReadAllBytes(ResourcePath("mc2_00073f_afterload.bin"));
            byte[] Part(int index) => game.Skip(index * 0x10000).Take(0x10000).ToArray();
            var generated = Generate(map);
            var placements = TerrainService.GetBuildingPlacements(map);
            Assert.That(placements, Has.Count.EqualTo(22));
            Assert.That(generated.MapHeightmap_11B4E0, Is.Not.EqualTo(Part(1)), "the buildings change the terrain");

            var built = Mc2Builder().Build(generated, placements);

            Assert.That(built.MapTerrainType_10B4E0, Is.EqualTo(Part(0)), "terrain types");
            Assert.That(built.MapHeightmap_11B4E0, Is.EqualTo(Part(1)), "heights");
            Assert.That(built.MapShading_12B4E0, Is.EqualTo(Part(2)), "shading");
            Assert.That(built.MapAngle_13B4E0, Is.EqualTo(Part(3)), "angles");
        }

        [Test]
        public void BuildingLeavesTheGeneratedTerrainAsItIs()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var generated = Generate(map);
            var heights = (byte[])generated.MapHeightmap_11B4E0.Clone();

            var built = Mc2Builder().Build(generated, TerrainService.GetBuildingPlacements(map));

            Assert.That(built, Is.Not.SameAs(generated));
            Assert.That(generated.MapHeightmap_11B4E0, Is.EqualTo(heights));
        }

        [Test]
        public void NoBuildingsLeaveTheTerrain()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var generated = Generate(map);
            Assert.That(Mc2Builder().Build(generated, new List<BuildingPlacement>()), Is.SameAs(generated));
        }

        [Test]
        public void OnlyBuildingsWithoutASwitchAreBuiltWhenTheLevelLoads()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var building = map.Entities.First(e => e.IsBuilding() && e.DisId == ushort.MaxValue);
            int before = TerrainService.GetBuildingPlacements(map).Count;

            building.DisId = 3;

            Assert.That(TerrainService.GetBuildingPlacements(map), Has.Count.EqualTo(before - 1));
        }

        /// <summary>A Magic Carpet building flattens its square to the base height and changes nothing far from it.</summary>
        [Test]
        public void Mc1BuildingsChangeTheTerrainAroundThem()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("data_1c0000.dat"));
            Assert.That(map.TerrainGame, Is.EqualTo(TerrainGame.MagicCarpet1));
            var generated = Generate(map);
            var placements = TerrainService.GetBuildingPlacements(map);
            Assert.That(placements, Has.Count.EqualTo(5));

            var built = Mc1Builder().Build(generated, placements);

            var shapes = Shapes("mc1");
            int changed = 0;
            for (int i = 0; i < 0x10000; i++)
            {
                if (built.MapHeightmap_11B4E0[i] == generated.MapHeightmap_11B4E0[i] && built.MapTerrainType_10B4E0[i] == generated.MapTerrainType_10B4E0[i])
                    continue;
                changed++;
                int x = i & 0xFF;
                int y = i >> 8;
                // within the shape, the smoothed band of 5 around it and the squares whose types follow
                bool near = placements.Any(p =>
                {
                    var shape = shapes[p.Shape + Mc1BuildingTerrainBuilder.SHAPE_OFFSET]!;
                    return Math.Abs(x - p.X) <= shape.Width / 2 + 7 && Math.Abs(y - p.Y) <= shape.Height / 2 + 7;
                });
                Assert.That(near, Is.True, $"square ({x},{y}) changed far from any building");
            }
            Assert.That(changed, Is.GreaterThan(0));
        }
    }
}
