using MagicCarpet2Terrain.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;

namespace MCLevelEdit.Infrastructure.Adapters;

public class FileAdapter : IFilePort
{
    public const int FILE_SIZE = 38812;

    // Magic Carpet 1 / Hidden Worlds terrain fields (remc1 Type_Mc1Level_2F503). Unlike MC2 there is no RkSte,
    // and LRiver comes straight after River.
    private const int MC1_RIVER = 0x14;
    private const int MC1_LRIVER = 0x18;
    private const int MC1_SOURCE = 0x1C;
    private const int MC1_SNLIN = 0x20;
    private const int MC1_SNFLT = 0x24;
    private const int MC1_BHLIN = 0x28;
    private const int MC1_BHFLT = 0x2C;

    public Task<bool> SaveMapAsync(Map map, string fileName)
    {
        return Task.Run<bool>(() => {
            return SaveMap(map, fileName);
        });
    }

    public bool SaveMap(Map map, string fileName)
    {
        if (map is MCLevelEdit.Model.Domain.MC2.Mc2Map mc2Map)
        {
            File.WriteAllBytes(fileName, Mc2LevelFormat.Write(mc2Map));
            return true;
        }

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
        WriteUInt32ToArray(map.Terrain.GenerationParameters.Raise, levfile, 12);
        WriteUShortToArray(map.Terrain.GenerationParameters.Gnarl, levfile, 16);
        WriteUShortToArray(map.Terrain.GenerationParameters.River, levfile, MC1_RIVER);
        WriteUShortToArray(map.Terrain.GenerationParameters.LRiver, levfile, MC1_LRIVER);
        WriteUShortToArray(map.Terrain.GenerationParameters.Source, levfile, MC1_SOURCE);
        WriteUShortToArray(map.Terrain.GenerationParameters.SnLin, levfile, MC1_SNLIN);
        WriteUShortToArray(map.Terrain.GenerationParameters.SnFlt, levfile, MC1_SNFLT);
        WriteUShortToArray(map.Terrain.GenerationParameters.BhLin, levfile, MC1_BHLIN);
        WriteUShortToArray(map.Terrain.GenerationParameters.BhFlt, levfile, MC1_BHFLT);
        // MC1 levels have no RkSte (Magic Carpet 2 added it)

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

            levfile[spellStart] = wizard.Spells.Fireball.Item1;
            levfile[aiStatStart] = wizard.Spells.Fireball.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Heal.Item1;
            levfile[aiStatStart] = wizard.Spells.Heal.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Accelerate.Item1;
            levfile[aiStatStart] = wizard.Spells.Accelerate.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Possess.Item1;
            levfile[aiStatStart] = wizard.Spells.Possess.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Shield.Item1;
            levfile[aiStatStart] = wizard.Spells.Shield.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.BeyondSight.Item1;
            levfile[aiStatStart] = wizard.Spells.BeyondSight.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Earthquake.Item1;
            levfile[aiStatStart] = wizard.Spells.Earthquake.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Meteor.Item1;
            levfile[aiStatStart] = wizard.Spells.Meteor.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Volcano.Item1;
            levfile[aiStatStart] = wizard.Spells.Volcano.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Crater.Item1;
            levfile[aiStatStart] = wizard.Spells.Crater.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Teleport.Item1;
            levfile[aiStatStart] = wizard.Spells.Teleport.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Duel.Item1;
            levfile[aiStatStart] = wizard.Spells.Duel.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Invisible.Item1;
            levfile[aiStatStart] = wizard.Spells.Invisible.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.StealMana.Item1;
            levfile[aiStatStart] = wizard.Spells.StealMana.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Rebound.Item1;
            levfile[aiStatStart] = wizard.Spells.Rebound.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.LightningBolt.Item1;
            levfile[aiStatStart] = wizard.Spells.LightningBolt.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.Castle.Item1;
            levfile[aiStatStart] = wizard.Spells.Castle.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.UndeadArmy.Item1;
            levfile[aiStatStart] = wizard.Spells.UndeadArmy.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.LightningStorm.Item1;
            levfile[aiStatStart] = wizard.Spells.LightningStorm.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.ManaMagnet.Item1;
            levfile[aiStatStart] = wizard.Spells.ManaMagnet.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.WallofFire.Item1;
            levfile[aiStatStart] = wizard.Spells.WallofFire.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.ReverseAcceleration.Item1;
            levfile[aiStatStart] = wizard.Spells.ReverseAcceleration.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.GlobalDeath.Item1;
            levfile[aiStatStart] = wizard.Spells.GlobalDeath.Item2;
            spellStart += 1;
            aiStatStart += 1;
            levfile[spellStart] = wizard.Spells.RapidFireball.Item1;
            levfile[aiStatStart] = wizard.Spells.RapidFireball.Item2;

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

        if (levfile[0] == 82 && levfile[1] == 78 && levfile[2] == 67)
        {
            throw new Exception("Compressed level detected! You must uncompress this file first");
        }

        if (LevelFormat.DetectFromFileSize(levfile.Length) == GameVersion.MagicCarpet2)
        {
            var mc2Map = Mc2LevelFormat.Read(levfile);
            mc2Map.FilePath = fileName;
            return mc2Map;
        }

        Map map = new Map();
        var counter = 0;

        map.FilePath = fileName;
        map.ManaTotal = BitConverter.ToUInt32(levfile, 0);
        map.ManaTarget = levfile[38800];
        var numWizards = levfile[38802];

        GenerationParameters terrainGenerationParameters = new GenerationParameters()
        {
            Seed = BitConverter.ToUInt16(levfile, 4),
            Offset = BitConverter.ToUInt16(levfile, 8),
            Raise = BitConverter.ToUInt32(levfile, 12),
            Gnarl = BitConverter.ToUInt16(levfile, 16),
            River = BitConverter.ToUInt16(levfile, MC1_RIVER),
            LRiver = BitConverter.ToUInt16(levfile, MC1_LRIVER),
            Source = (byte)BitConverter.ToUInt16(levfile, MC1_SOURCE),
            SnLin = BitConverter.ToUInt16(levfile, MC1_SNLIN),
            SnFlt = (byte)BitConverter.ToUInt16(levfile, MC1_SNFLT),
            BhLin = (byte)BitConverter.ToUInt16(levfile, MC1_BHLIN),
            BhFlt = (byte)BitConverter.ToUInt16(levfile, MC1_BHFLT)
            // MC1 levels have no RkSte (Magic Carpet 2 added it)
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

            var entityType = entityTypeId.GetEntityTypeFromTypeIdAndModelId(modelId, GameVersion.MagicCarpet1);

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
            wizard.Spells.Fireball = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Heal = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Accelerate = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Possess = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Shield = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.BeyondSight = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Earthquake = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Meteor = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Volcano = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Crater = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Teleport = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Duel = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Invisible = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.StealMana = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Rebound = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.LightningBolt = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.Castle = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.UndeadArmy = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.LightningStorm = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.ManaMagnet = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.WallofFire = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.ReverseAcceleration = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.GlobalDeath = new (levfile[spellStart], levfile[aiStatStart]);
            spellStart += 1;
            aiStatStart += 1;
            wizard.Spells.RapidFireball = new (levfile[spellStart], levfile[aiStatStart]);

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
