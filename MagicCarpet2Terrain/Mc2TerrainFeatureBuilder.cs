using MagicCarpet2Terrain.Model;
using System;
using System.Collections.Generic;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Builds Magic Carpet 2 terrain features (remc2 GenerateEvents_49290) in the game's passes: paths, walls, then
    /// the buildings with the building parameter flag 10h, then the other buildings. Each square of a building shape
    /// has 2 bytes: the terrain type, and the height above the building's base (FFh: the square is left as it is).
    /// </summary>
    public class Mc2TerrainFeatureBuilder : TerrainFeatureBuilder
    {
        private readonly BuildingParams _params;

        public Mc2TerrainFeatureBuilder(BuildingShapes shapes, BuildingParams buildingParams, SearchRings search) : base(shapes, search)
        {
            _params = buildingParams;
        }

        protected override uint RandMask => 0xFFFF;

        protected override bool BuildsGates => true;

        protected override bool CountsTurns => true;

        protected override IEnumerable<Func<LevelFeature, bool>> GetPasses()
        {
            // The game's 7 passes, each running its entities (and stepping the world's seed) even when nothing of it is
            // built here: events (52h); paths, canyons, ridges (with volcanoes, craters...); 50h-51h; cave features
            // (Special 2); walls; the buildings with the flag 10h; the other buildings.
            yield return f => f.Model == ROOM;
            yield return f => f.Model == PATH || f.Model == PATH_POINT || f.Model == CANYON || f.Model == CANYON_MOVER || f.Model == RIDGE
                || f.Model == RIDGE_MOVER || f.Model == VOLCANO || f.Model == CRATER || f.Model == EMBER
                || f.Model == CAVE_MOUND || f.Model == CEILING_CONE || f.Model == FLOOR_CONE;
            yield return f => f.Model == TUNNEL || f.Model == TUNNEL_CARVER;
            yield return f => f.Model == CAVE_DOOR;
            yield return f => f.Model == WALL_SECTION || f.Model == WALL;
            yield return f => f.Model == BUILDING && BuiltFirst(f);
            yield return f => f.Model == BUILDING;
        }

        /// <summary>
        /// remc2 SetStagetagForTermod_49830: every wall, path, canyon, ridge and 50h node with no switch gets the stage
        /// tag 1, so each of their lines is made whatever the level's SwitchId.
        /// </summary>
        protected override void SetStageTags(IList<LevelFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature.DisId == LevelFeature.NO_SWITCH && (feature.Model == WALL || feature.Model == PATH
                    || feature.Model == CANYON || feature.Model == RIDGE || feature.Model == 0x50))
                    feature.SwitchId = 1;
            }
        }

        /// <summary>
        /// remc2 player set-up (Player.cpp, a wizard with the castle spell and a castle level) and AddCastle_4AA40: the
        /// castle stands on the wizard's start, a square right on the odd squares, its base the lowest height round shape
        /// 1. The game stamps the levels below the start level there (UpdateManaSphereMovement_36FC0), then grows the start
        /// level in its first turns; here the shapes 1 to the start level are stamped, which ends where that growth does.
        /// </summary>
        protected override void BuildCastles(IList<LevelFeature> castles)
        {
            var first = Shapes?[1];
            if (first == null || first.IsEmpty)
                return;
            foreach (var castle in castles)
            {
                int x = castle.X & 0xFF;
                int y = castle.Y & 0xFF;
                if ((x + y) % 2 != 0)
                    x = (x + 1) & 0xFF;
                int z = GetEdge(Height, (x - first.Width / 2) & 0xFF, (y - first.Height / 2) & 0xFF, first.Width, first.Height, false);
                for (int level = 1; level <= castle.Parent; level++)
                    StampCastleLevel(x, y, z, level);
            }
        }

        /// <summary>remc2 UpdateManaSphereMovement_36FC0: one castle level's shape, at once, on the base height.</summary>
        private void StampCastleLevel(int x, int y, int z, int index)
        {
            var shape = Shapes[index];
            if (shape == null || shape.IsEmpty || !IsComplete(shape))
                return;
            bool raisesCeiling = IsCave && ((_params?.GetFlags(index) ?? 0) & BuildingParams.FLAG_KEEPS_CEILING) == 0;
            int startX = x - (shape.Width >> 1);
            int startY = y - (shape.Height >> 1);
            int position = shape.Offset;
            for (int row = 0; row < shape.Height; row++)
                for (int column = 0; column < shape.Width; column++, position += 2)
                {
                    int tx = startX + column, ty = startY + row;
                    int tile = Tile(tx, ty);
                    if (shape.Data[position + 1] != 0xFF)
                    {
                        Height[tile] = (byte)(shape.Data[position + 1] + z);
                        if ((Angle[tile] & 7) == 0)
                        {
                            Angle[tile] = (byte)((Angle[tile] & 0xF8) | 1);
                            UpdateTerrainTiles(tx, ty, tx, ty);
                        }
                    }
                    if (raisesCeiling)
                    {
                        int top = Math.Min(Math.Max((int)Height[tile], z) + 80, 255);
                        if (top > Ceiling[tile])
                            Ceiling[tile] = (byte)top;
                        CloseCeiling(tile);
                    }
                }
            position = shape.Offset;
            for (int row = 0; row < shape.Height; row++)
                for (int column = 0; column < shape.Width; column++, position += 2)
                    if (shape.Data[position] != 0xFF)
                        SetTileTerrainType(0, startX + column, startY + row, shape.Data[position], 0);
        }

        // remc2 SPELLS_BEGIN_BUFFER_str life_0x1A of the spells 18 (volcano), 16 (crater) and 17 (ember) at levels 0-2
        private static readonly int[] VolcanoSizes = { 7, 9, 11 };
        private static readonly int[] CraterLives = { 6, 12, 24 };
        private static readonly int[] EmberLives = { 16, 32, 64 };

        protected override int? GetSpellLife(int model, int level)
        {
            if (level < 0 || level > 2)
                return null;
            switch (model)
            {
                case VOLCANO: return VolcanoSizes[level];
                case CRATER: return CraterLives[level];
                case EMBER: return EmberLives[level];
                default: return null;
            }
        }

        /// <summary>remc2 CreateLinkedLevelFeatures_49090: the node's Par3 0-3 is the depth 2, 6, 16 or 32, otherwise as it is.</summary>
        protected override int GetCanyonDepth(LevelFeature node)
        {
            int depth;
            switch (node.Par3)
            {
                case 0: depth = 2; break;
                case 1: depth = 6; break;
                case 2: depth = 16; break;
                case 3: depth = 32; break;
                default: depth = node.Par3; break;
            }
            // levelPar3_0x46_70 is a signed byte
            return (sbyte)depth;
        }

        private bool BuiltFirst(LevelFeature building) =>
            ((_params?.GetFlags(building.Parent) ?? 0) & BuildingParams.FLAG_BUILT_FIRST) != 0;

        protected override int GetShapeIndex(LevelFeature building) => building.Parent;

        /// <summary>
        /// remc2 CheckTileType8Neighbours_33F70 compares the square's own height where Magic Carpet compares the square
        /// after it, so that one never counts.
        /// </summary>
        protected override bool IsWallEdge(int tile)
        {
            int x = tile & 0xFF, y = tile >> 8;
            int limit = Height[tile] + 30;
            return Type[Tile(x - 1, y)] != 8 || Height[Tile(x - 1, y)] > limit
                || Height[Tile(x, y + 1)] > limit || Height[Tile(x, y - 1)] > limit;
        }

        protected override bool IsComplete(BuildingShape shape) =>
            shape.Offset + 2 * shape.Width * shape.Height <= shape.Data.Length;

        protected override void OnPlaced(Building building)
        {
            building.RaisesCeiling = IsCave && ((_params?.GetFlags(building.Index) ?? 0) & BuildingParams.FLAG_KEEPS_CEILING) == 0;
        }

        /// <summary>remc2 ApplyTerrainModification_37240 (the action 33h of the Effect 2Dh).</summary>
        protected override void Step(Building building)
        {
            var shape = building.Shape;
            byte[] data = shape.Data;
            int rows = shape.Height;
            int columns = shape.Width;
            int halfHeight = rows >> 1;
            int halfWidth = columns >> 1;
            int startX = building.StartX;
            int startY = building.StartY;
            int z = building.Z;

            building.Life--;
            if (building.Life <= 0)
            {
                // the types of the squares of the shape again, then the ground around it smoothed
                int position = shape.Offset;
                for (int row = 0; row < rows; row++)
                    for (int column = 0; column < columns; column++, position += 2)
                        if (data[position] != 0xFF)
                            UpdateTerrainTiles(startX + column, startY + row, startX + column, startY + row);
                // the original passes the shape's corner where it expects its centre (remc2: //problem)
                SmoothBorder(startX - halfWidth, startY - halfHeight, halfHeight, halfWidth, 2);
                SmoothBorder(startX - halfWidth, startY - halfHeight, halfHeight, halfWidth, 5);
                building.Done = true;
                return;
            }

            int life = building.Life;
            // 1: the heights a step toward the building
            int index = shape.Offset;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++, index += 2)
                {
                    int x = startX + column;
                    int y = startY + row;
                    if (data[index + 1] != 0xFF)
                        StepHeight(x, y, data[index + 1] + z, life);
                    if (building.RaisesCeiling)
                    {
                        int tile = Tile(x, y);
                        int target = System.Math.Min(System.Math.Max((int)Height[tile], z) + 80, 255);
                        int ceiling = Ceiling[tile];
                        if (target > ceiling)
                            Ceiling[tile] = (byte)(ceiling + (target - ceiling) / life);
                        CheckCeiling(tile);
                    }
                }

            // 2: the terrain types on every 5th step and the last one
            if (life % 5 != 0 && life != 1)
                return;
            index = shape.Offset;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++, index += 2)
                {
                    if (data[index] == 0xFF)
                        continue;
                    int tile = Tile(startX + column, startY + row);
                    Angle[tile] = (byte)((Angle[tile] & 0xF0) | 1);
                    Type[tile] = 1;
                }
            index = shape.Offset;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++, index += 2)
                {
                    if (data[index] == 0xFF)
                        continue;
                    int x = (startX + column) & 0xFF;
                    // cl: the column; dl: 0 on the first column, else the square's x (what the original leaves there)
                    SetTileTerrainType((byte)column, x, startY + row, data[index], column != 0 ? (byte)x : (byte)0);
                }
        }
    }
}
