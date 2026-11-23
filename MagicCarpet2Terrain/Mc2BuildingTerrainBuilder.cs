using MagicCarpet2Terrain.Model;
using System.Collections.Generic;
using System.Linq;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Builds Magic Carpet 2 buildings into the terrain (remc2 GenerateEvents_49290): first the shapes with the
    /// building parameter flag 10h, then the others. Each square of a shape has 2 bytes: the terrain type, and the
    /// height above the building's base (FFh: the square is left as it is).
    /// </summary>
    public class Mc2BuildingTerrainBuilder : BuildingTerrainBuilder
    {
        private readonly BuildingParams _params;

        public Mc2BuildingTerrainBuilder(BuildingShapes shapes, BuildingParams buildingParams) : base(shapes)
        {
            _params = buildingParams;
        }

        protected override IEnumerable<IList<BuildingPlacement>> GetPasses(IList<BuildingPlacement> placements)
        {
            yield return placements.Where(p => BuiltFirst(p)).ToList();
            yield return placements.Where(p => !BuiltFirst(p)).ToList();
        }

        private bool BuiltFirst(BuildingPlacement placement) =>
            ((_params?.GetFlags(placement.Shape) ?? 0) & BuildingParams.FLAG_BUILT_FIRST) != 0;

        protected override int GetShapeIndex(BuildingPlacement placement) => placement.Shape;

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
