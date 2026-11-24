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
        /// <summary>The nodes of a canyon line.</summary>
        public const int CANYON = 0x1F;
        /// <summary>What digs a canyon: it moves along the line, leaving a <see cref="DIG"/> on every square.</summary>
        public const int CANYON_MOVER = 0x20;
        public const int BUILDING = 0x2D;
        /// <summary>The nodes of a ridge line.</summary>
        public const int RIDGE = 0x32;
        /// <summary>What raises a ridge: it moves along the line, raising the ground until it reaches water.</summary>
        public const int RIDGE_MOVER = 0x33;
        /// <summary>The volcano spell (remc2 ManaSphereSpawner): a cone with a crater.</summary>
        public const int VOLCANO = 0x09;
        /// <summary>A crater on its own, dug for as long as its spell level says.</summary>
        public const int CRATER = 0x0B;
        /// <summary>Wanders, leaving small craters.</summary>
        public const int EMBER = 0x0F;
        /// <summary>A path point on its own: it marks nothing, but takes an entity slot.</summary>
        public const int PATH_POINT = 0x1E;
        /// <summary>Caves: the nodes of a tunnel line.</summary>
        public const int TUNNEL = 0x50;
        /// <summary>Caves: what carves a tunnel to its target.</summary>
        public const int TUNNEL_CARVER = 0x51;
        /// <summary>Caves: a room carved round its square (Parent by Child, Par3 deep).</summary>
        public const int ROOM = 0x52;
        /// <summary>Caves: a mound rising toward the ceiling.</summary>
        public const int CAVE_MOUND = 0x53;
        /// <summary>Caves: a cone hanging from the ceiling.</summary>
        public const int CEILING_CONE = 0x54;
        /// <summary>Caves: a cone rising from the floor.</summary>
        public const int FLOOR_CONE = 0x55;
        /// <summary>The Special (class 14) model of a cave door.</summary>
        public const int SPECIAL_CAVE_DOOR = 2;
        /// <summary>A cave door (Special 2) as a feature: a model of its own past the Effects.</summary>
        public const int CAVE_DOOR = 0x100 + SPECIAL_CAVE_DOOR;
        /// <summary>The Special (class 14) model of a gate.</summary>
        public const int SPECIAL_GATE = 1;
        /// <summary>A gate (Special 1) as a feature: closed as the level starts (built on its first update).</summary>
        public const int GATE = 0x100 + SPECIAL_GATE;
        /// <summary>A wizard's castle as a feature: on the wizard's start, its level in Parent (not a level entity).</summary>
        public const int CASTLE = 0x200;
        /// <summary>A pit that widens for a few turns (remc2 SpellBlast, the model 11).</summary>
        private const int DIG = CRATER;
        private const int EMBER_DIG_RADIUS = 4;

        /// <summary>The level features this builder builds into the terrain.</summary>
        public static bool ShapesTerrain(int model) => model == WALL_SECTION || model == WALL || model == PATH || model == BUILDING
            || model == CANYON || model == CANYON_MOVER || model == RIDGE || model == RIDGE_MOVER
            || model == VOLCANO || model == CRATER || model == EMBER || model == PATH_POINT
            || model == TUNNEL || model == TUNNEL_CARVER || model == ROOM || model == CAVE_MOUND || model == CEILING_CONE || model == FLOOR_CONE
            || model == CAVE_DOOR || model == GATE || model == CASTLE;

        private const int BUILD_LIFE = 30;
        // A wall section is 2 squares thick (its life)
        private const int WALL_THICKNESS = 2;
        // A WallSection placed on its own: 10 long, raised along -y (action 1Bh)
        private const int WALL_SECTION_LENGTH = 10;
        private const int MAX_ENTITIES = 1000;
        // A canyon mover moves a square a turn, a ridge mover 4
        private const int CANYON_SPEED = 0x100;
        private const int RIDGE_SPEED = 0x400;
        // The rings a dig widens to (its box, 900h) and a ridge raises (300h)
        private const int DIG_RADIUS = 9;
        private const int RIDGE_RADIUS = 3;
        // How deep a dig goes (its life) when the level does not say
        private const int DEFAULT_DIG_DEPTH = 2;

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
        // The entities by their slot
        private FeatureEntity[] _slots;
        // remc2 world rand_0x8 / remc1 state rand_0x04_4: the level seed, a step on each time the entities run
        private uint _worldRand;
        private readonly SearchRings _search;

        /// <summary>An entity the features make: it runs one step a game turn until it is done.</summary>
        protected abstract class FeatureEntity
        {
            public int Slot;
            /// <summary>remc2 instanceIndex_0x3E_62 / remc1 index_0x3F_63: its slot, then one on a turn (MC2).</summary>
            public byte Index;
            /// <summary>Its random seed: its slot and the world's.</summary>
            public uint Rand;
            /// <summary>Done, but still in its slot (a building).</summary>
            public bool Finished;
            /// <summary>Whether the game frees the entity when it is done (buildings stay).</summary>
            public virtual bool FreedWhenDone => true;
            /// <returns>Whether it is done.</returns>
            public abstract bool Step();
        }

        /// <summary>A canyon or ridge mover, or a dig: an entity with a position (8.8 fixed point, wrapping).</summary>
        private abstract class MovingEntity : FeatureEntity
        {
            protected readonly TerrainFeatureBuilder Builder;
            protected MovingEntity(TerrainFeatureBuilder builder) { Builder = builder; }
            public short X;
            public short Y;
            public int Life;
            public ushort Yaw;
        }

        /// <summary>remc2 UpdateEffect0A20_344A0 / remc1 LayTrailAction_26920: a dig on every square until its life or water.</summary>
        private sealed class CanyonMover : MovingEntity
        {
            public CanyonMover(TerrainFeatureBuilder builder) : base(builder) { }
            public int Depth = DEFAULT_DIG_DEPTH;
            public override bool Step()
            {
                if (Life-- < 0 || Builder.IsWater(X, Y))
                    return true;
                Builder.Add(new Dig(Builder) { X = X, Y = Y, Life = Depth });
                Move(ref X, ref Y, Yaw, CANYON_SPEED);
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateSpellBlast_31FB0 / remc1 SinkTerrainAction_25670: lowers its rings by 3 a turn, a ring wider every
        /// 3rd turn (by its index), the outer ring once, until its life or water.
        /// </summary>
        private sealed class Dig : MovingEntity
        {
            public Dig(TerrainFeatureBuilder builder) : base(builder) { }
            /// <summary>The rings it widens to (its box, 900h, or an ember's 400h).</summary>
            public int Radius = DIG_RADIUS;
            private int _rings;
            private bool _started;
            public override bool Step()
            {
                if (Index % 3 == 0)
                    _rings++;
                if (Life-- < 0 || Builder.IsWater(X, Y))
                    return true;
                int last = _rings;
                if (last > Radius - 1)
                {
                    last = Radius - 1;
                    if (!_started)
                        Builder.ChangeHeightsAround(X, Y, Radius, Radius, Radius, -3, false);
                }
                _started = true;
                Builder.ChangeHeightsAround(X, Y, Radius, 0, last, -3, false);
                return false;
            }
        }

        /// <summary>remc2 UpdateEffectUntilWater_352C0 / remc1 RaiseRidgeAction_269A0: raises its 3 rings by 10-24 a turn.</summary>
        private sealed class RidgeMover : MovingEntity
        {
            public RidgeMover(TerrainFeatureBuilder builder) : base(builder) { }
            public override bool Step()
            {
                if (Life-- < 0 || Builder.IsWater(X, Y))
                    return true;
                int add = (int)(Builder.NextRand(this) % 15) + 10;
                Builder.ChangeHeightsAround(X, Y, RIDGE_RADIUS, 0, 0x400, add, true);
                Move(ref X, ref Y, Yaw, RIDGE_SPEED);
                return false;
            }
        }

        /// <summary>An entity that does nothing to the terrain: the game frees it the first time round, but it took a slot.</summary>
        private sealed class SlotOnly : FeatureEntity
        {
            public override bool Step() => true;
        }

        /// <summary>
        /// remc2 UpdateEmberSpawner_32530: wanders a square a turn (turning up to 45 either way at random), leaving a
        /// small dig (4 rings, 10 turns) on every square, until its life or 8 turns more on water than off it.
        /// </summary>
        private sealed class Ember : MovingEntity
        {
            public Ember(TerrainFeatureBuilder builder) : base(builder) { }
            private int _wet;
            public override bool Step()
            {
                if (Builder.IsWater(X, Y))
                    _wet++;
                else if (_wet > 0)
                    _wet--;
                Life--;
                if (Life < -1 || _wet > 8)
                    return true;
                Yaw = (ushort)(((int)(Builder.NextRand(this) % 0x5B) + Yaw - 45) & 0x7FF);
                Move(ref X, ref Y, Yaw, CANYON_SPEED);
                Builder.Add(new Dig(Builder) { X = X, Y = Y, Life = 10, Radius = EMBER_DIG_RADIUS });
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateManaSphereSpawner_31940 (the Effect 9, the volcano spell): first takes the lowest height round
        /// its square as its base, then for 16 turns raises a cone (its radius from its spell level) a step toward its
        /// shape, then cuts the top flat and sets the crater in the middle.
        /// </summary>
        private sealed class Volcano : MovingEntity
        {
            public Volcano(TerrainFeatureBuilder builder) : base(builder) { }
            public int Size;
            private int _state;
            private int _radius;
            private int _base;
            private int _height;

            public override bool Step()
            {
                var b = Builder;
                int cx = ((ushort)(X + 128) >> 8) & 0xFF;
                int cy = ((ushort)(Y + 128) >> 8) & 0xFF;
                if (_state == 0)
                {
                    int half = (Size | 1) & 0xFF;
                    _radius = (short)(half << 8);
                    _base = b.GetMinHeightOnEdge((cx - half) & 0xFF, (cy - half) & 0xFF, 2 * half, 2 * half);
                    _height = 2 * half + 100;
                    if (_base + _height > 255)
                        _height = 255 - _base;
                    // and it raises the first step straight away
                    _state = 1;
                }
                if (_state == 2)
                {
                    int top = _base + _height - 24;
                    int half = _radius >> 8;
                    int size = _radius >> 7;
                    for (int row = 0; row < size; row++)
                        for (int column = 0; column < size; column++)
                        {
                            int tile = Tile(cx - half + column, cy - half + row);
                            if (b.Height[tile] > top)
                                b.Height[tile] = (byte)top;
                        }
                    b.SetCrater(cx, cy, top - 16);
                    return true;
                }
                if (--Life <= 0)
                {
                    _state = 2;
                    return false;
                }
                int squares = _radius >> 7;
                int flat = _radius - ((((_radius >> 8) - 7) >> 1 << 8) + 512);
                int startX = cx - ((_radius >> 8) & 0xFF);
                int startY = cy - ((_radius >> 8) & 0xFF);
                for (int row = 0; row < squares; row++)
                    for (int column = 0; column < squares; column++)
                    {
                        int x = (startX + column) & 0xFF;
                        int y = (startY + row) & 0xFF;
                        int tile = Tile(x, y);
                        int distance = (int)GetDistance(X, Y, (short)(x << 8), (short)(y << 8));
                        if (distance < _radius)
                        {
                            int target = (int)((uint)(_height * (int)((uint)(0x10000 + MathsTables.Sin[0x200 + (distance << 10) / _radius]) >> 1)) >> 16) + _base;
                            int current = b.Height[tile];
                            int height = target > current ? (target - current) / Life + current : current;
                            b.SetTileHeight(x, y, height, distance <= flat);
                            if (b.IsCave)
                            {
                                int ceiling = Math.Min(height + 64, 254);
                                int old = b.Ceiling[tile];
                                if (ceiling > old)
                                    b.Ceiling[tile] = (byte)(old - (old - ceiling) / Life);
                            }
                        }
                        if (b.IsCave)
                        {
                            if (b.Ceiling[tile] > b.Height[tile])
                                b.Angle[tile] &= 0xF7;
                            else
                                b.Angle[tile] |= 8;
                        }
                    }
                if (Life == 3)
                {
                    b.SetCrater(cx, cy, _base + _height - 24 - 16);
                    // the eruption it makes (model 12h) is freed without running
                    b.Add(new SlotOnly());
                }
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateTerrainDeform_34910 (the Effect 52h, caves only): a room (2 x Parent by 2 x Child round its square):
        /// the floor down to 3 x Par3 below its lowest and the ceiling up to 3 x Par3 above its highest, then the ceiling
        /// roughened.
        /// </summary>
        private sealed class RoomCarver : MovingEntity
        {
            public RoomCarver(TerrainFeatureBuilder builder) : base(builder) { }
            public int HalfWidth;
            public int HalfHeight;
            public int Depth;
            public override bool Step()
            {
                var b = Builder;
                int depth = 3 * Depth;
                int width = 2 * HalfWidth;
                int height = 2 * HalfHeight;
                int startX = ((X >> 8) - HalfWidth) & 0xFF;
                int startY = ((Y >> 8) - HalfHeight) & 0xFF;
                int lowest = b.GetEdge(b.Height, startX, startY, width, height, false);
                int highest = b.GetEdge(b.Height, startX, startY, width, height, true);
                int floor = Clamp(lowest - depth, 0, 254);
                int ceiling = Clamp(depth + highest, 0, 254);
                for (int row = 0; row < height; row++)
                    for (int column = 0; column < width; column++)
                    {
                        int x = startX + column, y = startY + row;
                        int tile = Tile(x, y);
                        if (b.Height[tile] > floor)
                            b.SetTileHeight(x, y, floor, false, false);
                        if (ceiling > b.Ceiling[tile])
                            b.Ceiling[tile] = (byte)ceiling;
                        b.CloseCeiling(tile);
                    }
                b.MarkWaterEdgesFlat(startX - 1, startY - 1, width + 1, height + 1);
                b.RoughenCeiling(startX, startY, width, height);
                return true;
            }
        }

        /// <summary>
        /// remc2 UpdateArcProjectile_34540 (the Effect 51h, made by a line of 50h nodes in caves): a tunnel to its target
        /// in steps of 85, its radius going from the first node's Par3 to the next's, carved round the middle between the
        /// floor and ceiling 32 steps on.
        /// </summary>
        private sealed class Tunnel : MovingEntity
        {
            public Tunnel(TerrainFeatureBuilder builder) : base(builder) { }
            public short TargetX;
            public short TargetY;
            public sbyte Radii;
            private const int STEP = 85;

            private int Middle(short x, short y)
            {
                int tile = Tile((x >> 8) & 0xFF, (y >> 8) & 0xFF);
                return Clamp((Builder.Height[tile] + Builder.Ceiling[tile]) / 2, 0, 254);
            }

            public override bool Step()
            {
                var b = Builder;
                int startRadius = ((Radii >> 4) << 8) + 512;
                int endRadius = ((Radii & 0xF) << 8) + 512;
                int steps = (int)(GetDistance(X, Y, TargetX, TargetY) / STEP);
                ushort yaw = GetYaw(X, Y, TargetX, TargetY);
                var middles = new int[32];
                short aheadX = X, aheadY = Y;
                for (int i = 0; i < 32; i++)
                {
                    middles[i] = Middle(aheadX, aheadY);
                    Move(ref aheadX, ref aheadY, yaw, STEP);
                }
                short atX = X, atY = Y;
                for (int step = 0; step < steps; step++)
                {
                    int radius = startRadius + step * ((endRadius - startRadius) / steps);
                    int radius2 = radius * radius;
                    int size = (2 * radius + 128) >> 8;
                    int half = (2 * radius + 128) >> 9;
                    int cx = ((ushort)(atX + 128) >> 8) & 0xFF;
                    int cy = ((ushort)(atY + 128) >> 8) & 0xFF;
                    int middle = middles[0];
                    int startX = (cx - half) & 0xFF, startY = (cy - half) & 0xFF;
                    for (int row = 0; row < size; row++)
                        for (int column = 0; column < size; column++)
                        {
                            int x = (startX + column) & 0xFF, y = (startY + row) & 0xFF;
                            int tile = Tile(x, y);
                            int dx = Math.Abs(x - cx);
                            if (dx >= 0x80)
                                dx = Math.Abs(dx - 256);
                            int dy = Math.Abs(y - cy);
                            if (dy >= 0x80)
                                dy = Math.Abs(dy - 256);
                            int d2 = (dx << 8) * (dx << 8) + (dy << 8) * (dy << 8);
                            if (d2 <= radius2)
                            {
                                int span = (int)(IntegerSqrt((uint)(radius2 - d2)) >> 5);
                                int floor = Clamp(middle - span, 0, 254);
                                if (b.Height[tile] > floor)
                                    b.SetTileHeight(x, y, floor, false);
                                int ceiling = Clamp(span + middle, 0, 254);
                                if (b.Ceiling[tile] < ceiling)
                                    b.Ceiling[tile] = (byte)ceiling;
                            }
                            b.CloseCeiling(tile);
                        }
                    b.MarkWaterEdgesFlat(startX - 1, startY - 1, size + 1, size + 1);
                    Move(ref atX, ref atY, yaw, STEP);
                    Array.Copy(middles, 1, middles, 0, 31);
                    middles[31] = Middle(aheadX, aheadY);
                    Move(ref aheadX, ref aheadY, yaw, STEP);
                }
                return true;
            }
        }

        /// <summary>
        /// remc2 UpdateCraterEffect_34C40 (the Effect 53h, caves only): over 16 turns a mound up toward the ceiling round
        /// its square (radius in SwitchSize), the ceiling coming down to meet it.
        /// </summary>
        private sealed class CaveMound : MovingEntity
        {
            public CaveMound(TerrainFeatureBuilder builder) : base(builder) { }
            public int Radius;
            private int _state;
            private int _base;
            private int _top;

            public override bool Step()
            {
                var b = Builder;
                if (--Life <= 0)
                    return true;
                int size = 2 * Radius;
                int startX = ((((ushort)(X + 128)) >> 8) - Radius) & 0xFF;
                int startY = ((((ushort)(Y + 128)) >> 8) - Radius) & 0xFF;
                if (_state == 0)
                {
                    _base = b.GetEdge(b.Height, startX, startY, size, size, false);
                    _top = (short)b.GetEdge(b.Ceiling, startX, startY, size, size, true);
                    _state = (_top - _base <= 0 ? 1 : 0) + 1;
                    return false;
                }
                if (_state == 2)
                {
                    Life = 0;
                    return false;
                }
                int radius = Radius << 8;
                int flat = 192 * radius >> 8;
                int rise = _top - _base;
                for (int row = 0; row < size; row++)
                    for (int column = 0; column < size; column++)
                    {
                        int x = (startX + column) & 0xFF, y = (startY + row) & 0xFF;
                        int tile = Tile(x, y);
                        int distance = (int)GetDistance(X, Y, (short)(x << 8), (short)(y << 8));
                        if (distance >= radius)
                            continue;
                        int floor = b.Height[tile];
                        int ceiling = b.Ceiling[tile];
                        int cone = (int)((uint)rise * ((uint)(0x10000 + MathsTables.Sin[0x200 + (distance << 10) / radius]) >> 1) >> 16);
                        int target = Math.Min(cone + _base, 254);
                        if (target > floor)
                            b.SetTileHeight(x, y, (int)((uint)(target - floor) / (uint)Life) + floor, distance <= flat);
                        int down = Math.Max(_top - cone, 0);
                        if (down < ceiling)
                            b.Ceiling[tile] = (byte)(ceiling - (int)((uint)(ceiling - down) / (uint)Life));
                        if (b.Ceiling[tile] > b.Height[tile])
                            b.Angle[tile] &= 0xF7;
                        else
                            b.Angle[tile] |= 8;
                    }
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateVolcanoEffect_34EE0 (the Effects 54h and 55h, caves only): over 16 turns a cone round its square
        /// (radius in SwitchSize): 54h hangs the ceiling down, 55h raises the floor, by Par3 fifths of the room there
        /// (or at random with none); 54h roughens the ceiling at the end.
        /// </summary>
        private sealed class CaveCone : MovingEntity
        {
            public CaveCone(TerrainFeatureBuilder builder) : base(builder) { }
            public bool FromCeiling;
            public int Radius;
            public short Z;
            private int _state;
            private int _start;

            public override bool Step()
            {
                var b = Builder;
                if (--Life <= 0)
                    return true;
                int size = 2 * Radius;
                int startX = (((X + 128) >> 8) - Radius) & 0xFF;
                int startY = (((Y + 128) >> 8) - Radius) & 0xFF;
                if (_state == 0)
                {
                    int start, room;
                    if (FromCeiling)
                    {
                        start = b.GetEdge(b.Ceiling, startX, startY, size, size, true);
                        room = start - b.GetEdge(b.Height, startX, startY, size, size, true) - 1;
                    }
                    else
                    {
                        start = b.GetEdge(b.Height, startX, startY, size, size, false);
                        room = b.GetEdge(b.Ceiling, startX, startY, size, size, false) - start - 1;
                    }
                    if (room <= 0)
                    {
                        _state = 2;
                        return false;
                    }
                    int length = Z != 0 ? 51 * Z * room / 256 : (int)(b.NextRand(this) % (uint)room);
                    _start = start;
                    Z = (short)(FromCeiling ? start - length : length + start);
                    _state = 1;
                    return false;
                }
                if (_state == 2)
                {
                    if (FromCeiling)
                        b.RoughenCeiling(startX, startY, size, size);
                    Life = 0;
                    return false;
                }
                int radius = Radius << 8;
                int flat = 49152 * Radius >> 8;
                int length2 = Math.Abs(Z - _start);
                for (int row = 0; row < size; row++)
                    for (int column = 0; column < size; column++)
                    {
                        int x = (startX + column) & 0xFF, y = (startY + row) & 0xFF;
                        int tile = Tile(x, y);
                        int distance = (int)GetDistance(X, Y, (short)(x << 8), (short)(y << 8));
                        if (distance >= radius)
                            continue;
                        int cone = (int)((uint)length2 * ((uint)(0x10000 + MathsTables.Sin[0x200 + (distance << 10) / radius]) >> 1) >> 16);
                        if (FromCeiling)
                        {
                            int target = Math.Max(_start - cone, 0);
                            int ceiling = b.Ceiling[tile];
                            int step = (ceiling - target) / Life;
                            if (ceiling - step < ceiling)
                                b.Ceiling[tile] = (byte)(ceiling - step);
                        }
                        else
                        {
                            int target = Math.Min(_start + cone, 254);
                            int floor = b.Height[tile];
                            int step = (target - floor) / Life;
                            if (step + floor > floor)
                                b.SetTileHeight(x, y, step + floor, distance <= flat);
                        }
                        b.CloseCeiling(tile);
                    }
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateCastleBuilding_5B100 (the Special 2, a cave door, caves only), the one step it takes before the
        /// level starts: its height half way between the closed rock either side (across with Parent, else along y), and
        /// its 2 by (2 Par3 + 4) squares marked as set by hand (80h). It then waits in its slot; with no rock to find it
        /// is freed.
        /// </summary>
        private sealed class CaveDoor : MovingEntity
        {
            public CaveDoor(TerrainFeatureBuilder builder) : base(builder) { }
            public bool Across;
            public int Size;
            public override bool FreedWhenDone => _freed;
            private bool _freed;

            public override bool Step()
            {
                var b = Builder;
                int length = (2 * ((Size << 8) + 512) + 128) >> 8;
                int width = Across ? length : 2;
                int height = Across ? 2 : length;
                int x = (X >> 8) & 0xFF;
                int y = (Y >> 8) & 0xFF;
                // the game searches the row or column until it finds rock (for ever with none)
                if (!FindRock(x, y, -1) || !FindRock(x, y, 1))
                {
                    _freed = true;
                    return true;
                }
                int startX = x - (width >> 1), startY = y - (height >> 1);
                for (int row = 0; row < height; row++)
                    for (int column = 0; column < width; column++)
                        b.Angle[Tile(startX + column, startY + row)] |= 0x80;
                return true;
            }

            private bool FindRock(int x, int y, int step)
            {
                for (int i = 0; i < 256; i++, x += Across ? step : 0, y += Across ? 0 : step)
                    if ((Builder.Angle[Tile(x, y)] & 8) != 0)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// remc2 UpdateCastle_59F60 (the Special 1, a gate), its first update as the level starts: a closed gate, a wall
        /// of Child + 1 squares, 2 thick, along x (Parent 0) or y (Parent 1) from its square, raised 48 like a wall, its
        /// squares wall (type 8) and the area round it marked as set by hand. Squares are counted as the original does,
        /// in 16-bit map indexes (a step along x past the edge runs into the next row).
        /// </summary>
        protected void BuildGate(int x, int y, int direction, int length)
        {
            int count = length + 1;
            ushort centre = (ushort)Tile(x, y);
            if (direction != 0 && direction != 1)
                return;
            if (direction == 0)
            {
                ushort start = (ushort)Tile(x - 1, y);
                ushort column = start;
                for (int i = 0; i < count; i++, column++)
                {
                    ushort tile = column;
                    for (int k = 0; k < 2; k++, tile = Up(tile))
                        if (Type[tile] != 8 || Math.Abs(Height[tile] - Height[(ushort)(tile + 1)]) > 30)
                            Height[tile] = (byte)(Height[tile] + 48);
                }
                column = (ushort)(start - 1);
                for (int i = -1; i < count; i++, column++)
                {
                    ushort tile = column;
                    for (int k = 0; k < 3; k++, tile = Up(tile))
                    {
                        Type[tile] = 8;
                        Angle[tile] = 1;
                    }
                }
                column = (ushort)(start - 1);
                for (int i = -1; i < count + 1; i++, column++)
                {
                    ushort tile = (ushort)(column + 256);
                    for (int k = 0; k < 4; k++, tile = Up(tile))
                        OpenOrCloseAfterGate(tile);
                }
                column = (ushort)(start - 1);
                for (int i = -1; i < count; i++, column++)
                    ShadeGateTile(column);
                column = (ushort)(start - 3);
                for (int i = -3; i < count + 3; i++, column++)
                {
                    ushort tile = (ushort)(column + 768);
                    for (int k = -3; k < 6; k++, tile = Up(tile))
                        Angle[tile] |= 0x80;
                }
            }
            else
            {
                ushort start = (ushort)Tile(x, y - 1);
                ushort row = start;
                for (int k = 0; k < 2; k++, row--)
                {
                    ushort tile = row;
                    for (int i = 0; i < count; i++, tile = Down(tile))
                    {
                        ushort below = (ushort)((row & 0xFF) | (((tile >> 8) + 1) & 0xFF) << 8);
                        if (Type[tile] != 8 || Math.Abs(Height[tile] - Height[below]) > 30)
                            Height[tile] = (byte)(Height[tile] + 48);
                    }
                }
                row = start;
                for (int k = 0; k < 3; k++, row--)
                {
                    ushort tile = (ushort)(row - 256);
                    for (int i = -1; i < count; i++, tile = Down(tile))
                    {
                        Type[tile] = 8;
                        Angle[tile] = 1;
                    }
                }
                if (IsCave)
                {
                    // across by the column only
                    ushort first = (ushort)(start + 1);
                    for (int k = 0; k < 4; k++, first = (ushort)((first & 0xFF00) | ((first - 1) & 0xFF)))
                    {
                        ushort tile = (ushort)(first - 256);
                        for (int i = -1; i < count + 1; i++, tile = Down(tile))
                            CloseCeiling(tile);
                    }
                }
                else
                {
                    ushort first = (ushort)(start + 1);
                    for (int k = 0; k < 4; k++, first--)
                    {
                        ushort tile = (ushort)(first - 256);
                        for (int i = -1; i < count + 1; i++, tile = Down(tile))
                            Angle[tile] &= 0xF7;
                    }
                }
                ushort centreTile = (ushort)(start - 256);
                for (int i = -1; i < count; i++, centreTile = Down(centreTile))
                    ShadeGateTile(centreTile);
                ushort columnStart = (ushort)(start - 3);
                for (int k = -3; k < 6; k++, columnStart++)
                {
                    ushort tile = (ushort)(columnStart - 768);
                    for (int i = -3; i < count + 3; i++, tile = Down(tile))
                        Angle[tile] |= 0x80;
                }
            }
        }

        private static ushort Up(ushort tile) => (ushort)((tile & 0xFF) | ((((tile >> 8) - 1) & 0xFF) << 8));

        private static ushort Down(ushort tile) => (ushort)((tile & 0xFF) | ((((tile >> 8) + 1) & 0xFF) << 8));

        private void OpenOrCloseAfterGate(ushort tile)
        {
            if (IsCave)
                CloseCeiling(tile);
            else
                Angle[tile] &= 0xF7;
        }

        /// <summary>The gate's own shading: as a wall's, but from the full difference (not a byte).</summary>
        private void ShadeGateTile(ushort tile)
        {
            int x = tile & 0xFF, y = tile >> 8;
            int shade = Height[Tile(x - 1, y - 1)] - Height[Tile(x + 1, y + 1)] + 32;
            if (shade >= 28)
            {
                if (shade > 40)
                    shade = (shade & 7) + 40;
            }
            else
                shade = (shade & 3) + 28;
            Shading[tile] = _isNight ? (byte)(32 - shade + 32) : (byte)shade;
        }

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        /// <summary>In caves: where the ceiling is not above the floor it comes down to the floor (closed, angle bit 8).</summary>
        protected void CloseCeiling(int tile)
        {
            if (Ceiling[tile] > Height[tile])
                Angle[tile] &= 0xF7;
            else
            {
                Ceiling[tile] = (byte)(Height[tile] - 1);
                Angle[tile] |= 8;
            }
        }

        /// <summary>
        /// remc2 UpdateTileAngles_34B00: round the rectangle from (x, y), the squares marked closed (8) get the mark 1 and
        /// their types made again (the top row and left column also the type 1 first).
        /// </summary>
        private void MarkWaterEdgesFlat(int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                OpenSquare(x + i, y, true);
                OpenSquare(x + i, y + height, false);
            }
            for (int i = 0; i < height; i++)
            {
                OpenSquare(x, y + i, true);
                OpenSquare(x + width, y + i, false);
            }
        }

        private void OpenSquare(int x, int y, bool resetType)
        {
            int tile = Tile(x, y);
            if ((Angle[tile] & 8) == 0)
                return;
            Angle[tile] = (byte)((Angle[tile] & 0xF8) | 1);
            if (resetType)
                Type[tile] = 1;
            UpdateTerrainTiles(x, y, x, y);
        }

        /// <summary>
        /// remc2 RandomiseTerrainArea_43C60: the open ceiling of <paramref name="rows"/> rows of <paramref name="columns"/>
        /// squares from (x, y) moves -3 to 3 at random (the same sequence each time), then is closed where it meets the floor.
        /// </summary>
        private void RoughenCeiling(int x, int y, int rows, int columns)
        {
            uint rand = 37487429;
            for (int row = 0; row < (ushort)rows; row++)
                for (int column = 0; column < (ushort)columns; column++)
                {
                    int tile = Tile(x + column, y + row);
                    if ((Angle[tile] & 8) != 0)
                        continue;
                    rand = unchecked(9377 * rand + 9439);
                    Ceiling[tile] = (byte)(Ceiling[tile] + (int)(rand % 7) - 3);
                }
            for (int row = 0; row < (ushort)rows; row++)
                for (int column = 0; column < (ushort)columns; column++)
                    CloseCeiling(Tile(x + column, y + row));
        }

        /// <summary>The middle 2 x 2 squares of a volcano: the crater floor, shaded dark.</summary>
        private void SetCrater(int cx, int cy, int height)
        {
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 2; column++)
                {
                    int tile = Tile(cx - 1 + column, cy - 1 + row);
                    Height[tile] = (byte)height;
                    Shading[tile] = _isNight ? (byte)1 : (byte)63;
                }
        }

        /// <summary>
        /// remc2 GetMinHeightmapInArea_48F20: the lowest height (at most 250) on the edge of the area: its top row and
        /// the row below it, its left column and the column after it.
        /// </summary>
        private int GetMinHeightOnEdge(int x, int y, int height, int width)
        {
            int result = 250;
            for (int i = 0; i < (ushort)width; i++)
            {
                result = Math.Min(result, Height[Tile(x + i, y)]);
                result = Math.Min(result, Height[Tile(x + i, y + height)]);
            }
            for (int i = 0; i < (ushort)height; i++)
            {
                result = Math.Min(result, Height[Tile(x + width, y + i)]);
                result = Math.Min(result, Height[Tile(x + width - width, y + i)]);
            }
            return result;
        }

        /// <summary>
        /// remc2 FindFreeTileInArea_570F0 (on any square, the water mark at 0): the square's height within 0-255; the
        /// mark 1 where <paramref name="flat"/> or the type takes spells, and its terrain types made again.
        /// </summary>
        /// <summary>
        /// remc2 GetMin/MaxHeightmapInArea_48F20/48FD0 (floor or ceiling): the lowest (from 250) or highest (from 0) value
        /// on the edge: <paramref name="width"/> squares of the top row and of the row <paramref name="height"/> below,
        /// <paramref name="height"/> squares of the left column and of the column <paramref name="width"/> on.
        /// </summary>
        protected int GetEdge(byte[] map, int x, int y, int height, int width, bool max)
        {
            int result = max ? 0 : 250;
            int Pick(int value) => max ? Math.Max(result, value) : Math.Min(result, value);
            for (int i = 0; i < (ushort)width; i++)
            {
                result = Pick(map[Tile(x + i, y)]);
                result = Pick(map[Tile(x + i, y + height)]);
            }
            for (int i = 0; i < (ushort)height; i++)
            {
                result = Pick(map[Tile(x + width, y + i)]);
                result = Pick(map[Tile(x, y + i)]);
            }
            return result;
        }

        private void SetTileHeight(int x, int y, int height, bool flat) => SetTileHeight(x, y, height, flat, true);

        /// <param name="waterOnlyWhenFree">With false, a square at 0 always gets the water mark.</param>
        private void SetTileHeight(int x, int y, int height, bool flat, bool waterOnlyWhenFree)
        {
            int tile = Tile(x, y);
            if (height > 255)
                height = 255;
            if (height < 0)
                height = 0;
            Height[tile] = (byte)height;
            if (flat || IsSpellCastable(Type[tile]))
                Angle[tile] = (byte)((Angle[tile] & 0xF8) | 1);
            if (height == 0 && (!waterOnlyWhenFree || IsFreeOfFeaturesAround(x, y)))
                Angle[tile] &= 0xF0;
            UpdateTerrainTiles(x, y, x, y, false);
        }

        /// <summary>Whether none of the 8 squares around has the corner marks 2, 3 or 5 (remc2 IsTileFreeOfFeatures_56EE0).</summary>
        private bool IsFreeOfFeaturesAround(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    int mark = Angle[Tile(x + dx, y + dy)] & 7;
                    if (mark == 5 || mark == 2 || mark == 3)
                        return false;
                }
            return true;
        }

        /// <summary>remc2 IsSpellCastable_57450: the terrain types a spell may change.</summary>
        private static bool IsSpellCastable(byte type) =>
            type == 0 || type == 0x25 || type == 0x26 || (type >= 0x2C && type <= 0x2F) || type == 0x51 || type == 0x53
            || type == 0x68 || type == 0x69 || type == 0x6D || type == 0x72 || type == 0x74;

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

        protected TerrainFeatureBuilder(BuildingShapes shapes, SearchRings search)
        {
            Shapes = shapes;
            _search = search;
        }

        /// <summary>
        /// A copy of <paramref name="generated"/> with the level's features built into it. <paramref name="generated"/>
        /// itself is not changed, and is returned if it was not generated in full or nothing changes it.
        /// </summary>
        public Terrain Build(Terrain generated, IEnumerable<LevelFeature> features)
        {
            var list = features?.Where(f => f != null).Select(f => f.Copy()).OrderBy(f => f.Id).ToList() ?? new List<LevelFeature>();
            if (!list.Any(f => ShapesTerrain(f.Model) && (f.DisId == LevelFeature.NO_SWITCH || f.Model == GATE && f.DisId == 0 || f.Model == CASTLE)) || generated?.AngleTypes == null
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
            _slots = new FeatureEntity[MAX_ENTITIES];
            _worldRand = terrain.GenerationParameters?.Seed ?? 0;
            SetStageTags(list);

            foreach (var pass in GetPasses())
            {
                // The features of a pass make their entities (buildings take their base height then), then those run
                foreach (var feature in list)
                {
                    if (feature.Built || feature.DisId != LevelFeature.NO_SWITCH || !pass(feature))
                        continue;
                    Prepare(feature);
                    feature.Built = true;
                }
                Run();
            }

            // then the wizards' castles are put up, and as the level starts the gates close (their first update)
            BuildCastles(list.Where(f => f.Model == CASTLE).ToList());
            if (BuildsGates)
                foreach (var gate in list.Where(f => f.Model == GATE && f.DisId == 0))
                    // spawned in the middle of its square, which the gate rounds up a square more
                    BuildGate((gate.X + 1) & 0xFF, (gate.Y + 1) & 0xFF, gate.Parent, gate.Child);

            terrain.Seed = _seed;
            return terrain;
        }

        /// <summary>
        /// The groups of features (by their model) in the order the game builds them; each is built in turn, the
        /// entities running between. A group may have nothing built here (the game runs its entities all the same).
        /// </summary>
        protected abstract IEnumerable<Func<LevelFeature, bool>> GetPasses();

        /// <summary>The wizards' castles at their start level, as the wizards are set up (Magic Carpet 2 only).</summary>
        protected virtual void BuildCastles(IList<LevelFeature> castles)
        {
        }

        /// <summary>Whether the level's gates (Special 1) close as it starts: Magic Carpet 2 only.</summary>
        protected virtual bool BuildsGates => false;

        /// <summary>Before the passes: the stage tags lines are made from (Magic Carpet 2 sets them on all line nodes).</summary>
        protected virtual void SetStageTags(IList<LevelFeature> features)
        {
        }

        /// <summary>The bits of an entity's random seed: 16 in Magic Carpet 2, 32 in Magic Carpet.</summary>
        protected abstract uint RandMask { get; }

        /// <summary>Whether an entity's index goes on one each turn (in Magic Carpet it stays its slot before the level starts).</summary>
        protected abstract bool CountsTurns { get; }

        private uint NextRand(FeatureEntity entity)
        {
            entity.Rand = unchecked(entity.Rand * 9377 + 9439) & RandMask;
            return entity.Rand;
        }

        /// <summary>
        /// remc2 ApplyEvents_498A0 / remc1 InitCreatedEntities_36620: the world's seed takes a step, then each turn every
        /// entity takes a step in the order of the slots (one made into a later slot that turn takes its step too),
        /// until none is left to; a done entity is freed (its slot goes on top of the free ones).
        /// </summary>
        private void Run()
        {
            _worldRand = unchecked(_worldRand * 9377 + 9439);
            bool again = true;
            while (again)
            {
                again = false;
                for (int slot = 1; slot < MAX_ENTITIES; slot++)
                {
                    var entity = _slots[slot];
                    if (entity == null || entity.Finished)
                        continue;
                    again = true;
                    bool done = entity.Step();
                    if (CountsTurns)
                        entity.Index++;
                    if (!done)
                        continue;
                    if (entity.FreedWhenDone)
                    {
                        _slots[slot] = null;
                        _freeSlots.Push(slot);
                    }
                    else
                        entity.Finished = true;
                }
            }
        }

        /// <summary>remc2 NewEvent_4A050 / remc1 AllocEntity_372C0: the entity takes the free slot on top.</summary>
        private void Add(FeatureEntity entity)
        {
            if (entity == null || _freeSlots.Count == 0)
                return;
            entity.Slot = _freeSlots.Pop();
            entity.Index = (byte)entity.Slot;
            entity.Rand = unchecked((uint)entity.Slot + _worldRand) & RandMask;
            _slots[entity.Slot] = entity;
        }

        /// <summary>remc2 PrepareEvents_49540 / remc1 CreateFeature_36480: the entities of one feature.</summary>
        private void Prepare(LevelFeature feature)
        {
            switch (feature.Model)
            {
                case WALL:
                case PATH:
                case CANYON:
                case RIDGE:
                case TUNNEL:
                    // a line of nodes is made once, from the first node with a stage tag (SwitchId)
                    if (feature.SwitchId != 0)
                        BuildChain(feature);
                    break;
                case TUNNEL_CARVER:
                    // on its own: toward (0, 0), from 512 to 1024 wide
                    if (IsCave)
                        Add(new Tunnel(this) { X = (short)(feature.X << 8), Y = (short)(feature.Y << 8), Radii = 2 });
                    break;
                case ROOM:
                    if (IsCave)
                        Add(new RoomCarver(this)
                        {
                            X = (short)(feature.X << 8),
                            Y = (short)(feature.Y << 8),
                            HalfWidth = (sbyte)feature.Parent,
                            HalfHeight = (sbyte)feature.Child,
                            Depth = (sbyte)feature.Par3
                        });
                    break;
                case CAVE_DOOR:
                    if (IsCave)
                        Add(new CaveDoor(this) { X = (short)(feature.X << 8), Y = (short)(feature.Y << 8), Across = feature.Parent != 0, Size = feature.Par3 });
                    break;
                case CAVE_MOUND:
                    if (IsCave)
                        Add(new CaveMound(this) { X = (short)(feature.X << 8), Y = (short)(feature.Y << 8), Radius = (short)feature.SwitchSize, Life = 16 });
                    break;
                case CEILING_CONE:
                case FLOOR_CONE:
                    if (IsCave)
                        Add(new CaveCone(this)
                        {
                            X = (short)(feature.X << 8),
                            Y = (short)(feature.Y << 8),
                            FromCeiling = feature.Model == CEILING_CONE,
                            Radius = (short)feature.SwitchSize,
                            Z = (short)feature.Par3,
                            Life = 16
                        });
                    break;
                case WALL_SECTION:
                    {
                        int x = feature.X, y = feature.Y;
                        Add(new ActionEntity(() => RaiseWallYMinus(x, y, WALL_SECTION_LENGTH, WALL_THICKNESS)));
                        break;
                    }
                case CANYON_MOVER:
                    // on its own it has no life: one dig where it stands
                    Add(new CanyonMover(this) { X = (short)(feature.X << 8), Y = (short)(feature.Y << 8) });
                    break;
                case RIDGE_MOVER:
                    Add(new RidgeMover(this) { X = (short)(feature.X << 8), Y = (short)(feature.Y << 8) });
                    break;
                case BUILDING:
                    Add(Place(feature));
                    break;
                case PATH_POINT:
                    Add(new SlotOnly());
                    break;
                case VOLCANO:
                case CRATER:
                case EMBER:
                    {
                        // their size or life is the spell's at the level in Parent
                        var life = GetSpellLife(feature.Model, feature.Parent);
                        if (life == null)
                            break;
                        short x = (short)(feature.X << 8), y = (short)(feature.Y << 8);
                        if (feature.Model == VOLCANO)
                            Add(new Volcano(this) { X = x, Y = y, Size = life.Value, Life = 17 });
                        else if (feature.Model == CRATER)
                            Add(new Dig(this) { X = x, Y = y, Life = life.Value });
                        else
                        {
                            var ember = new Ember(this) { X = x, Y = y, Life = life.Value };
                            Add(ember);
                            ember.Yaw = (ushort)(NextRand(ember) & 0x7FF);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// The spell's value at <paramref name="level"/> (0-2) for a volcano (its radius), a crater or an ember (their
        /// life); null where the game does not build it here.
        /// </summary>
        protected virtual int? GetSpellLife(int model, int level) => null;

        /// <summary>remc2 MoveEntity_57FA0: a step of <paramref name="speed"/> toward <paramref name="yaw"/> (800h a turn, 0 up the map).</summary>
        private static void Move(ref short x, ref short y, ushort yaw, int speed)
        {
            int angle = yaw & 0x7FF;
            x = unchecked((short)(x + ((speed * MathsTables.Sin[angle]) >> 16)));
            y = unchecked((short)(y - ((speed * MathsTables.Sin[0x200 + angle]) >> 16)));
        }

        /// <summary>remc2 GetYawAngle_581E0.</summary>
        private static ushort GetYaw(short fromX, short fromY, short toX, short toY) =>
            Atan2(unchecked((short)(toX - fromX)), unchecked((short)(toY - fromY)));

        /// <summary>remc2 Atan2_72633.</summary>
        private static ushort Atan2(short a1, short a2)
        {
            var atan = MathsTables.Atan;
            if (a1 == 0 && a2 == 0)
                return 0;
            int result;
            if (a1 < 0)
            {
                if (a2 < 0)
                    result = -a1 < -a2 ? 0x800 - atan[(-a1 << 8) / -a2] : atan[(-a2 << 8) / -a1] + 0x600;
                else if (-a1 < a2)
                    result = atan[(-a1 << 8) / a2] + 0x400;
                else
                    result = 0x600 - atan[(a2 << 8) / -a1];
            }
            else if (a2 < 0)
                result = a1 < -a2 ? atan[(a1 << 8) / -a2] : 0x200 - atan[(-a2 << 8) / a1];
            else if (a1 < a2)
                result = 0x400 - atan[(a1 << 8) / a2];
            else
                result = atan[(a2 << 8) / a1] + 0x200;
            return (ushort)result;
        }

        /// <summary>remc2 DistanceXY_58490.</summary>
        private static uint GetDistance(short fromX, short fromY, short toX, short toY)
        {
            int dx = unchecked((short)(toX - fromX));
            int dy = unchecked((short)(toY - fromY));
            return IntegerSqrt((uint)(dx * dx + dy * dy));
        }

        /// <summary>remc2 IntegerSqrt_7277A: Newton's steps from a guess by the highest bit.</summary>
        private static uint IntegerSqrt(uint value)
        {
            if (value == 0)
                return 0;
            int bit = 31;
            while ((value & (1u << bit)) == 0)
                bit--;
            uint i = MathsTables.SqrtGuess[bit];
            while ((int)(value / i) < (int)i)
                i = (value / i + i) >> 1;
            return i;
        }

        /// <summary>
        /// remc2 GetTileAngleBit_104A0 &amp; 1: whether the square is flat water (its corner mark 0). The square is taken as
        /// the original takes it from signed positions, in a 16-bit index: past the middle of the map across, it is a row up.
        /// </summary>
        private bool IsWater(short x, short y)
        {
            int index = ((x >> 8) + ((y >> 8) << 8)) & 0xFFFF;
            return (Angle[index] & 0x0F) == 0;
        }

        /// <summary>
        /// remc2 SpawnAroundEntity_572C0 / SpawnEffectAroundTile_31F00, remc1 ChangeHeightsAround_40D30 /
        /// LowerHeightsAround_255D0: changes the heights of the search rings first..last (at most the radius) around the
        /// square of (x, y); with <paramref name="stopWhenClamped"/> it stops at the square (0, 0) when that is clamped.
        /// </summary>
        private void ChangeHeightsAround(short x, short y, int radius, int first, int last, int add, bool stopWhenClamped)
        {
            int tileX = (x + 128) >> 8;
            int tileY = (y + 128) >> 8;
            if (last > radius)
                last = radius;
            var search = _search.Start(first, last);
            while (search.Next(out int dx, out int dy))
            {
                if (ChangeTileHeight(tileX + dx, tileY + dy, add) && stopWhenClamped)
                    return;
            }
        }

        /// <summary>
        /// remc2 FindFreeTileNear_56F10 / remc1 ChangeTileHeight_40A10 (not on fixed squares): the height goes by
        /// <paramref name="add"/> within 0-200, the square gets the mark 1, or at 0 the water mark when no feature is
        /// next to it, and its terrain types are made again. True when the square (0, 0) was clamped.
        /// </summary>
        private bool ChangeTileHeight(int x, int y, int add)
        {
            int tile = Tile(x, y);
            bool origin = (short)x == 0 && (short)y == 0;
            bool clamped = false;
            int height = (short)(Height[tile] + add);
            if (height > 200)
            {
                height = 200;
                clamped = origin;
            }
            if (height < 0)
            {
                height = 0;
                clamped = origin;
            }
            Height[tile] = (byte)height;
            if (IsCave)
            {
                int ceiling = Ceiling[tile] - add;
                Ceiling[tile] = ceiling >= 255 ? (byte)255 : (byte)ceiling;
            }
            if (height != 0)
                Angle[tile] = (byte)((Angle[tile] & 0xF8) | 1);
            else
            {
                bool free = true;
                for (int dy = -1; dy <= 1 && free; dy++)
                    for (int dx = -1; dx <= 1 && free; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        int mark = Angle[Tile(x + dx, y + dy)] & 7;
                        free = mark != 5 && mark != 2 && mark != 3;
                    }
                if (free)
                    Angle[tile] &= 0xF0;
            }
            UpdateTerrainTiles(x, y, x, y, false);
            return clamped;
        }

        /// <summary>How deep the digs of a canyon line from <paramref name="node"/> go (their life).</summary>
        protected virtual int GetCanyonDepth(LevelFeature node) => DEFAULT_DIG_DEPTH;

        /// <summary>
        /// remc2 CreateSubtype32MovingToPoint_487D0 / remc1 CreateFeatureLine1F_35D30: a canyon mover from (x, y)
        /// toward (x2, y2), a square a turn for the distance.
        /// </summary>
        private void AddCanyonLine(int x, int y, int x2, int y2, int depth)
        {
            var mover = new CanyonMover(this) { X = (short)(x << 8), Y = (short)(y << 8), Depth = depth };
            mover.Yaw = GetYaw(mover.X, mover.Y, (short)(x2 << 8), (short)(y2 << 8));
            mover.Life = (ushort)GetDistance(mover.X, mover.Y, (short)(x2 << 8), (short)(y2 << 8)) >> 8;
            Add(mover);
        }

        /// <summary>
        /// remc2 CreateSubtype51MovingToPoint_48880 / remc1 CreateFeatureLine32_35DE0: a ridge mover from (x, y)
        /// toward (x2, y2), 4 squares a turn for the distance.
        /// </summary>
        private void AddRidgeLine(int x, int y, int x2, int y2)
        {
            var mover = new RidgeMover(this) { X = (short)(x << 8), Y = (short)(y << 8) };
            mover.Yaw = GetYaw(mover.X, mover.Y, (short)(x2 << 8), (short)(y2 << 8));
            mover.Life = (ushort)GetDistance(mover.X, mover.Y, (short)(x2 << 8), (short)(y2 << 8)) / RIDGE_SPEED;
            Add(mover);
        }

        /// <summary>
        /// remc2 CreateLinkedLevelFeatures_49090 / remc1 CreateFeatureChain_362C0: back along Parent to the first node,
        /// then a line from each node to the next (Child) while they are of the same kind. Every node's stage tag is
        /// cleared so the line is made once.
        /// </summary>
        private void BuildChain(LevelFeature feature)
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
                switch (feature.Model)
                {
                    case WALL:
                        AddWallLine(current.X, current.Y, next.X, next.Y);
                        break;
                    case PATH:
                        AddPathLine(current.X, current.Y, next.X, next.Y);
                        break;
                    case CANYON:
                        AddCanyonLine(current.X, current.Y, next.X, next.Y, GetCanyonDepth(current));
                        break;
                    case TUNNEL:
                        // remc2 CreateSubtype81WithTarget_48930: the radius from this node's Par3 (high nibble) to the next's
                        if (IsCave)
                            Add(new Tunnel(this)
                            {
                                X = (short)(current.X << 8),
                                Y = (short)(current.Y << 8),
                                TargetX = (short)(next.X << 8),
                                TargetY = (short)(next.Y << 8),
                                Radii = (sbyte)(((current.Par3 & 0xF) << 4) | (next.Par3 & 0xF))
                            });
                        break;
                    default:
                        AddRidgeLine(current.X, current.Y, next.X, next.Y);
                        break;
                }
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
        private void AddWallLine(int x, int y, int x2, int y2)
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
                    AddWallSection(x, y, WallDirection.X, stepX + restX);
                    x += stepX + restX;
                    AddYWallSection(x, y, stepY, restY);
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
                    AddYWallSection(x, y, stepY, restY);
                    y += stepY + restY;
                    AddWallSection(x, y, WallDirection.X, stepX + restX);
                    x += stepX + restX;
                    restX = 0;
                    restY = 0;
                }
            }
        }

        private void AddYWallSection(int x, int y, int stepY, int restY)
        {
            if (stepY < 0)
                AddWallSection(x, y, WallDirection.YMinus, -stepY - restY);
            else
                AddWallSection(x, y, WallDirection.YPlus, stepY + restY);
        }

        private void AddWallSection(int x, int y, WallDirection direction, int length)
        {
            x &= 0xFF;
            y &= 0xFF;
            switch (direction)
            {
                case WallDirection.YMinus:
                    Add(new ActionEntity(() => RaiseWallYMinus(x, y, length, WALL_THICKNESS)));
                    break;
                case WallDirection.YPlus:
                    Add(new ActionEntity(() => RaiseWallYPlus(x, y, length, WALL_THICKNESS)));
                    break;
                default:
                    Add(new ActionEntity(() => RaiseWallX(x, y, length, WALL_THICKNESS)));
                    break;
            }
        }

        /// <summary>
        /// remc2 CreateSubtype30LineBetweenPoints_48690 / remc1 CreateFeatureLine1D_35BF0: the path from (x, y) to
        /// (x2, y2) as a diagonal run, then a straight one.
        /// </summary>
        private void AddPathLine(int x, int y, int x2, int y2)
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
            Add(new ActionEntity(() => MarkPath(startX, startY, signX, signY, diagonal)));
            int midX = (x + signX * diagonal) & 0xFF, midY = (y + signY * diagonal) & 0xFF;
            Add(new ActionEntity(() => MarkPath(midX, midY, straightX, straightY, straight)));
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
        protected void UpdateTerrainTiles(int fromX, int fromY, int toX, int toY, bool keepFixed = true)
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
                    ResetType(Tile(x, y), keepFixed);
                    ResetType(Tile(x - 1, y), keepFixed);
                    ResetType(Tile(x - 1, y - 1), keepFixed);
                    ResetType(Tile(x, y - 1), keepFixed);
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

        /// <summary>
        /// The square's type is made again (1 until it is), unless it was set by hand (80h) and is kept: remc2
        /// AddBuildingToTerrain_46570 / remc1 RebuildTerrainTiles_33E10 make them all again.
        /// </summary>
        private void ResetType(int tile, bool keepFixed)
        {
            if (!keepFixed || (Angle[tile] & 0x80) == 0)
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
