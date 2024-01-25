using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;

namespace MCLevelEdit.Infrastructure.Adapters;

public class FileAdapter : IFilePort
{
    public Task<Map> LoadMapAsync(string fileName)
    {
        return Task.Run<Map>(() => {
            return LoadMap(fileName);
        });
    }

    public Map LoadMap(string fileName)
    {
        var levfile = File.ReadAllBytes(fileName);
        Map map = new Map();
        var counter = 0;
        var Wnumber = 1;

        if (levfile[0] == 82 && levfile[1] == 78 && levfile[2] == 67)
        {
            throw new Exception("Compressed level detected! You must uncompress this file first");
        }

        map.FilePath = fileName;
        map.ManaTotal = BitConverter.ToUInt32(levfile, 0);
        map.ManaTarget = levfile[38800];
        var numWizards = levfile[38802];

        GenerationParameters terrainGenerationParameters = new GenerationParameters()
        {
            Seed = BitConverter.ToUInt16(levfile, 4),
            Offset = BitConverter.ToUInt16(levfile, 8),
            Raise = (ushort)BitConverter.ToUInt32(levfile, 12),
            Gnarl = BitConverter.ToUInt16(levfile, 16),
            River = BitConverter.ToUInt16(levfile, 20),
            Source = (byte)BitConverter.ToUInt16(levfile, 24),
            SnLin = BitConverter.ToUInt16(levfile, 28),
            SnFlt = (byte)BitConverter.ToUInt16(levfile, 32),
            BhLin = (byte)BitConverter.ToUInt16(levfile, 36),
            BhFlt = (byte)BitConverter.ToUInt16(levfile, 40),
            RkSte = (byte)BitConverter.ToUInt16(levfile, 44)
        };

        int fpos = 1090; // Move on to Thing data

        var entityCount = Globals.MAX_ENTITIES;

        var terrain = new Terrain()
        {
            GenerationParameters = terrainGenerationParameters
        };

        map.Terrain = terrain;

        do
        {
            counter++;
            TypeId entityTypeId = (TypeId)BitConverter.ToUInt16(levfile, fpos);
            ushort modelId = BitConverter.ToUInt16(levfile, fpos + 2);
            ushort Xpos = BitConverter.ToUInt16(levfile, fpos + 4);
            ushort Ypos = BitConverter.ToUInt16(levfile, fpos + 6);
            ushort DisId = BitConverter.ToUInt16(levfile, fpos + 8);
            ushort SwiSz = BitConverter.ToUInt16(levfile, fpos + 10);
            ushort SwiId = BitConverter.ToUInt16(levfile, fpos + 12);
            ushort Parent = BitConverter.ToUInt16(levfile, fpos + 14);
            ushort Child = BitConverter.ToUInt16(levfile, fpos + 16);

            var entityType = entityTypeId.GetEntityTypeFromTypeIdAndModelId(modelId);

            if (entityType != null)
            {
                map.Entities.Add(new Entity()
                {
                    Id = counter,
                    EntityType = entityType,
                    Position = new Position(Xpos, Ypos),
                    DisId = DisId,
                    SwitchSize = SwiSz,
                    SwitchId = SwiId,
                    Parent = Parent,
                    Child = Child
                });
            }
            fpos += 18;
            entityCount--;
        } while (entityCount != 0);

        fpos = 37076;
        int clevelPos = 38804;

        foreach (var wizard in map.Wizards)
        {
            wizard.IsActive = numWizards > 0;
            wizard.Agression = levfile[fpos];
            wizard.Perception = levfile[fpos + 4];
            wizard.Reflexes = levfile[fpos + 8];
            wizard.CastleLevel = levfile[clevelPos];

            int spellStart = fpos + 12;

            wizard.Spells.Fireball = levfile[spellStart] > 0;
            wizard.Spells.Shield = levfile[spellStart + 1] > 0;
            wizard.Spells.Accelerate = levfile[spellStart + 2] > 0;
            wizard.Spells.Possession = levfile[spellStart + 3] > 0;
            wizard.Spells.Health = levfile[spellStart + 4] > 0;
            wizard.Spells.BeyondSight = levfile[spellStart + 5] > 0;
            wizard.Spells.Earthquake = levfile[spellStart + 6] > 0;
            wizard.Spells.Meteor = levfile[spellStart + 7] > 0;
            wizard.Spells.Volcano = levfile[spellStart + 8] > 0;
            wizard.Spells.Crater = levfile[spellStart + 9] > 0;
            wizard.Spells.Teleport = levfile[spellStart + 10] > 0;
            wizard.Spells.Duel = levfile[spellStart + 11] > 0;
            wizard.Spells.Invisible = levfile[spellStart + 12] > 0;
            wizard.Spells.StealMana = levfile[spellStart + 13] > 0;
            wizard.Spells.Rebound = levfile[spellStart + 14] > 0;
            wizard.Spells.Lightning = levfile[spellStart + 15] > 0;
            wizard.Spells.Castle = levfile[spellStart + 16] > 0;
            wizard.Spells.UndeadArmy = levfile[spellStart + 17] > 0;
            wizard.Spells.LightningStorm = levfile[spellStart + 18] > 0;
            wizard.Spells.ManaMagnet = levfile[spellStart + 19] > 0;
            wizard.Spells.WallofFire = levfile[spellStart + 20] > 0;
            wizard.Spells.ReverseAcceleration = levfile[spellStart + 21] > 0;
            wizard.Spells.GlobalDeath = levfile[spellStart + 22] > 0;
            wizard.Spells.RapidFireball = levfile[spellStart + 23] > 0;

            fpos += 216;
            clevelPos += 1;

            if (numWizards > 0)
                numWizards--;
        }
        return map;
    }
}
