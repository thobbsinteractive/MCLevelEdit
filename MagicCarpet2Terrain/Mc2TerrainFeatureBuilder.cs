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

        public Mc2TerrainFeatureBuilder(BuildingShapes shapes, BuildingParams buildingParams) : base(shapes)
        {
            _params = buildingParams;
        }

        protected override IEnumerable<Func<LevelFeature, bool>> GetPasses()
        {
            // the passes of the features built here (others the game builds in between: canyons, ridges, tunnels...)
            yield return f => f.Model == PATH;
            yield return f => f.Model == WALL_SECTION || f.Model == WALL;
            yield return f => f.Model == BUILDING && BuiltFirst(f);
            yield return f => f.Model == BUILDING;
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
