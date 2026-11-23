using MagicCarpet2Terrain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Builds the level's terrain features into generated terrain, as the games do before a level starts
    /// (remc2 GenerateEvents_49290, remc1 CreateFeatures_36430): each Effect with no switch (DisId FFFFh) of a kind
    /// that shapes the ground makes entities whose actions change the terrain, and those run until they are done.
    /// Built here: walls (chains of Wall nodes, and single WallSections), paths (chains of Path nodes) and buildings.
    /// Not built: canyons, ridges and the other features (volcanoes, cave tunnels...).
    /// Ported from remc2 and remc1; the entity slots the game runs them in are kept too, as where they overlap the
    /// order matters.
    /// </summary>
    public abstract class TerrainFeatureBuilder
    {
        // The Effect models (class 10) that shape the terrain, the same in both games
        public const int WALL_SECTION = 0x1B;
        public const int WALL = 0x1C;
        public const int PATH = 0x1D;
        public const int BUILDING = 0x2D;

        /// <summary>The level features this builder builds into the terrain.</summary>
        public static bool ShapesTerrain(int model) => model == WALL_SECTION || model == WALL || model == PATH || model == BUILDING;

        private const int BUILD_LIFE = 30;
        // A wall section is 2 squares thick (its life)
        private const int WALL_THICKNESS = 2;
        // A WallSection placed on its own: 10 long, raised along -y (action 1Bh)
        private const int WALL_SECTION_LENGTH = 10;
        private const int MAX_ENTITIES = 1000;

        // remc2 unk_D4A30 = remc1 terrainTypes_909AC: terrain type and angle bits by the shape of the tile's corners
        private static readonly byte[,] TerrainTypes = {
            { 0x1B, 0x00 }, { 0x1B, 0x50 }, { 0x1B, 0x30 }, { 0x1B, 0x60 }, { 0x1A, 0x00 }, { 0x1A, 0x50 }, { 0x1A, 0x30 }, { 0x1A, 0x60 },
            { 0x0A, 0x00 }, { 0x0A, 0x50 }, { 0x0A, 0x30 }, { 0x0A, 0x60 }, { 0x0A, 0x00 }, { 0x0A, 0x50 }, { 0x0A, 0x30 }, { 0x0A, 0x60 },
            { 0x0B, 0x00 }, { 0x0B, 0x50 }, { 0x0B, 0x30 }, { 0x0B, 0x60 }, { 0x0B, 0x00 }, { 0x0B, 0x50 }, { 0x0B, 0x30 }, { 0x0B, 0x60 },
            { 0x0C, 0x00 }, { 0x0C, 0x50 }, { 0x0C, 0x30 }, { 0x0C, 0x60 }, { 0x0C, 0x00 }, { 0x0C, 0x50 }, { 0x0C, 0x30 }, { 0x0C, 0x60 },
            { 0x15, 0x00 }, { 0x15, 0x50 }, { 0x15, 0x30 }, { 0x15, 0x60 }, { 0x16, 0x00 }, { 0x16, 0x50 }, { 0x16, 0x30 }, { 0x16, 0x60 },
            { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x18, 0x00 }, { 0x18, 0x50 }, { 0x18, 0x30 }, { 0x18, 0x60 },
            { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x17, 0x00 }, { 0x17, 0x50 }, { 0x17, 0x30 }, { 0x17, 0x60 },
            { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x00, 0x00 }, { 0x19, 0x00 }, { 0x19, 0x50 }, { 0x19, 0x30 }, { 0x19, 0x60 },
            { 0x10, 0x00 }, { 0x10, 0x50 }, { 0x10, 0x30 }, { 0x10, 0x60 }, { 0x0F, 0x00 }, { 0x0F, 0x50 }, { 0x0F, 0x30 }, { 0x0F, 0x60 },
            { 0x10, 0x00 }, { 0x10, 0x50 }, { 0x10, 0x30 }, { 0x10, 0x60 }, { 0x0F, 0x00 }, { 0x0F, 0x50 }, { 0x0F, 0x30 }, { 0x0F, 0x60 },
            { 0x1E, 0x00 }, { 0x1E, 0x50 }, { 0x1E, 0x30 }, { 0x1D, 0x60 }, { 0x1F, 0x00 }, { 0x1D, 0x50 }, { 0x1D, 0x30 }, { 0x1D, 0x60 },
            { 0x1E, 0x00 }, { 0x1E, 0x50 }, { 0x1E, 0x30 }, { 0x1E, 0x60 }, { 0x1D, 0x00 }, { 0x1D, 0x50 }, { 0x1D, 0x30 }, { 0x1D, 0x60 },
            { 0x21, 0x00 }, { 0x21, 0x50 }, { 0x21, 0x30 }, { 0x21, 0x60 }, { 0x22, 0x00 }, { 0x20, 0x50 }, { 0x20, 0x30 }, { 0x20, 0x60 },
            { 0x21, 0x00 }, { 0x21, 0x50 }, { 0x21, 0x30 }, { 0x21, 0x60 }, { 0x20, 0x00 }, { 0x20, 0x50 }, { 0x20, 0x30 }, { 0x20, 0x60 },
            { 0x13, 0x00 }, { 0x13, 0x50 }, { 0x13, 0x30 }, { 0x13, 0x60 }, { 0x14, 0x00 }, { 0x12, 0x50 }, { 0x12, 0x30 }, { 0x12, 0x60 },
            { 0x13, 0x00 }, { 0x13, 0x50 }, { 0x13, 0x30 }, { 0x13, 0x60 }, { 0x12, 0x00 }, { 0x12, 0x50 }, { 0x12, 0x30 }, { 0x12, 0x60 },
            { 0x13, 0x00 }, { 0x13, 0x50 }, { 0x13, 0x30 }, { 0x13, 0x60 }, { 0x14, 0x00 }, { 0x12, 0x50 }, { 0x12, 0x30 }, { 0x12, 0x60 },
            { 0x21, 0x00 }, { 0x21, 0x50 }, { 0x21, 0x30 }, { 0x21, 0x60 }, { 0x20, 0x00 }, { 0x20, 0x50 }, { 0x20, 0x30 }, { 0x20, 0x60 },
        };

        protected readonly BuildingShapes Shapes;

        protected byte[] Height;
        protected byte[] Angle;
        protected byte[] Type;
        protected byte[] Shading;
        protected byte[] Ceiling;
        protected bool IsCave;
        private bool _isNight;
        private byte[,] _angleTypes;
        private ushort _seed;
        // remc2 lowDiffHeightmap_D47DC / remc1 lowRange_90758: the last tile's corners differ by 8 or less
        private bool _lowRange;

        private Dictionary<int, LevelFeature> _features;
        // The free entity slots: entity 1 is taken first, a freed slot is taken again before the others
        private Stack<int> _freeSlots;

        /// <summary>An entity the features make: it runs one step a game turn until it is done.</summary>
        protected abstract class FeatureEntity
        {
            public int Slot;
            /// <summary>Whether the game frees the entity when it is done (buildings stay).</summary>
            public virtual bool FreedWhenDone => true;
            /// <returns>Whether it is done.</returns>
            public abstract bool Step();
        }

        private sealed class ActionEntity : FeatureEntity
        {
            private readonly Action _action;
            public ActionEntity(Action action) { _action = action; }
            public override bool Step()
            {
                _action();
                return true;
            }
        }

        protected sealed class Building : FeatureEntity
        {
            private readonly TerrainFeatureBuilder _builder;
            public Building(TerrainFeatureBuilder builder) { _builder = builder; }
            public BuildingShape Shape;
            public int Index;
            /// <summary>The square the building stands on (the centre of its shape).</summary>
            public int X;
            public int Y;
            /// <summary>The top left square of its shape.</summary>
            public int StartX;
            public int StartY;
            /// <summary>The height the shape is built up from: the average of the shape's 4 corners.</summary>
            public int Z;
            public int Life = BUILD_LIFE;
            public bool RaisesCeiling;
            public bool Done;
            public override bool FreedWhenDone => false;
            public override bool Step()
            {
                _builder.Step(this);
                return Done;
            }
        }

        protected TerrainFeatureBuilder(BuildingShapes shapes)
        {
            Shapes = shapes;
        }

        /// <summary>
        /// A copy of <paramref name="generated"/> with the level's features built into it. <paramref name="generated"/>
        /// itself is not changed, and is returned if it was not generated in full or nothing changes it.
        /// </summary>
        public Terrain Build(Terrain generated, IEnumerable<LevelFeature> features)
        {
            var list = features?.Where(f => f != null).Select(f => f.Copy()).OrderBy(f => f.Id).ToList() ?? new List<LevelFeature>();
            if (!list.Any(f => ShapesTerrain(f.Model) && f.DisId == LevelFeature.NO_SWITCH) || generated?.AngleTypes == null
                || generated.MapHeightmap_11B4E0?.Length != 0x10000 || generated.MapShading_12B4E0 == null)
                return generated;

            var terrain = generated.Clone();
            Height = terrain.MapHeightmap_11B4E0;
            Angle = terrain.MapAngle_13B4E0;
            Type = terrain.MapTerrainType_10B4E0;
            Shading = terrain.MapShading_12B4E0;
            Ceiling = terrain.MapCeilingHeightmap_14B4E0;
            var mapType = terrain.GenerationParameters?.MapType ?? MapType.Day;
            IsCave = mapType == MapType.Cave && Ceiling != null;
            _isNight = mapType != MapType.Day;
            _angleTypes = terrain.AngleTypes;
            _seed = terrain.Seed;
            _features = new Dictionary<int, LevelFeature>();
            foreach (var feature in list)
                _features[feature.Id] = feature;
            _freeSlots = new Stack<int>(Enumerable.Range(1, MAX_ENTITIES - 1).Reverse());

            foreach (var pass in GetPasses())
            {
                // The features of a pass make their entities (buildings take their base height then), then those run
                var entities = new List<FeatureEntity>();
                foreach (var feature in list)
                {
                    if (feature.Built || feature.DisId != LevelFeature.NO_SWITCH || !pass(feature))
                        continue;
                    Prepare(feature, entities);
                    feature.Built = true;
                }
                Run(entities);
            }

            terrain.Seed = _seed;
            return terrain;
        }

        /// <summary>The groups of features (by their model) in the order the game builds them; each is built in turn.</summary>
        protected abstract IEnumerable<Func<LevelFeature, bool>> GetPasses();

        /// <summary>
        /// remc2 ApplyEvents_498A0 / remc1 InitCreatedEntities_36620: each turn the entities take a step in the order of
        /// their slots, until all are done; a done entity is freed (its slot goes on top of the free ones).
        /// </summary>
        private void Run(List<FeatureEntity> entities)
        {
            var active = entities.ToList();
            while (active.Count > 0)
            {
                foreach (var entity in active.OrderBy(e => e.Slot).ToList())
                {
                    if (!entity.Step())
                        continue;
                    active.Remove(entity);
                    if (entity.FreedWhenDone)
                        _freeSlots.Push(entity.Slot);
                }
            }
        }

        private void Add(List<FeatureEntity> entities, FeatureEntity entity)
        {
            if (entity == null || _freeSlots.Count == 0)
                return;
            entity.Slot = _freeSlots.Pop();
            entities.Add(entity);
        }

        /// <summary>remc2 PrepareEvents_49540 / remc1 CreateFeature_36480: the entities of one feature.</summary>
        private void Prepare(LevelFeature feature, List<FeatureEntity> entities)
        {
            switch (feature.Model)
            {
                case WALL:
                case PATH:
                    // a line of nodes is made once, from the first node with a stage tag (SwitchId)
                    if (feature.SwitchId != 0)
                        BuildChain(feature, entities);
                    break;
                case WALL_SECTION:
                    {
                        int x = feature.X, y = feature.Y;
                        Add(entities, new ActionEntity(() => RaiseWallYMinus(x, y, WALL_SECTION_LENGTH, WALL_THICKNESS)));
                        break;
                    }
                case BUILDING:
                    Add(entities, Place(feature));
                    break;
            }
        }

        /// <summary>
        /// remc2 CreateLinkedLevelFeatures_49090 / remc1 CreateFeatureChain_362C0: back along Parent to the first node,
        /// then a line from each node to the next (Child) while they are of the same kind. Every node's stage tag is
        /// cleared so the line is made once.
        /// </summary>
        private void BuildChain(LevelFeature feature, List<FeatureEntity> entities)
        {
            // the game would loop for ever on a ring of links: each node is taken once here
            var seen = new HashSet<int> { feature.Id };
            var current = feature;
            while (current.Parent != 0 && _features.TryGetValue(current.Parent, out var previous) && seen.Add(previous.Id))
                current = previous;
            seen.Clear();
            while (current.Model == feature.Model && seen.Add(current.Id))
            {
                current.SwitchId = 0;
                if (current.Child == 0 || !_features.TryGetValue(current.Child, out var next))
                    break;
                if (feature.Model == WALL)
                    AddWallLine(current.X, current.Y, next.X, next.Y, entities);
                else
                    AddPathLine(current.X, current.Y, next.X, next.Y, entities);
                current = next;
            }
        }

        // remc2 WrappedDelta_48370 / remc1 WrapDelta_358D0: the shorter way on the wrapped map
        private static int WrapDelta(int from, int to)
        {
            int delta = to - from;
            if (delta > 0x80)
                return delta - 0x100;
            if (delta < -0x80)
                return delta + 0x100;
            return delta;
        }

        private enum WallDirection { YMinus, YPlus, X }

        /// <summary>
        /// remc2 CreateSubtype27LineBetweenPoints_48400 / remc1 CreateFeatureLine1C_35960: the wall from (x, y) to
        /// (x2, y2) as steps of at most about 10 squares, each a section along x and one along y.
        /// </summary>
        private void AddWallLine(int x, int y, int x2, int y2, List<FeatureEntity> entities)
        {
            int dx = WrapDelta(x, x2);
            int dy = WrapDelta(y, y2);
            if (dx == 0 && dy == 0)
                return;
            if (dx < 0)
            {
                // always from the left
                dx = -dx;
                dy = -dy;
                x = x2;
                y = y2;
            }
            if (Math.Abs(dy) < dx)
            {
                int steps = dx / 10 + 1;
                int stepX = dx / steps, restX = dx - stepX * steps;
                int stepY = dy / steps, restY = dy - stepY * steps;
                for (; steps > 0; steps--)
                {
                    AddWallSection(x, y, WallDirection.X, stepX + restX, entities);
                    x += stepX + restX;
                    AddYWallSection(x, y, stepY, restY, entities);
                    y += stepY + restY;
                    restX = 0;
                    restY = 0;
                }
            }
            else
            {
                int steps = Math.Abs(dy / 10) + 1;
                int stepY = dy / steps, restY = dy - stepY * steps;
                int stepX = dx / steps, restX = dx - stepX * steps;
                for (; steps > 0; steps--)
                {
                    AddYWallSection(x, y, stepY, restY, entities);
                    y += stepY + restY;
                    AddWallSection(x, y, WallDirection.X, stepX + restX, entities);
                    x += stepX + restX;
                    restX = 0;
                    restY = 0;
                }
            }
        }

        private void AddYWallSection(int x, int y, int stepY, int restY, List<FeatureEntity> entities)
        {
            if (stepY < 0)
                AddWallSection(x, y, WallDirection.YMinus, -stepY - restY, entities);
            else
                AddWallSection(x, y, WallDirection.YPlus, stepY + restY, entities);
        }

        private void AddWallSection(int x, int y, WallDirection direction, int length, List<FeatureEntity> entities)
        {
            x &= 0xFF;
            y &= 0xFF;
            switch (direction)
            {
                case WallDirection.YMinus:
                    Add(entities, new ActionEntity(() => RaiseWallYMinus(x, y, length, WALL_THICKNESS)));
                    break;
                case WallDirection.YPlus:
                    Add(entities, new ActionEntity(() => RaiseWallYPlus(x, y, length, WALL_THICKNESS)));
                    break;
                default:
                    Add(entities, new ActionEntity(() => RaiseWallX(x, y, length, WALL_THICKNESS)));
                    break;
            }
        }

        /// <summary>
        /// remc2 CreateSubtype30LineBetweenPoints_48690 / remc1 CreateFeatureLine1D_35BF0: the path from (x, y) to
        /// (x2, y2) as a diagonal run, then a straight one.
        /// </summary>
        private void AddPathLine(int x, int y, int x2, int y2, List<FeatureEntity> entities)
        {
            int dx = WrapDelta(x, x2);
            int dy = WrapDelta(y, y2);
            int signX = Math.Sign(dx);
            int signY = Math.Sign(dy);
            int ax = Math.Abs(dx);
            int ay = Math.Abs(dy);
            int diagonal = ax;
            int straight = Math.Abs(ay - ax);
            int straightX = 0, straightY = signY;
            if (ax > ay)
            {
                diagonal = ay;
                straightX = signX;
                straightY = 0;
            }
            int startX = x & 0xFF, startY = y & 0xFF;
            Add(entities, new ActionEntity(() => MarkPath(startX, startY, signX, signY, diagonal)));
            int midX = (x + signX * diagonal) & 0xFF, midY = (y + signY * diagonal) & 0xFF;
            Add(entities, new ActionEntity(() => MarkPath(midX, midY, straightX, straightY, straight)));
        }

        /// <summary>remc2 ApplyPointToPath_343F0 / remc1 ApplyPathAction_26890: <paramref name="count"/> squares get the mark 1.</summary>
        private void MarkPath(int x, int y, int stepX, int stepY, int count)
        {
            for (int i = (ushort)count; i > 0; i--, x += stepX, y += stepY)
            {
                int tile = Tile(x, y);
                Angle[tile] = (byte)((Angle[tile] & 0xF0) | 1);
                UpdateTerrainTiles(x, y, x, y);
            }
        }

        /// <summary>
        /// remc2 UpdateEffectAction28_34000 / remc1 RaiseWallYPlusAction_26560: <paramref name="length"/> +
        /// <paramref name="thickness"/> rows down from (x, y), each <paramref name="thickness"/> squares wide.
        /// </summary>
        private void RaiseWallYPlus(int x, int y, int length, int thickness)
        {
            if ((x & 1) != 0)
                x++;
            x = x - thickness + 1;
            for (int rows = (ushort)(length + thickness); rows > 0; rows--, y++)
                RaiseWallRow(x, y, thickness);
        }

        /// <summary>remc2 UpdateTerrainPaint_34110 / remc1 RaiseWallYMinusAction_26670: the rows go up from 2 below (x, y).</summary>
        private void RaiseWallYMinus(int x, int y, int length, int thickness)
        {
            y += 2;
            for (int rows = (ushort)(length + thickness); rows > 0; rows--, y--)
                RaiseWallRow(x, y, thickness);
        }

        /// <summary>
        /// The squares of a row are the next ones in the map (a row runs on into the next at the edge); the squares
        /// before and after it are marked as set by hand (80h).
        /// </summary>
        private void RaiseWallRow(int x, int y, int width)
        {
            Angle[Tile(x - 1, y)] |= 0x80;
            ushort tile = (ushort)Tile(x, y);
            for (int i = (ushort)width; i > 0; i--, tile++)
                RaiseWallTile(tile);
            Angle[tile] |= 0x80;
        }

        /// <summary>remc2 UpdateEffectAction29_34210 / remc1 RaiseWallXAction_26760: <paramref name="thickness"/> rows of <paramref name="length"/> squares.</summary>
        private void RaiseWallX(int x, int y, int length, int thickness)
        {
            if (((x + y) & 1) != 0)
                x++;
            int width = (ushort)length;
            ushort tile = (ushort)Tile(x, y - 1);
            for (int i = width; i > 0; i--, tile++)
                Angle[tile] |= 0x80;
            for (int rows = thickness; rows > 0; rows--, y++)
            {
                tile = (ushort)Tile(x, y);
                for (int i = width; i > 0; i--, tile++)
                    RaiseWallTile(tile);
            }
            tile = (ushort)Tile(x, y);
            for (int i = width; i > 0; i--, tile++)
                Angle[tile] |= 0x80;
        }

        /// <summary>A square of wall goes up 48 (unless it is wall already among walls) and is wall (type 8) with the 3 before it.</summary>
        private void RaiseWallTile(int tile)
        {
            if (Type[tile] != 8 || IsWallEdge(tile))
                Height[tile] = (byte)(Height[tile] + 48);
            int x = tile & 0xFF, y = tile >> 8;
            // remc2 SetTerrainTypeAndNeighbours_46180 / remc1 SetTerrainTypeAround_33AE0
            Type[Tile(x, y)] = 8;
            Type[Tile(x - 1, y)] = 8;
            Type[Tile(x - 1, y - 1)] = 8;
            Type[Tile(x, y - 1)] = 8;
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                    ShadeWallTile(x - 1 + column, y - 1 + row);
        }

        /// <summary>
        /// remc2 CheckTileType8Neighbours_33F70 / remc1 CheckTileType8Neighbours_264D0: whether a square of wall is
        /// still raised again: the square before it is not wall, or a square next to it is over 30 higher.
        /// </summary>
        protected virtual bool IsWallEdge(int tile)
        {
            int x = tile & 0xFF, y = tile >> 8;
            int limit = Height[tile] + 30;
            return Type[Tile(x - 1, y)] != 8 || Height[Tile(x - 1, y)] > limit || Height[Tile(x + 1, y)] > limit
                || Height[Tile(x, y + 1)] > limit || Height[Tile(x, y - 1)] > limit;
        }

        /// <summary>The shading of a square next to new wall.</summary>
        protected virtual void ShadeWallTile(int x, int y) => ShadeTile(x, y);

        /// <summary>remc2 InitBuildingEntity_49A30 / remc1 InitBuildingEntity_36DF0.</summary>
        private Building Place(LevelFeature feature)
        {
            int index = GetShapeIndex(feature);
            var shape = Shapes?[index];
            if (shape == null || shape.IsEmpty || !IsComplete(shape))
                return null;
            int width = shape.Width;
            int height = shape.Height;
            int x = feature.X & 0xFF;
            int y = feature.Y & 0xFF;
            int startX = (x - (width >> 1)) & 0xFF;
            int startY = (y - (height >> 1)) & 0xFF;
            if ((startX + startY) % 2 != 0)
            {
                // one square right on the odd squares
                x = (x + 1) & 0xFF;
                startX = (startX + 1) & 0xFF;
            }
            // remc2 GetTerrainHeightFromSquare_48DF0
            int z = (Height[Tile(startX, startY)] + Height[Tile(startX + width, startY)]
                + Height[Tile(startX + width, startY + height)] + Height[Tile(startX, startY + height)]) >> 2;
            var building = new Building(this) { Shape = shape, Index = index, X = x, Y = y, StartX = startX, StartY = startY, Z = z };
            OnPlaced(building);
            return building;
        }

        /// <summary>The index in <see cref="Shapes"/> of a building's shape.</summary>
        protected abstract int GetShapeIndex(LevelFeature building);

        /// <summary>Whether the shape's data holds all of it.</summary>
        protected abstract bool IsComplete(BuildingShape shape);

        /// <summary>One step of building (the building's action, once per game turn).</summary>
        protected abstract void Step(Building building);

        protected virtual void OnPlaced(Building building)
        {
        }


        protected static int Tile(int x, int y) => (x & 0xFF) | ((y & 0xFF) << 8);

        protected static bool IsSteep(byte type) => type > 5 && type <= 0x22;

        /// <summary>One step of a square's height toward <paramref name="target"/>; a flat square gets the mark 1.</summary>
        protected void StepHeight(int x, int y, int target, int life)
        {
            int tile = Tile(x, y);
            Height[tile] = (byte)(Height[tile] + (target - Height[tile]) / life);
            if ((Angle[tile] & 7) == 0)
            {
                Angle[tile] = (byte)((Angle[tile] & 0xF0) | 1);
                UpdateTerrainTiles(x, y, x, y);
            }
        }

        /// <summary>In caves: the ceiling of a square at or below the floor is closed (angle bit 8).</summary>
        protected void CheckCeiling(int tile)
        {
            if (Ceiling[tile] > Height[tile])
                Angle[tile] &= 0xF7;
            else
            {
                Ceiling[tile] = (byte)(Height[tile] - 1);
                Angle[tile] |= 8;
            }
        }

        private static readonly int[,] Corners = { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 0, 1 } };

        /// <summary>
        /// remc2 GetTileHeightType_45BE0 / remc1 GetTileHeightType_33640: which corner of the square (x, y) is highest,
        /// or 4-7 for an edge when the second highest is close. <paramref name="callerCl"/> and <paramref name="callerDl"/>
        /// are what the caller leaves in cl and dl, which the original uses when the corners are all 0.
        /// </summary>
        private byte GetTileHeightType(byte callerCl, int x, int y, byte callerDl)
        {
            byte lowest = 0xFF;
            byte highest = 0;
            int corner = callerDl;
            for (int i = 0; i < 4; i++)
            {
                byte h = Height[Tile(x + Corners[i, 0], y + Corners[i, 1])];
                if (h > highest)
                {
                    highest = h;
                    corner = i;
                }
                if (h < lowest)
                    lowest = h;
            }
            byte second = 0;
            int secondCorner = callerCl;
            for (int i = 0; i < 4; i++)
            {
                if (corner == i)
                    continue;
                byte h = Height[Tile(x + Corners[i, 0], y + Corners[i, 1])];
                if (h > second)
                {
                    second = h;
                    secondCorner = i;
                }
            }
            _lowRange = highest - lowest <= 8;
            if (highest - second >= 8)
                return (byte)corner;
            switch (corner)
            {
                case 0:
                    return (byte)(secondCorner == 1 ? 4 : 7);
                case 1:
                    return (byte)(secondCorner == 2 ? 5 : 4);
                case 2:
                    return (byte)(secondCorner == 3 ? 6 : 5);
                case 3:
                    return (byte)(secondCorner == 0 ? 7 : 6);
            }
            return 0;
        }

        /// <summary>
        /// For type 10h: Magic Carpet passes the square's old terrain type on as dl, Magic Carpet 2 the caller's dl.
        /// </summary>
        protected virtual bool PassesOldTypeForType10 => false;

        /// <summary>
        /// remc2 SetTileTerrainType_45DC0 / remc1 SetTileTerrainType_33800: below 8 the square's corner mark, which
        /// gives the terrain types around it; from 8 a terrain type, some chosen by the slope of the square.
        /// </summary>
        protected void SetTileTerrainType(byte callerCl, int x, int y, byte type, byte callerDl)
        {
            int tile = Tile(x, y);
            if (type < 8)
            {
                Angle[tile] = (byte)((Angle[tile] & 0xF0) | type);
                UpdateTerrainTiles(x, y, x, y);
                return;
            }
            int parity = ((x + y) & 1) << 3;
            int entry = -1;
            switch (type)
            {
                case 8:
                    Type[tile] = 8;
                    break;
                case 9:
                    Type[tile] = 9;
                    break;
                case 0x0F:
                    Type[tile] = 0x0B;
                    break;
                case 0x10:
                    {
                        byte old = Type[tile];
                        if (old != 0x0A && old != 0x0B && old != 0x0C)
                            entry = GetTileHeightType(callerCl, x, y, PassesOldTypeForType10 ? old : callerDl);
                        break;
                    }
                case 0x11:
                    entry = 32 + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x12:
                    entry = 40 + parity + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x13:
                    entry = 48 + parity + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x14:
                    entry = 8 + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x15:
                    entry = 16 + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x16:
                    entry = 24 + GetTileHeightType(callerCl, x, y, callerDl);
                    break;
                case 0x0A:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                case 0x0E:
                    {
                        // 16 per type, + 8 when the corners differ little
                        int index = GetTileHeightType(callerCl, x, y, callerDl) + (type - 0x0A) * 0x10;
                        if (_lowRange)
                            index += 8;
                        entry = 64 + index;
                        break;
                    }
            }
            if (entry >= 0 && entry < TerrainTypes.GetLength(0))
            {
                Type[tile] = TerrainTypes[entry, 0];
                Angle[tile] = (byte)((Angle[tile] & 0x8F) | TerrainTypes[entry, 1]);
            }
            // Changed by hand: the square keeps its type (80h), and its corners are not water edges / closed rock
            Angle[tile] |= 0x80;
            for (int i = 0; i < 4; i++)
            {
                int corner = Tile(x + Corners[i, 0], y + Corners[i, 1]);
                if (IsCave)
                    CheckCeiling(corner);
                else
                    Angle[corner] &= 0xF7;
            }
        }

        /// <summary>
        /// remc2 UpdateTerrainTiles_462A0 / remc1 UpdateTerrainTiles_33B90: the terrain types of the squares from
        /// (fromX, fromY) to (toX, toY) again from their corner marks (keeping those set by hand), then the shading.
        /// </summary>
        protected void UpdateTerrainTiles(int fromX, int fromY, int toX, int toY)
        {
            // byte counters: the area can wrap over the map edge
            byte width = (byte)(toX - fromX + 1);
            byte height = (byte)(toY - fromY + 1);
            // 1: the squares touching the area, not those set by hand
            for (int row = 0; row < height; row++)
                for (int column = 0; column < width; column++)
                {
                    int x = fromX + column;
                    int y = fromY + row;
                    ResetType(Tile(x, y));
                    ResetType(Tile(x - 1, y));
                    ResetType(Tile(x - 1, y - 1));
                    ResetType(Tile(x, y - 1));
                }
            // 2: one square more up and left
            int startX = fromX - 1;
            int startY = fromY - 1;
            width++;
            height++;
            for (int row = 0; row < height; row++)
                for (int column = 0; column < width; column++)
                {
                    int x = startX + column;
                    int y = startY + row;
                    int tile = Tile(x, y);
                    if (Type[tile] != 1)
                        continue;
                    int key = (Angle[tile] & 7) * 343 + (Angle[Tile(x + 1, y)] & 7) * 49
                        + (Angle[Tile(x + 1, y + 1)] & 7) * 7 + (Angle[Tile(x, y + 1)] & 7);
                    byte type = key < _angleTypes.GetLength(0) ? _angleTypes[key, 0] : (byte)0;
                    Type[tile] = type;
                    if (type >= 8)
                        Angle[tile] = (byte)(_angleTypes[key, 1] + (Angle[tile] & 0x87));
                    else
                    {
                        _seed = unchecked((ushort)(9377 * _seed + 9439));
                        Angle[tile] = (byte)((Angle[tile] & 0x87) + 16 * (_seed % 7));
                    }
                }
            // 3: the shading, one square more right and down
            width++;
            height++;
            for (int row = 0; row < height; row++)
                for (int column = 0; column < width; column++)
                    ShadeTile(startX + column, startY + row);
        }

        /// <summary>
        /// The shading of a square from the slope across it (Night and Cave levels the other way round); in caves a
        /// ceiling at or below the floor is closed, elsewhere the square is no water edge (angle bit 8).
        /// </summary>
        protected void ShadeTile(int x, int y)
        {
            int tile = Tile(x, y);
            int shade = (sbyte)(Height[Tile(x - 1, y - 1)] - Height[Tile(x + 1, y + 1)] + 32);
            if (shade >= 28)
            {
                if (shade > 40)
                    shade = (shade & 7) + 40;
            }
            else
                shade = (shade & 3) + 28;
            Shading[tile] = _isNight ? (byte)(64 - shade) : (byte)shade;
            if (IsCave && Ceiling[tile] <= Height[tile])
            {
                Ceiling[tile] = (byte)(Height[tile] - 1);
                Angle[tile] |= 8;
            }
            else
                Angle[tile] &= 0xF7;
        }

        private void ResetType(int tile)
        {
            if ((Angle[tile] & 0x80) == 0)
                Type[tile] = 1;
        }

        /// <summary>
        /// remc2 DrawTerrainLine_48A20 / remc1 SmoothBuildingBorder_35F30: smooths a band <paramref name="border"/>
        /// squares wide around the rectangle 2 * halfWidth by 2 * halfHeight from (left, top).
        /// </summary>
        protected void SmoothBorder(int left, int top, int halfHeight, int halfWidth, int border)
        {
            // the left and the right side
            int row = top;
            for (int i = 0; i < 2 * halfHeight; i++, row++)
            {
                int a = left - border;
                int b = left + 2 * halfWidth;
                for (int j = 0; j <= border; j++, a++, b++)
                {
                    SmoothTile(Tile(a, row));
                    SmoothTile(Tile(b, row));
                }
            }
            // the top and the bottom side
            int column = left - border;
            for (int i = 0; i < 2 * halfWidth + 2 * border; i++, column++)
            {
                int a = top - border;
                int b = top + 2 * halfHeight;
                for (int j = 0; j <= border; j++, a++, b++)
                {
                    SmoothTile(Tile(column, a));
                    SmoothTile(Tile(column, b));
                }
            }
        }

        private static readonly int[] SmoothCorners = { -0x101, -0x100, -1, 0 };

        /// <summary>remc2 SetHeightmapByBuilding_48B90 / remc1 SmoothTileHeight_360C0: the average height of the 3x3 squares that are not steep.</summary>
        private void SmoothTile(int tile)
        {
            if ((Angle[tile] & 7) == 0 || Height[tile] == 0)
                return;
            foreach (int offset in SmoothCorners)
            {
                // before the map at the row 0 the original reads other memory, taken as type 0 like remc2
                int index = tile + offset;
                if (index >= 0 && IsSteep(Type[index]))
                    return;
            }
            uint sum = 0;
            uint count = 0;
            ushort i = unchecked((ushort)(tile - 0x101));
            for (int row = 0; row < 3; row++, i = unchecked((ushort)(i + 0xFD)))
                for (int column = 0; column < 3; column++, i++)
                    if (!IsSteep(Type[i]))
                    {
                        count++;
                        sum += Height[i];
                    }
            if (count == 0)
                return;
            Height[tile] = (byte)(sum / count);
            if (IsCave)
                CheckCeiling(tile);
        }
    }
}
