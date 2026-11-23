using MagicCarpet2Terrain;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Test
{
    public class TerrainFeatureTests
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

        private static Mc2TerrainFeatureBuilder Mc2Builder() =>
            new Mc2TerrainFeatureBuilder(Shapes("mc2"), BuildingParams.Load(Asset("building-params-mc2.dat")));

        private static Mc1TerrainFeatureBuilder Mc1Builder() => new Mc1TerrainFeatureBuilder(Shapes("mc1"));

        private static List<LevelFeature> Features(Map map) => TerrainService.GetLevelFeatures(map);

        /// <summary>The buildings the level builds when it loads.</summary>
        private static List<LevelFeature> Buildings(Map map) =>
            Features(map).Where(f => f.Model == TerrainFeatureBuilder.BUILDING && f.DisId == LevelFeature.NO_SWITCH).ToList();

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
        public void Mc2FeaturesMatchTheGame()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var game = File.ReadAllBytes(ResourcePath("mc2_00073f_afterload.bin"));
            byte[] Part(int index) => game.Skip(index * 0x10000).Take(0x10000).ToArray();
            var generated = Generate(map);
            Assert.That(Buildings(map), Has.Count.EqualTo(22));
            Assert.That(generated.MapHeightmap_11B4E0, Is.Not.EqualTo(Part(1)), "the buildings change the terrain");

            var built = Mc2Builder().Build(generated, Features(map));

            Assert.That(built.MapTerrainType_10B4E0, Is.EqualTo(Part(0)), "terrain types");
            Assert.That(built.MapHeightmap_11B4E0, Is.EqualTo(Part(1)), "heights");
            Assert.That(built.MapShading_12B4E0, Is.EqualTo(Part(2)), "shading");
            Assert.That(built.MapAngle_13B4E0, Is.EqualTo(Part(3)), "angles");
        }

        /// <summary>
        /// Levels 3 (a ring of walls) and 7 (30 path nodes): every square the walls and paths change is as the game has
        /// it after its first turn, except where a wizard's castle (Special 1) grows on that turn.
        /// </summary>
        [TestCase("mc2_00109e", TerrainFeatureBuilder.WALL, 600)]
        [TestCase("mc2_0041e2", TerrainFeatureBuilder.PATH, 600)]
        public void Mc2WallsAndPathsMatchTheGame(string level, int model, int minChanged)
        {
            var map = new FileAdapter().LoadMap(ResourcePath(level + ".dat"));
            var game = File.ReadAllBytes(ResourcePath(level + "_afterload.bin"));
            byte[] Part(int index) => game.Skip(index * 0x10000).Take(0x10000).ToArray();
            var types = Part(0);
            var heights = Part(1);
            var generated = Generate(map);
            var features = Features(map).Where(f => f.Model != TerrainFeatureBuilder.BUILDING).ToList();
            Assert.That(features.Count(f => f.Model == model && f.DisId == LevelFeature.NO_SWITCH), Is.GreaterThan(1));
            var castles = map.Entities.Where(e => e.EntityType.TypeId == TypeId.Special && e.EntityType.Model.Id == 1).ToList();
            bool AtCastle(int x, int y) => castles.Any(c => Math.Abs(x - c.Position.X) <= 15 && Math.Abs(y - c.Position.Y) <= 15);

            var built = Mc2Builder().Build(generated, features);

            int changed = 0;
            for (int i = 0; i < 0x10000; i++)
            {
                if (built.MapHeightmap_11B4E0[i] == generated.MapHeightmap_11B4E0[i] && built.MapTerrainType_10B4E0[i] == generated.MapTerrainType_10B4E0[i])
                    continue;
                changed++;
                if (AtCastle(i & 0xFF, i >> 8))
                    continue;
                Assert.That(built.MapHeightmap_11B4E0[i], Is.EqualTo(heights[i]), $"height of ({i & 0xFF},{i >> 8})");
                Assert.That(built.MapTerrainType_10B4E0[i], Is.EqualTo(types[i]), $"type of ({i & 0xFF},{i >> 8})");
            }
            Assert.That(changed, Is.GreaterThan(minChanged));
        }

        private static LevelFeature Node(int id, int model, int x, int y, int parent, int child, ushort switchId = 1, ushort disId = LevelFeature.NO_SWITCH) =>
            new LevelFeature { Id = id, Model = model, X = x, Y = y, Parent = (ushort)parent, Child = (ushort)child, SwitchId = switchId, DisId = disId };

        private static Terrain Level2Terrain() => Generate(new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat")));

        [Test]
        public void WallRaisesItsLineBy48AsWall()
        {
            var generated = Level2Terrain();
            var built = Mc2Builder().Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.WALL, 40, 100, 0, 2),
                Node(2, TerrainFeatureBuilder.WALL, 60, 100, 1, 0),
            });

            // along x from 40 (41 on the odd squares) to 60, 2 squares thick
            for (int x = 42; x < 60; x++)
                for (int y = 100; y < 102; y++)
                {
                    int i = x | (y << 8);
                    Assert.That(built.MapTerrainType_10B4E0[i], Is.EqualTo(8), $"({x},{y}) is wall");
                    Assert.That(built.MapHeightmap_11B4E0[i], Is.EqualTo((byte)(generated.MapHeightmap_11B4E0[i] + 48)), $"({x},{y}) is raised");
                }
            Assert.That(built.MapHeightmap_11B4E0[50 | (120 << 8)], Is.EqualTo(generated.MapHeightmap_11B4E0[50 | (120 << 8)]));
        }

        [Test]
        public void PathMarksADiagonalThenAStraightRun()
        {
            var generated = Level2Terrain();
            var built = Mc2Builder().Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.PATH, 40, 100, 0, 2),
                Node(2, TerrainFeatureBuilder.PATH, 50, 104, 1, 0),
            });

            // 4 squares diagonally from (40,100), then 6 along x from (44,104): the corner mark 1, the heights as they were
            var squares = Enumerable.Range(0, 4).Select(i => (40 + i, 100 + i)).Concat(Enumerable.Range(0, 6).Select(i => (44 + i, 104)));
            foreach (var (x, y) in squares)
                Assert.That(built.MapAngle_13B4E0[x | (y << 8)] & 7, Is.EqualTo(1), $"({x},{y}) is path");
            Assert.That(built.MapHeightmap_11B4E0, Is.EqualTo(generated.MapHeightmap_11B4E0));
        }

        [Test]
        public void LinesAreOnlyMadeWithAStageTagAndNoSwitch()
        {
            var generated = Level2Terrain();
            var builder = Mc2Builder();

            var noTag = builder.Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.WALL, 40, 100, 0, 2, switchId: 0),
                Node(2, TerrainFeatureBuilder.WALL, 60, 100, 1, 0, switchId: 0),
            });
            var switched = builder.Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.WALL, 40, 100, 0, 2, disId: 4),
                Node(2, TerrainFeatureBuilder.WALL, 60, 100, 1, 0, disId: 4),
            });
            var tagOnTheLastNode = builder.Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.WALL, 40, 100, 0, 2, switchId: 0),
                Node(2, TerrainFeatureBuilder.WALL, 60, 100, 1, 0),
            });

            Assert.That(noTag.MapHeightmap_11B4E0, Is.EqualTo(generated.MapHeightmap_11B4E0));
            Assert.That(switched.MapHeightmap_11B4E0, Is.EqualTo(generated.MapHeightmap_11B4E0));
            // the line is made from its first node whichever node has the tag
            Assert.That(tagOnTheLastNode.MapTerrainType_10B4E0[50 | (100 << 8)], Is.EqualTo(8));
        }

        [Test]
        public void ARingOfLinksIsMadeOnce()
        {
            var generated = Level2Terrain();
            var ring = new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.WALL, 40, 100, 3, 2),
                Node(2, TerrainFeatureBuilder.WALL, 60, 100, 1, 3),
                Node(3, TerrainFeatureBuilder.WALL, 60, 120, 2, 1),
            };

            var built = Mc2Builder().Build(generated, ring);

            Assert.That(built.MapTerrainType_10B4E0[50 | (100 << 8)], Is.EqualTo(8));
        }

        [Test]
        public void BuildingLeavesTheGeneratedTerrainAsItIs()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var generated = Generate(map);
            var heights = (byte[])generated.MapHeightmap_11B4E0.Clone();

            var built = Mc2Builder().Build(generated, Buildings(map));

            Assert.That(built, Is.Not.SameAs(generated));
            Assert.That(generated.MapHeightmap_11B4E0, Is.EqualTo(heights));
        }

        [Test]
        public void NoBuildingsLeaveTheTerrain()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var generated = Generate(map);
            Assert.That(Mc2Builder().Build(generated, new List<LevelFeature>()), Is.SameAs(generated));
        }

        [Test]
        public void OnlyBuildingsWithoutASwitchAreBuiltWhenTheLevelLoads()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00073f.dat"));
            var building = map.Entities.First(e => e.IsBuilding() && e.DisId == ushort.MaxValue);
            int before = Buildings(map).Count;

            building.DisId = 3;

            Assert.That(Buildings(map), Has.Count.EqualTo(before - 1));
        }

        /// <summary>A Magic Carpet building flattens its square to the base height and changes nothing far from it.</summary>
        [Test]
        public void Mc1BuildingsChangeTheTerrainAroundThem()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("data_1c0000.dat"));
            Assert.That(map.TerrainGame, Is.EqualTo(TerrainGame.MagicCarpet1));
            var generated = Generate(map);
            var placements = Buildings(map);
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
                    var shape = shapes[p.Parent + Mc1TerrainFeatureBuilder.SHAPE_OFFSET]!;
                    return Math.Abs(x - p.X) <= shape.Width / 2 + 7 && Math.Abs(y - p.Y) <= shape.Height / 2 + 7;
                });
                Assert.That(near, Is.True, $"square ({x},{y}) changed far from any building");
            }
            Assert.That(changed, Is.GreaterThan(0));
        }
    }
}
