using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MCLevelEdit.Services
{
    public class FileService : IFileService
    {
        public Task<Map> LoadMapFromFile(string fileName)
        {
            var levfile = File.ReadAllBytes(fileName);
            var counter = 0;
            var Wnumber = 1;

            if (levfile[0] == 82 && levfile[1] == 78 && levfile[2] == 67)
            {
                throw new Exception("Compressed level detected! You must uncompress this file first");
            }

            var manaTarget = levfile[38800];
            var numWizards = levfile[38802];
            var manaTotal = BitConverter.ToInt32(levfile, 0);

            TerrainGenerationParameters terrainGenerationParameters = new TerrainGenerationParameters()
            {
                Seed = BitConverter.ToUInt16(levfile, 4),
                Offset = BitConverter.ToUInt16(levfile, 8),
                Raise = (ushort)BitConverter.ToUInt32(levfile, 12),
                Gnarl = BitConverter.ToUInt16(levfile, 16),
                River = BitConverter.ToUInt16(levfile, 20),
                Source = BitConverter.ToUInt16(levfile, 24),
                SnLin = BitConverter.ToUInt16(levfile, 28),
                SnFlt = (byte)BitConverter.ToUInt16(levfile, 32),
                BhLin = (byte)BitConverter.ToUInt16(levfile, 36),
                BhFlt = BitConverter.ToUInt16(levfile, 40),
                RkSte = BitConverter.ToUInt16(levfile, 44)
            };

            int fpos = 1090; // Move on to Thing data

            var entityCount = Globals.MAX_ENTITIES;

            Map map = new Map()
            {
                TerrainGenerationParameters = terrainGenerationParameters,
            };

            do
            {
                counter++;
                // Get Thing entries
                TypeId entityTypeId = (TypeId)BitConverter.ToUInt16(levfile, fpos);
                //string ThingType = IdentifyThing(classValue);
                ushort childId = BitConverter.ToUInt16(levfile, fpos + 2);
                ushort Xpos = BitConverter.ToUInt16(levfile, fpos + 4);
                ushort Ypos = BitConverter.ToUInt16(levfile, fpos + 6);
                ushort DisId = BitConverter.ToUInt16(levfile, fpos + 8);
                ushort SwiSz = BitConverter.ToUInt16(levfile, fpos + 10);
                ushort SwiId = BitConverter.ToUInt16(levfile, fpos + 12);
                ushort Parent = BitConverter.ToUInt16(levfile, fpos + 14);
                ushort Child = BitConverter.ToUInt16(levfile, fpos + 16);

                string ThingName = "";

                var entityType = EntityTypeExtensions.GetEntityFromTypeIdAndChildId(entityTypeId, childId);

                if (entityType != null)
                {
                    map.Entities.Add(new Entity()
                    {
                        Id = counter,
                        EntityType = entityType,
                        Position = new Position(Xpos, Ypos),
                        Parent = Parent,
                        Child = Child
                    });
                }

                //if (classValue == 0)
                //{
                //    ThingName = "Blank";
                //}
                //else if (classValue == 2)
                //{
                //    ThingName = IdentifyScenery(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.Green);
                //    }
                //}
                //else if (classValue == 3)
                //{
                //    ThingName = IdentifySpawn(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.Yellow);
                //    }
                //}
                //else if (classValue == 5)
                //{
                //    ThingName = IdentifyCreature(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.Red);
                //    }
                //}
                //else if (classValue == 7)
                //{
                //    ThingName = IdentifyWeather(Model);
                //}
                //else if (classValue == 10)
                //{
                //    ThingName = IdentifyEffect(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.Cyan);
                //    }
                //}
                //else if (classValue == 11)
                //{
                //    ThingName = IdentifySwitch(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.White);
                //    }
                //}
                //else if (classValue == 12)
                //{
                //    ThingName = IdentifySpell(Model);
                //    if (drawMap == 1)
                //    {
                //        bmp.SetPixel(Xpos, Ypos, Color.Purple);
                //    }
                //}

                // Define Thing datatable rows
                //DataRow row = datatable.NewRow();

                //row["thingno"] = ThingNo;
                //row["class"] = classValue;
                //row["ThingTypeHidden"] = ThingType;
                //row["Model"] = Model;
                //row["ThingNameHidden"] = ThingName;
                //row["XPos"] = Xpos;
                //row["YPos"] = Ypos;
                //row["DisId"] = DisId;
                //row["swisz"] = SwiSz;
                //row["SwiId"] = SwiId;
                //row["parent"] = Parent;
                //row["child"] = Child;
                //datatable.Rows.Add(row);

                //ThingCount--;

                fpos += 18;
                entityCount--;
            } while (entityCount != 0);

            return Task.FromResult(map);
        }
    }
}
