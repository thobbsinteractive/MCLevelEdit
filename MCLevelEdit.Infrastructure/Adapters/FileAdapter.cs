using MagicCarpet2Terrain.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;

namespace MCLevelEdit.Infrastructure.Adapters;

public class FileAdapter : IFilePort
{
    public const int FILE_SIZE = 38812;

    public Task<bool> SaveMapAsync(Map map, string fileName)
    {
        return Task.Run<bool>(() => {
            return SaveMap(map, fileName);
        });
    }

    public bool SaveMap(Map map, string fileName)
    {
        var levfile = new byte[FILE_SIZE];
        var counter = 0;
        var numWizards = map.Wizards.Where(w => w.IsActive).Count();

        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        WriteUInt32ToArray(map.ManaTotal, levfile, 0);
        levfile[38800] = map.ManaTarget;
        levfile[38802] = (byte)numWizards;

        WriteUShortToArray(map.Terrain.GenerationParameters.Seed, levfile, 4);
        WriteUShortToArray(map.Terrain.GenerationParameters.Offset, levfile, 8);
        WriteUShortToArray(map.Terrain.GenerationParameters.Raise, levfile, 12);
        WriteUShortToArray(map.Terrain.GenerationParameters.Gnarl, levfile, 16);
        WriteUShortToArray(map.Terrain.GenerationParameters.River, levfile, 20);
        WriteUShortToArray(map.Terrain.GenerationParameters.Source, levfile, 24);
        WriteUShortToArray(map.Terrain.GenerationParameters.SnLin, levfile, 28);
        WriteUShortToArray(map.Terrain.GenerationParameters.SnFlt, levfile, 32);
        WriteUShortToArray(map.Terrain.GenerationParameters.BhLin, levfile, 36);
        WriteUShortToArray(map.Terrain.GenerationParameters.BhFlt, levfile, 40);
        WriteUShortToArray(map.Terrain.GenerationParameters.RkSte, levfile, 44);

        int fpos = 1090; // Move on to Thing data
        var entityCount = Globals.MAX_ENTITIES;

        do
        {
            counter++;
            var entity = map.GetEntity(counter);

            ushort entityTypeId = 0;
            ushort modelId = 0;
            ushort Xpos = 0;
            ushort Ypos = 0;
            ushort DisId = 0;
            ushort SwiSz = 0;
            ushort SwiId = 0;
            ushort Parent = 0;
            ushort Child = 0;

            if (entity != null)
            {
                entityTypeId = (ushort)entity.EntityType.TypeId;
                modelId = (ushort)entity.EntityType.Model.Id;
                Xpos = (ushort)entity.Position.X;
                Ypos = (ushort)entity.Position.Y;
                DisId = entity.DisId;
                SwiSz = entity.SwitchSize;
                SwiId = entity.SwitchId;
                Parent = entity.Parent;
                Child = entity.Child;
            }

            WriteUShortToArray(entityTypeId, levfile, fpos);
            WriteUShortToArray(modelId, levfile, fpos + 2);
            WriteUShortToArray(Xpos, levfile, fpos + 4);
            WriteUShortToArray(Ypos, levfile, fpos + 6);
            WriteUShortToArray(DisId, levfile, fpos + 8);
            WriteUShortToArray(SwiSz, levfile, fpos + 10);
            WriteUShortToArray(SwiId, levfile, fpos + 12);
            WriteUShortToArray(Parent, levfile, fpos + 14);
            WriteUShortToArray(Child, levfile, fpos + 16);

            fpos += 18;
            entityCount--;
        } while (entityCount != 0);

        fpos = 37076;
        int clevelPos = 38804;

        foreach (var wizard in map.Wizards)
        {
            levfile[fpos] = wizard.Agression;
            levfile[fpos + 4] = wizard.Perception;
            levfile[fpos + 8] = wizard.Reflexes;
            levfile[clevelPos] = wizard.CastleLevel;

            int spellStart = fpos + 12;
            int aiStatStart = spellStart + 100;

            levfile[spellStart] = wizard.Spells.Fireball.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Fireball.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Heal.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Heal.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Accelerate.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Accelerate.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Possess.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Possess.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Shield.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Shield.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.BeyondSight.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.BeyondSight.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Earthquake.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Earthquake.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Meteor.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Meteor.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Volcano.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Volcano.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Crater.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Crater.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Teleport.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Teleport.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Duel.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Duel.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Invisible.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Invisible.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.StealMana.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.StealMana.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Rebound.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Rebound.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.LightningBolt.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.LightningBolt.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Castle.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.Castle.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.UndeadArmy.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.UndeadArmy.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.LightningStorm.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.LightningStorm.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.ManaMagnet.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.ManaMagnet.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.WallofFire.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.WallofFire.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.ReverseAcceleration.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.ReverseAcceleration.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.GlobalDeath.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.GlobalDeath.GetBytes()[1];
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.RapidFireball.GetBytes()[0];
            levfile[aiStatStart] = wizard.Spells.RapidFireball.GetBytes()[1];

            for (int i = 0; i < 76; i++)
            {
                spellStart += 1;
                aiStatStart += 1;
                levfile[spellStart] = 0;
                levfile[aiStatStart] = 1;
            }

            fpos += 216;
            clevelPos += 1;
        }

        File.WriteAllBytes(fileName, levfile);

        return true;
    }

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
            int aiStatStart = spellStart + 100;
            wizard.Spells.Fireball = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Heal = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Accelerate = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Possess = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Shield = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.BeyondSight = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Earthquake = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Meteor = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Volcano = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Crater = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Teleport = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Duel = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Invisible = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.StealMana = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Rebound = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.LightningBolt = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Castle = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.UndeadArmy = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.LightningStorm = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.ManaMagnet = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.WallofFire = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.ReverseAcceleration = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.GlobalDeath = new Abilities(levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.RapidFireball = new Abilities(levfile[spellStart], levfile[aiStatStart]);

            fpos += 216;
            clevelPos += 1;

            if (numWizards > 0)
                numWizards--;
        }
        return map;
    }

    private void WriteUShortToArray(ushort value, byte[] target, int startIdx)
    {
        var source = UShortToBytes(value);
        Buffer.BlockCopy(source, 0, target, startIdx, source.Length);
    }

    private void WriteUInt32ToArray(uint value, byte[] target, int startIdx)
    {
        var source = UInt32ToBytes(value);
        Buffer.BlockCopy(source, 0, target, startIdx, source.Length);
    }

    private byte[] UInt32ToBytes(uint target)
    {
        byte[] decoded = new byte[sizeof(uint)];
        Buffer.BlockCopy(new uint[] { target }, 0, decoded, 0, decoded.Length);
        return decoded;
    }

    private byte[] UShortToBytes(ushort target)
    {
        byte[] decoded = new byte[sizeof(ushort)];
        Buffer.BlockCopy(new ushort[] { target }, 0, decoded, 0, decoded.Length);
        return decoded;
    }
}
