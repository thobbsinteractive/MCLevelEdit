using MagicCarpet2Terrain.Model;
using System;
using System.Collections.Generic;

namespace MagicCarpet2Terrain
{
    /// <summary>
    /// Builds Magic Carpet (and Hidden Worlds) terrain features (remc1 CreateFeatures_36430): walls, paths and
    /// buildings all in one pass. A building's shape is 10h after the level's shape number in BUILD?-0.TAB. A shape
    /// is rows of run-length codes: 0 ends the row, a negative code skips that many squares, a positive one is
    /// followed by that many square codes, each giving the square's height above the building's base and its type.
    /// </summary>
    public class Mc1TerrainFeatureBuilder : TerrainFeatureBuilder
    {
        /// <summary>The level's shape number is the shape's index less this.</summary>
        public const int SHAPE_OFFSET = 0x10;

        public Mc1TerrainFeatureBuilder(BuildingShapes shapes, SearchRings search) : base(shapes, search)
        {
        }

        protected override uint RandMask => 0xFFFFFFFF;

        // remc1 InitCreatedEntities_36620 leaves the entities' index as their slot
        protected override bool CountsTurns => false;

        protected override IEnumerable<Func<LevelFeature, bool>> GetPasses()
        {
            yield return f => ShapesTerrain(f.Model);
        }

        protected override bool PassesOldTypeForType10 => true;

        protected override int GetShapeIndex(LevelFeature building) => building.Parent + SHAPE_OFFSET;

        /// <summary>remc1 SetTerrainTypeAround_33AE0: next to wall, a flat square is shaded 20h, not at random.</summary>
        protected override void ShadeWallTile(int x, int y)
        {
            int shade = (sbyte)(Height[Tile(x - 1, y - 1)] - Height[Tile(x + 1, y + 1)] + 0x20);
            if (shade < 0x20)
                shade = 0x20;
            else if (shade > 0x28)
                shade = (shade & 7) + 0x28;
            Shading[Tile(x, y)] = (byte)shade;
            Angle[Tile(x, y)] &= 0xF7;
        }

        // The rows are read to the end of the file at most
        protected override bool IsComplete(BuildingShape shape) => true;

        /// <summary>remc1 BuildBuildingAction_27D30 (the action 33h of the Effect 2Dh).</summary>
        protected override void Step(Building building)
        {
            var shape = building.Shape;
            int rows = shape.Height;
            int halfWidth = shape.Width >> 1;
            int halfHeight = rows >> 1;
            int centreX = building.X;
            int centreY = building.Y;
            int z = building.Z;

            building.Life--;
            if (building.Life == 0)
            {
                UpdateTerrainTiles(centreX - halfWidth, centreY - halfHeight, centreX + halfWidth, centreY + halfHeight);
                SmoothBorder(centreX - halfWidth, centreY - halfHeight, halfHeight, halfWidth, 2);
                SmoothBorder(centreX - halfWidth, centreY - halfHeight, halfHeight, halfWidth, 5);
                building.Done = true;
                return;
            }

            int life = building.Life;
            int startX = centreX - halfWidth;
            int startY = centreY - halfHeight;
            // 1: the heights a step toward the building
            ForEachSquare(shape, startX, startY, (x, y, code) =>
            {
                int low = code & 0x0F;
                int target;
                if (code < 0x0F)
                {
                    if (code <= 6)
                        return;
                    target = z;
                }
                else if ((code >> 4) == 3)
                {
                    int part = low % 3;
                    if (part == 0)
                        return;
                    target = z + (part == 1 ? 0x0C : 0x10);
                }
                else
                {
                    if (low == 0)
                        return;
                    target = z + (low - 1) * 4;
                }
                StepHeight(x, y, target, life);
            });

            // 2: the terrain types on every 5th step and the last one
            if (life % 5 != 0 && life != 1)
                return;
            ForEachSquare(shape, startX, startY, (x, y, code) =>
            {
                int high = code >> 4;
                int low = code & 0x0F;
                // callerCl / callerDl: what the original leaves in cl / dl
                byte type;
                byte callerCl = code;
                byte callerDl = 0;
                if (high == 0)
                {
                    int part = code % 7;
                    if (part == 0)
                        return;
                    type = (byte)(part - 1);
                    callerCl = 7;
                    callerDl = (byte)part;
                }
                else if (high <= 2)
                    type = (byte)(high + 7);
                else if (high == 3)
                {
                    type = (byte)(low / 3 + 10);
                    callerCl = type;
                    callerDl = (byte)(low % 3);
                }
                else
                    type = (byte)(high + 0x0B);
                SetTileTerrainType(callerCl, x, y, type, callerDl);
            });
        }

        private delegate void SquareAction(int x, int y, byte code);

        private static void ForEachSquare(BuildingShape shape, int startX, int startY, SquareAction action)
        {
            byte[] data = shape.Data;
            int position = shape.Offset;
            int x = startX;
            int y = startY;
            for (int left = shape.Height; left > 0 && position < data.Length;)
            {
                sbyte code = (sbyte)data[position++];
                if (code == 0)
                {
                    y++;
                    left--;
                    x = startX;
                    continue;
                }
                if (code < 0)
                {
                    x -= code;
                    continue;
                }
                for (int n = code; n > 0 && position < data.Length; n--, x++)
                    action(x & 0xFF, y & 0xFF, data[position++]);
            }
        }
    }
}
