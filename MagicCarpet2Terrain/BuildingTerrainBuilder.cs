using MagicCarpet2Terrain.Model;
using System.Collections.Generic;
using System.Linq;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Builds the level's buildings into generated terrain, as the games do before a level starts: each building
    /// (an Effect 2Dh with no switch, DisId FFFFh) stamps its shape from DATA/BUILD?-0 into the heights and terrain
    /// types over 30 steps, then smooths the ground around it. The buildings of a pass take their steps in turn.
    /// Ported from remc2 GenerateEvents_49290 / ApplyTerrainModification_37240 and remc1 CreateFeatures_36430 /
    /// BuildBuildingAction_27D30. Other terrain features (paths, walls, canyons, ridges) are not built.
    /// </summary>
    public abstract class BuildingTerrainBuilder
    {
        private const int BUILD_LIFE = 30;

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

        protected class Building
        {
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
        }

        protected BuildingTerrainBuilder(BuildingShapes shapes)
        {
            Shapes = shapes;
        }

        /// <summary>
        /// A copy of <paramref name="generated"/> with the buildings built into it. <paramref name="generated"/> is
        /// returned unchanged if it was not generated in full or has no buildings to build.
        /// </summary>
        public Terrain Build(Terrain generated, IEnumerable<BuildingPlacement> placements)
        {
            var list = placements?.ToList() ?? new List<BuildingPlacement>();
            if (list.Count == 0 || Shapes == null || generated?.AngleTypes == null
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

            foreach (var pass in GetPasses(list))
            {
                // All of a pass are placed (and take their base height) before any is built
                var buildings = pass.Select(Place).Where(b => b != null).ToList();
                bool again = true;
                while (again)
                {
                    again = false;
                    foreach (var building in buildings.Where(b => !b.Done))
                    {
                        Step(building);
                        again = true;
                    }
                }
            }

            terrain.Seed = _seed;
            return terrain;
        }

        /// <summary>The groups of buildings in the order the game builds them; each group is built in turn.</summary>
        protected virtual IEnumerable<IList<BuildingPlacement>> GetPasses(IList<BuildingPlacement> placements)
        {
            yield return placements;
        }

        /// <summary>The index in <see cref="Shapes"/> of the placement's shape.</summary>
        protected abstract int GetShapeIndex(BuildingPlacement placement);

        /// <summary>Whether the shape's data holds all of it.</summary>
        protected abstract bool IsComplete(BuildingShape shape);

        /// <summary>One step of building (the building's action, once per game turn).</summary>
        protected abstract void Step(Building building);

        protected virtual void OnPlaced(Building building)
        {
        }

        /// <summary>remc2 InitBuildingEntity_49A30 / remc1 InitBuildingEntity_36DF0.</summary>
        private Building Place(BuildingPlacement placement)
        {
            int index = GetShapeIndex(placement);
            var shape = Shapes[index];
            if (shape == null || shape.IsEmpty || !IsComplete(shape))
                return null;
            int width = shape.Width;
            int height = shape.Height;
            int x = placement.X & 0xFF;
            int y = placement.Y & 0xFF;
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
            var building = new Building { Shape = shape, Index = index, X = x, Y = y, StartX = startX, StartY = startY, Z = z };
            OnPlaced(building);
            return building;
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
                {
                    int x = startX + column;
                    int y = startY + row;
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
