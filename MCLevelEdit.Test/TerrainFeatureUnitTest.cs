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
            new Mc2TerrainFeatureBuilder(Shapes("mc2"), BuildingParams.Load(Asset("building-params-mc2.dat")), Search());

        private static Mc1TerrainFeatureBuilder Mc1Builder() => new Mc1TerrainFeatureBuilder(Shapes("mc1"), Search());

        private static SearchRings Search() => SearchRings.Load(Asset("search.dat"));

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
        /// Whole levels as the game has them after its first turn (remc2 regression memory at 2DC4E0: types, heights,
        /// shading, angles, and in caves the ceiling): level 2 (22 buildings), level 3 (a ring of walls closed by a gate)
        /// and the cave level 15 (rooms, tunnels, mounds, craters, canyons, doors and buildings; its shading differs on a
        /// few squares, from what happens in that first turn).
        /// </summary>
        [TestCase("mc2_00073f", true)]
        [TestCase("mc2_00109e", true)]
        [TestCase("mc2_009c1e", false)]
        public void Mc2FeaturesMatchTheGame(string level, bool shading)
        {
            var map = new FileAdapter().LoadMap(ResourcePath(level + ".dat"));
            var game = File.ReadAllBytes(ResourcePath(level + "_afterload.bin"));
            byte[] Part(int index) => game.Skip(index * 0x10000).Take(0x10000).ToArray();
            var generated = Generate(map);
            Assert.That(generated.MapHeightmap_11B4E0, Is.Not.EqualTo(Part(1)), "the features change the terrain");

            var built = Mc2Builder().Build(generated, Features(map));

            Assert.That(built.MapTerrainType_10B4E0, Is.EqualTo(Part(0)), "terrain types");
            Assert.That(built.MapHeightmap_11B4E0, Is.EqualTo(Part(1)), "heights");
            if (shading)
                Assert.That(built.MapShading_12B4E0, Is.EqualTo(Part(2)), "shading");
            Assert.That(built.MapAngle_13B4E0, Is.EqualTo(Part(3)), "angles");
            if (built.MapCeilingHeightmap_14B4E0 != null)
                Assert.That(built.MapCeilingHeightmap_14B4E0, Is.EqualTo(Part(4)), "ceiling");
        }

        private static int WrappedDistance(int a, int b)
        {
            int d = ((a - b) % 256 + 256) % 256;
            return Math.Min(d, 256 - d);
        }

        /// <summary>
        /// Levels 3 (a ring of walls), 7 (30 path nodes) and 9 (40 ridge nodes): every square the walls, paths and ridges
        /// change is as the game has it after its first turn, except where a wizard's castle grows on that turn (at a
        /// Special 1, a spawn point, or the map's corner for a player with none).
        /// </summary>
        [TestCase("mc2_00109e", TerrainFeatureBuilder.WALL, 15, 600)]
        [TestCase("mc2_0041e2", TerrainFeatureBuilder.PATH, 15, 600)]
        [TestCase("mc2_00590d", TerrainFeatureBuilder.RIDGE, 40, 5000)]
        public void Mc2WallsPathsAndRidgesMatchTheGame(string level, int model, int castleSize, int minChanged)
        {
            var map = new FileAdapter().LoadMap(ResourcePath(level + ".dat"));
            var game = File.ReadAllBytes(ResourcePath(level + "_afterload.bin"));
            byte[] Part(int index) => game.Skip(index * 0x10000).Take(0x10000).ToArray();
            var types = Part(0);
            var heights = Part(1);
            var generated = Generate(map);
            var features = Features(map).Where(f => f.Model != TerrainFeatureBuilder.BUILDING).ToList();
            Assert.That(features.Count(f => f.Model == model && f.DisId == LevelFeature.NO_SWITCH), Is.GreaterThan(1));
            var castles = map.Entities
                .Where(e => e.EntityType.TypeId == TypeId.Special && e.EntityType.Model.Id == 1 || e.EntityType.TypeId == TypeId.Spawn)
                .Select(e => e.Position).Append(new Position(0, 0)).ToList();
            // Effects that change the ground in the first turn, not before the level starts (craters, explosions...)
            var firstTurn = map.Entities
                .Where(e => e.EntityType.TypeId == TypeId.Effect && !(TerrainFeatureBuilder.ShapesTerrain(e.EntityType.Model.Id) && e.DisId == LevelFeature.NO_SWITCH))
                .Select(e => e.Position).ToList();
            bool AtCastle(int x, int y) => castles.Any(c => WrappedDistance(x, c.X) <= castleSize && WrappedDistance(y, c.Y) <= castleSize)
                || firstTurn.Any(c => WrappedDistance(x, c.X) <= 12 && WrappedDistance(y, c.Y) <= 12);

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
        public void Mc2MakesLinesWithNoSwitchWhateverTheirTag()
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

            // Magic Carpet 2 tags every line node with no switch itself (remc2 SetStagetagForTermod_49830)
            Assert.That(noTag.MapTerrainType_10B4E0[50 | (100 << 8)], Is.EqualTo(8));
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
        public void CanyonDigsAlongItsLineAsDeepAsPar3()
        {
            var generated = Level2Terrain();
            // a row of dry land (canyons stop at water)
            int row = Enumerable.Range(0, 256).First(y => Enumerable.Range(30, 50).All(x => (generated.MapAngle_13B4E0[x | (y << 8)] & 0x0F) != 0
                && generated.MapHeightmap_11B4E0[x | (y << 8)] > 20));
            LevelFeature[] Canyon(ushort par3) => new[]
            {
                Node(1, TerrainFeatureBuilder.CANYON, 40, row, 0, 2),
                Node(2, TerrainFeatureBuilder.CANYON, 70, row, 1, 0),
            }.Select(n => { n.Par3 = par3; return n; }).ToArray();

            var shallow = Mc2Builder().Build(generated, Canyon(0));
            var deep = Mc2Builder().Build(generated, Canyon(3));

            int Sum(Terrain t) => Enumerable.Range(42, 26).Sum(x => t.MapHeightmap_11B4E0[x | (row << 8)]);
            Assert.That(Sum(shallow), Is.LessThan(Sum(generated)), "the canyon is dug");
            Assert.That(Sum(deep), Is.LessThan(Sum(shallow)), "Par3 3 digs deeper than 0");
            int away = 55 | (((row + 40) & 0xFF) << 8);
            Assert.That(shallow.MapHeightmap_11B4E0[away], Is.EqualTo(generated.MapHeightmap_11B4E0[away]));
        }

        [Test]
        public void RidgeRaisesItsLine()
        {
            var generated = Level2Terrain();
            var built = Mc2Builder().Build(generated, new List<LevelFeature>
            {
                Node(1, TerrainFeatureBuilder.RIDGE, 40, 100, 0, 2),
                Node(2, TerrainFeatureBuilder.RIDGE, 70, 100, 1, 0),
            });

            // 4 squares a turn: the ridge rises along the line unless it is water
            int raised = Enumerable.Range(40, 28).Count(x => built.MapHeightmap_11B4E0[x | (100 << 8)] > generated.MapHeightmap_11B4E0[x | (100 << 8)]);
            Assert.That(raised, Is.GreaterThan(20));
        }

        [Test]
        public void SearchRingsGoOutFromTheCentre()
        {
            var search = Search();
            var ring0 = new List<(int, int)>();
            var walk = search.Start(0, 1);
            while (walk.Next(out int dx, out int dy))
                ring0.Add((dx, dy));
            // ring 0 is the 2 x 2 centre, ring 1 around it; the last offset of the last ring is not given
            Assert.That(ring0.Take(4), Is.EquivalentTo(new[] { (0, 0), (1, 0), (0, 1), (1, 1) }));
            Assert.That(ring0.Skip(4), Has.All.Matches<(int, int)>(o => Math.Abs(o.Item1 - 0.5) <= 2 && Math.Abs(o.Item2 - 0.5) <= 2));
        }

        [Test]
        public void Mc2WizardsKeepTheirCastleLevels()
        {
            var map = new FileAdapter().LoadMap(ResourcePath("mc2_00590d.dat"));
            var castles = Features(map).Where(f => f.Model == TerrainFeatureBuilder.CASTLE).ToList();

            // level 9: one wizard (Flyer5, at 110,110) starts with a castle of level 3
            Assert.That(map.Wizards[4].CastleLevel, Is.EqualTo(3));
            Assert.That(castles, Has.Count.EqualTo(1));
            Assert.That((castles[0].X, castles[0].Y, (int)castles[0].Parent), Is.EqualTo((110, 110, 3)));
        }

        /// <summary>A castle at its start level: building shapes 1 to the level, stamped on the lowest height round shape 1.</summary>
        [Test]
        public void CastleIsBuiltOnItsWizardsStart()
        {
            var generated = Level2Terrain();
            var shapes = Shapes("mc2");
            // on an even square (x + y), so the castle stays where it is
            var castle = new LevelFeature { Id = 1, Model = TerrainFeatureBuilder.CASTLE, X = 150, Y = 150, Parent = 2 };

            var built = Mc2Builder().Build(generated, new List<LevelFeature> { castle });

            var first = shapes[1]!;
            int Edge(int x, int y) => generated.MapHeightmap_11B4E0[(x & 0xFF) | ((y & 0xFF) << 8)];
            int x0 = 150 - first.Width / 2, y0 = 150 - first.Height / 2;
            int z = Enumerable.Range(0, first.Width).SelectMany(i => new[] { Edge(x0 + i, y0), Edge(x0 + i, y0 + first.Height) })
                .Concat(Enumerable.Range(0, first.Height).SelectMany(i => new[] { Edge(x0 + first.Width, y0 + i), Edge(x0, y0 + i) }))
                .Append(250).Min();
            // the middle square: the height of the last shape that sets it (FFh leaves it)
            int expected = generated.MapHeightmap_11B4E0[150 | (150 << 8)];
            foreach (var shape in new[] { shapes[1]!, shapes[2]! })
            {
                int middle = shape.Offset + 2 * (shape.Height / 2 * shape.Width + shape.Width / 2);
                if (shape.Data[middle + 1] != 0xFF)
                    expected = (byte)(shape.Data[middle + 1] + z);
            }
            Assert.That(built.MapHeightmap_11B4E0[150 | (150 << 8)], Is.EqualTo(expected));
            Assert.That(built.MapHeightmap_11B4E0[30 | (30 << 8)], Is.EqualTo(generated.MapHeightmap_11B4E0[30 | (30 << 8)]));
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
