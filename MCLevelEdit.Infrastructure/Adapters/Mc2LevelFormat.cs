using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using MCLevelEdit.Model.Domain.MC2;
using System.Buffers.Binary;

namespace MCLevelEdit.Infrastructure.Adapters;

/// <summary>
/// Reads and writes uncompressed Magic Carpet 2 level files.
/// Layout follows Type_Level_2FECE in remc2 (remc2/engine/BasicTerrain.h), 0x6604 bytes.
/// </summary>
public static class Mc2LevelFormat
{
    // Header
    private const int LEVEL_ID = 2;
    private const int LEVEL_TYPE = 4;
    private const int BASIC_HEIGHT = 5;
    private const int MAP_TYPE = 6;
    private const int MANA_GOAL = 7;
    private const int NUMBER_OF_PLAYERS = 9;
    // The castle level each wizard starts with, a byte each (player_0x2FED9)
    private const int CASTLE_LEVELS = 11;
    private const int SEED = 23;
    private const int OFFSET = 27;
    private const int RAISE = 31;
    private const int GNARL = 35;
    private const int RIVER = 39;
    private const int LRIVER = 43;
    private const int SOURCE = 47;
    private const int SNLIN = 51;
    private const int SNFLT = 55;
    private const int BHLIN = 59;
    private const int BHFLT = 63;
    private const int RKSTE = 67;

    // Entities: slot 0 is unused by the game, slots 1..1199 hold the level's things
    private const int ENTITIES = 1091;
    private const int ENTITY_SIZE = 20;

    // Wizards (Type_WizardMapSettings_0x360D2)
    private const int WIZARDS = 25092;
    private const int WIZARD_SIZE = 110;
    private const int WIZARD_AGGRESSION = 3;
    private const int WIZARD_REFLEXES = 7;
    private const int WIZARD_PERCEPTION = 11;
    private const int WIZARD_STARTING_SPELLS = 15;
    private const int WIZARD_SPELL_LEVELS = 41;
    private const int WIZARD_BLOCKED_SPELLS = 67;
    private const int WIZARD_LIFE = 93;

    // Objectives (type_str_0x36442) and scripted stage vars (type_str_0x3647Ac)
    private const int OBJECTIVES = 25972;
    private const int OBJECTIVE_SIZE = 7;
    private const int STAGE_VARS = 26028;
    private const int STAGE_VAR_SIZE = 8;

    public static Mc2Map Read(byte[] data)
    {
        if (data.Length != LevelFormat.MC2_FILE_SIZE)
            throw new ArgumentException($"A Magic Carpet 2 level must be {LevelFormat.MC2_FILE_SIZE} bytes, but this one is {data.Length}.");

        var map = new Mc2Map
        {
            SourceData = (byte[])data.Clone(),
            LevelId = U16(data, LEVEL_ID),
            LevelType = data[LEVEL_TYPE],
            ManaTarget = (byte)U16(data, MANA_GOAL)
        };

        map.Terrain = new Terrain
        {
            GenerationParameters = new GenerationParameters
            {
                MapType = (MapType)data[MAP_TYPE],
                Seed = U16(data, SEED),
                Offset = U16(data, OFFSET),
                Raise = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(RAISE)),
                Gnarl = U16(data, GNARL),
                River = U16(data, RIVER),
                LRiver = U16(data, LRIVER),
                Source = (byte)U16(data, SOURCE),
                SnLin = U16(data, SNLIN),
                SnFlt = (byte)U16(data, SNFLT),
                BhLin = (byte)U16(data, BHLIN),
                BhFlt = (byte)U16(data, BHFLT),
                RkSte = (byte)U16(data, RKSTE),
                // The cave ceiling is generated from it, so it lives with the terrain parameters
                BasicHeight = data[BASIC_HEIGHT]
            }
        };

        for (int id = 1; id <= LevelFormat.MC2_MAX_ENTITIES; id++)
        {
            int pos = ENTITIES + id * ENTITY_SIZE;
            if (data.AsSpan(pos, ENTITY_SIZE).IndexOfAnyExcept((byte)0) < 0)
                continue;

            var typeId = (TypeId)U16(data, pos);
            ushort modelId = U16(data, pos + 2);

            map.Entities.Add(new Mc2Entity
            {
                Id = id,
                EntityType = typeId.GetEntityTypeOrUnknown(modelId, GameVersion.MagicCarpet2),
                Position = new Position(U16(data, pos + 4), U16(data, pos + 6)),
                DisId = U16(data, pos + 8),
                SwitchSize = U16(data, pos + 10),
                SwitchId = U16(data, pos + 12),
                Parent = U16(data, pos + 14),
                Child = U16(data, pos + 16),
                Par3 = U16(data, pos + 18)
            });
        }

        int activeWizards = (short)U16(data, NUMBER_OF_PLAYERS);
        for (int w = 0; w < map.Wizards.Length; w++)
        {
            var wizard = (Mc2Wizard)map.Wizards[w];
            int pos = WIZARDS + w * WIZARD_SIZE;

            wizard.IsActive = activeWizards > 0;
            wizard.Agression = (byte)U16(data, pos + WIZARD_AGGRESSION);
            wizard.Reflexes = (byte)U16(data, pos + WIZARD_REFLEXES);
            wizard.Perception = (byte)U16(data, pos + WIZARD_PERCEPTION);
            wizard.StartingSpells = data.AsSpan(pos + WIZARD_STARTING_SPELLS, LevelFormat.MC2_SPELL_COUNT).ToArray();
            wizard.SpellLevels = data.AsSpan(pos + WIZARD_SPELL_LEVELS, LevelFormat.MC2_SPELL_COUNT).ToArray();
            wizard.BlockedSpells = data.AsSpan(pos + WIZARD_BLOCKED_SPELLS, LevelFormat.MC2_SPELL_COUNT).ToArray();
            wizard.Life = (short)U16(data, pos + WIZARD_LIFE);
            wizard.CastleLevel = data[CASTLE_LEVELS + w];

            if (activeWizards > 0)
                activeWizards--;
        }

        for (int i = 0; i < LevelFormat.MC2_OBJECTIVE_COUNT; i++)
        {
            int pos = OBJECTIVES + i * OBJECTIVE_SIZE;
            map.Objectives.Add(new Objective
            {
                Type = (ObjectiveType)(sbyte)data[pos],
                Value = (short)U16(data, pos + 1),
                X = (short)U16(data, pos + 3),
                Y = (short)U16(data, pos + 5)
            });
        }

        for (int i = 0; i < LevelFormat.MC2_STAGE_VAR_COUNT; i++)
        {
            int pos = STAGE_VARS + i * STAGE_VAR_SIZE;
            map.StageVars.Add(new StageVar
            {
                TypeAndFlags = data[pos],
                NextStage = data[pos + 1],
                EntityIndex = U16(data, pos + 2),
                TargetX = U16(data, pos + 4),
                TargetY = U16(data, pos + 6)
            });
        }

        return map;
    }

    public static byte[] Write(Mc2Map map)
    {
        ArgumentNullException.ThrowIfNull(map);

        // Start from the original file so bytes the editor doesn't model are kept.
        var data = map.SourceData is { Length: LevelFormat.MC2_FILE_SIZE }
            ? (byte[])map.SourceData.Clone()
            : CreateBlank();

        var gp = map.Terrain.GenerationParameters;

        W16(data, LEVEL_ID, map.LevelId);
        data[LEVEL_TYPE] = map.LevelType;
        data[BASIC_HEIGHT] = map.BasicHeight;
        data[MAP_TYPE] = (byte)gp.MapType;
        W16(data, MANA_GOAL, map.ManaTarget);
        W16(data, NUMBER_OF_PLAYERS, (ushort)map.Wizards.Count(w => w.IsActive));

        W16(data, SEED, gp.Seed);
        W16(data, OFFSET, gp.Offset);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(RAISE), gp.Raise);
        W16(data, GNARL, gp.Gnarl);
        W16(data, RIVER, gp.River);
        W16(data, LRIVER, gp.LRiver);
        W16(data, SOURCE, gp.Source);
        W16(data, SNLIN, gp.SnLin);
        W16(data, SNFLT, gp.SnFlt);
        W16(data, BHLIN, gp.BhLin);
        W16(data, BHFLT, gp.BhFlt);
        W16(data, RKSTE, gp.RkSte);

        for (int id = 1; id <= LevelFormat.MC2_MAX_ENTITIES; id++)
        {
            int pos = ENTITIES + id * ENTITY_SIZE;
            var entity = map.GetEntity(id);

            if (entity is null)
            {
                data.AsSpan(pos, ENTITY_SIZE).Clear();
                continue;
            }

            W16(data, pos, (ushort)entity.EntityType.TypeId);
            W16(data, pos + 2, (ushort)entity.EntityType.Model.Id);
            W16(data, pos + 4, (ushort)entity.Position.X);
            W16(data, pos + 6, (ushort)entity.Position.Y);
            W16(data, pos + 8, entity.DisId);
            W16(data, pos + 10, entity.SwitchSize);
            W16(data, pos + 12, entity.SwitchId);
            W16(data, pos + 14, entity.Parent);
            W16(data, pos + 16, entity.Child);
            W16(data, pos + 18, (entity as Mc2Entity)?.Par3 ?? 0);
        }

        for (int w = 0; w < map.Wizards.Length; w++)
        {
            var wizard = map.Wizards[w];
            int pos = WIZARDS + w * WIZARD_SIZE;

            WriteByteStat(data, pos + WIZARD_AGGRESSION, wizard.Agression);
            WriteByteStat(data, pos + WIZARD_REFLEXES, wizard.Reflexes);
            WriteByteStat(data, pos + WIZARD_PERCEPTION, wizard.Perception);
            data[CASTLE_LEVELS + w] = wizard.CastleLevel;

            // A plain Wizard (no MC2 settings) leaves the file's spell settings and Life as they were
            if (wizard is Mc2Wizard mc2Wizard)
            {
                CopySpells(mc2Wizard.StartingSpells, data, pos + WIZARD_STARTING_SPELLS);
                CopySpells(mc2Wizard.SpellLevels, data, pos + WIZARD_SPELL_LEVELS);
                CopySpells(mc2Wizard.BlockedSpells, data, pos + WIZARD_BLOCKED_SPELLS);
                W16(data, pos + WIZARD_LIFE, (ushort)mc2Wizard.Life);
            }
        }

        for (int i = 0; i < LevelFormat.MC2_OBJECTIVE_COUNT; i++)
        {
            int pos = OBJECTIVES + i * OBJECTIVE_SIZE;
            var objective = i < map.Objectives.Count ? map.Objectives[i] : new Objective();

            data[pos] = (byte)(sbyte)objective.Type;
            W16(data, pos + 1, (ushort)objective.Value);
            W16(data, pos + 3, (ushort)objective.X);
            W16(data, pos + 5, (ushort)objective.Y);
        }

        for (int i = 0; i < LevelFormat.MC2_STAGE_VAR_COUNT; i++)
        {
            int pos = STAGE_VARS + i * STAGE_VAR_SIZE;
            var stageVar = i < map.StageVars.Count ? map.StageVars[i] : new StageVar();

            data[pos] = stageVar.TypeAndFlags;
            data[pos + 1] = stageVar.NextStage;
            W16(data, pos + 2, stageVar.EntityIndex);
            W16(data, pos + 4, stageVar.TargetX);
            W16(data, pos + 6, stageVar.TargetY);
        }

        return data;
    }

    private static byte[] CreateBlank()
    {
        var data = new byte[LevelFormat.MC2_FILE_SIZE];
        W16(data, 0, 2); // every original level starts with 2
        return data;
    }

    /// <summary>
    /// Wizard stats are 16-bit in MC2 files but bytes in the editor. Some original levels hold
    /// values above 255 (e.g. 0x7878 in unused wizard slots), so only overwrite when the user changed it.
    /// </summary>
    private static void WriteByteStat(byte[] data, int pos, byte value)
    {
        if ((byte)U16(data, pos) != value)
            W16(data, pos, value);
    }

    private static void CopySpells(byte[] spells, byte[] target, int pos)
    {
        var span = target.AsSpan(pos, LevelFormat.MC2_SPELL_COUNT);
        span.Clear();
        spells.AsSpan(0, Math.Min(spells.Length, LevelFormat.MC2_SPELL_COUNT)).CopyTo(span);
    }

    private static ushort U16(byte[] data, int pos) => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(pos));

    private static void W16(byte[] data, int pos, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(pos), value);
}
