using MCLevelEdit.Model.Domain.MC1; // MC1 effect IDs for paths, walls, canyons etc. are the same in MC2
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Domain.Validation;

namespace MCLevelEdit.Model.Domain;

public class Map
{
    public string FilePath {  get; set; }
    /// <summary>The game this level is for. Magic Carpet 2 levels are <see cref="MC2.Mc2Map"/>.</summary>
    public virtual GameVersion GameVersion => GameVersion.MagicCarpet1;
    public int MaxEntities => LevelFormat.GetMaxEntities(GameVersion);

    /// <summary>
    /// Whether this Magic Carpet format level is for Hidden Worlds. The level file can't say, so the editor
    /// keeps it while the level is open: set it with <see cref="SetHiddenWorlds"/>. Always false for MC2 levels.
    /// </summary>
    public bool IsHiddenWorlds { get; private set; }

    /// <summary>The game this level is for, and runs in. Magic Carpet 2 levels override it.</summary>
    public virtual GameTarget Game => IsHiddenWorlds ? GameTarget.HiddenWorlds : GameTarget.MagicCarpet1;

    /// <summary>The game whose terrain generation this level uses. Magic Carpet 2 levels override it.</summary>
    public virtual TerrainGame TerrainGame => IsHiddenWorlds ? TerrainGame.HiddenWorlds : TerrainGame.MagicCarpet1;

    /// <summary>Marks the level as Hidden Worlds (or Magic Carpet) and renames its spell jars to match.</summary>
    public void SetHiddenWorlds(bool hiddenWorlds)
    {
        if (GameVersion != GameVersion.MagicCarpet1)
            return;

        IsHiddenWorlds = hiddenWorlds;

        var spells = EntityCatalog.GetMc1EntityTypes(hiddenWorlds).Spells;
        foreach (var entity in Entities)
        {
            if (entity.EntityType?.TypeId == TypeId.Spell && spells.TryGetValue(entity.EntityType.Model.Id, out var spellType))
                entity.EntityType = spellType;
        }
    }
    public uint ManaTotal { get; set; }
    public byte ManaTarget { get; set; }
    public IList<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
    public IList<Entity> Entities { get; set; } = new List<Entity>();
    public Terrain Terrain { get; set; } = new Terrain();
    public Wizard[] Wizards { get; set; } = new[] {
        new Wizard()
        {
            Name = "Player",
            IsActive = true,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Vodor",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Gryshnak",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Mahmoud",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Syed",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Raschid",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Alhabbal",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        },
        new Wizard()
        {
            Name = "Scheherazade",
            IsActive = false,
            Agression = 128,
            Perception = 128,
            Reflexes = 128,
            CastleLevel = 0
        }
    };

    /// <summary>Entities shown in the editor: everything except inert type 0 leftovers.</summary>
    public IEnumerable<Entity> VisibleEntities => Entities.Where(e => !e.IsInert());

    public IList<Entity> GetEntitiesByPosition(Position postion)
    {
        return Entities.Where(e => e.Position == postion).ToList();
    }

    public void SetEntities(IList<Entity> entities)
    {
        Entities.Clear();
        foreach (Entity entity in entities)
            Entities.Add(entity);
    }

    public int AddEntity(Entity entity)
    {
        if (this.Entities.Count < MaxEntities)
        {
            entity.Id = GetNextId();
            this.Entities.Add(entity);
        }
        return entity.Id;
    }

    public void DeleteEntity(Entity entity)
    {
        int index = GetIndexOf(entity);
        if (index > -1)
            this.Entities.RemoveAt(index);
    }

    public void UpdateEntity(Entity entity)
    {
        int index = GetIndexOf(entity);
        if (index > -1)
            this.Entities[index] = entity;
    }

    public Entity? GetEntity(int id)
    {
        return this.Entities?.Where(e => e.Id == id).FirstOrDefault();
    }

    public IList<ValidationResult> ValidateEntities()
    {
        ValidationResults = new List<ValidationResult>();

        ValidationResults.Add(MapRules.HasPlayerSpawn(this.Entities));
        var spawnValidation = MapRules.HasCorrectNumberOfWizardSpawns(this);
        foreach (var spawnResult in spawnValidation)
            ValidationResults.Add(spawnResult);

        var wizardValidation = MapRules.HasValidWizardParamters(this);
        foreach (var wizardResult in wizardValidation)
            ValidationResults.Add(wizardResult);

        // The basic-spells check is for MC1 spell jars; MC2 levels use vases
        if (GameVersion == GameVersion.MagicCarpet1)
        {
            var spellValidation = MapRules.HasBasicSpells(this);
            foreach (var spellResult in spellValidation)
                ValidationResults.Add(spellResult);
        }

        var entities = VisibleEntities.ToList();
        foreach (Entity entity in entities)
        {
            ValidationResults.Add(EntityRules.HasSwitch(entity, entities));
            ValidationResults.Add(EntityRules.BuildingHasSwidAndDisIdAndParent(entity));
            ValidationResults.Add(EntityRules.PathEntityCannotSameChildAndParent(entity, (int)Effect.Wall));
            ValidationResults.Add(EntityRules.PathEntityCannotSameChildAndParent(entity, (int)Effect.Path));
            ValidationResults.Add(EntityRules.PathEntityCannotSameChildAndParent(entity, (int)Effect.Canyon));
            ValidationResults.Add(EntityRules.PathEntityCannotSameChildAndParent(entity, (int)Effect.RidgeNode));
            ValidationResults.Add(EntityRules.HasUniqueCoordinates(entity, entities));
            ValidationResults.Add(EntityRules.CheckConnectedWalls(entity, entities));
            ValidationResults.Add(EntityRules.TeleportDestinationCoordinatesAreDifferentToStart(entity));
        }
        
        return ValidationResults;
    }

    private int GetIndexOf(Entity entity)
    {
        for (int i = 0; i < this.Entities.Count; i++)
        {
            if(this.Entities[i].Id == entity.Id)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetNextId()
    {
        if (!this.Entities.Any())
            return 1;

        var entityIds = this.Entities.Select(e => e.Id).OrderBy(e => e).ToList();
        var allIds = Enumerable.Range(1, MaxEntities + 1).ToArray();
        var freeIds = allIds.Except(entityIds).ToList();

        return freeIds.FirstOrDefault();
    }
}
