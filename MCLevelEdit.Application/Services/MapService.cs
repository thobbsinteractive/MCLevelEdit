using MCLevelEdit.Model.Domain.MC1; // MC1 effect IDs for paths, walls, canyons etc. are the same in MC2
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using Mc2Wizard = MCLevelEdit.Model.Domain.MC2.Mc2Wizard;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.Model.Repository;
using Splat;

namespace MCLevelEdit.Application.Services;

public class MapService : IMapService, IEnableLogger
{
    private readonly IFilePort _filePort;
    private readonly ITerrainService _terrainService;
    protected readonly EventAggregator<object> _eventAggregator;

    public MapService(EventAggregator<object> eventAggregator, ITerrainService terrainService, IFilePort filePort)
    {
        _filePort = filePort;
        _terrainService = terrainService;
        _eventAggregator = eventAggregator;
    }

    public async Task<bool> LoadMapFromFileAsync(string filePath)
    {
        try
        {
            MapRepository.Map = await _filePort.LoadMapAsync(filePath);
            await GenerateTerrain(MapRepository.Map.Terrain.GenerationParameters);
            MapRepository.Map.ValidateEntities();
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error loading level {filePath}:\n{ex.Message}");
            return false;
        }
        return true;
    }

    public async Task<bool> SaveMapToFileAsync(string filePath)
    {
        try
        {
            // Magic Carpet 2 levels have no mana total and set their goals through objectives
            if (MapRepository.Map.GameVersion == GameVersion.MagicCarpet1)
            {
                if (MapRepository.Map.ManaTotal == 0)
                {
                    MapRepository.Map.ManaTotal = CalculateMana();
                }

                if (MapRepository.Map.ManaTarget == 0)
                {
                    MapRepository.Map.ManaTarget = 35;
                }
            }
            MapRepository.Map.ValidateEntities();

            var result = await _filePort.SaveMapAsync(MapRepository.Map, filePath);
            _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
            return result;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error saving level {filePath}:\n{ex.Message}");
            return false;
        }
    }

    public async Task<bool> ValidateMapAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                MapRepository.Map.ValidateEntities();
            });
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error validating level:\n{ex.Message}");
            return false;
        }
        return true;
    }

    public async Task<bool> CreateNewMap(bool randomTerrain = false, ushort size = Globals.MAX_MAP_SIZE, GameVersion game = GameVersion.MagicCarpet1, MapType mapType = MapType.Day, bool hiddenWorlds = false)
    {
        try
        {
            // Hidden Worlds levels use the Magic Carpet 1 format
            MapRepository.Map = game == GameVersion.MagicCarpet2 ? new MCLevelEdit.Model.Domain.MC2.Mc2Map() : new Map();
            MapRepository.Map.SetHiddenWorlds(hiddenWorlds);
            _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
            if (randomTerrain)
                MapRepository.Map.Terrain.GenerationParameters = _terrainService.GetRandomGeneratorParamters();

            // Only Magic Carpet 2 levels have a map type
            var gp = MapRepository.Map.Terrain.GenerationParameters;
            gp.MapType = game == GameVersion.MagicCarpet2 ? mapType : MapType.Day;
            if (gp.MapType == MapType.Cave && gp.BasicHeight == 0)
            {
                // The cave roof: with none, the whole cave is solid rock. MC2's own caves use around 114-154.
                gp.BasicHeight = (byte)Random.Shared.Next(110, 161);
            }

            return await RecalculateTerrain(MapRepository.Map.Terrain.GenerationParameters);
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Creating level:\n{ex.Message}");
            return false;
        }
    }

    public async Task<bool> RecalculateTerrain(GenerationParameters generationParameters)
    {
        try
        {
            await GenerateTerrain(generationParameters);
            _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Recalculating level:\n{ex.Message}");
            return false;
        }
        return true;
    }

    /// <summary>Generates the map's terrain, then builds its walls, paths and buildings into it.</summary>
    private async Task GenerateTerrain(GenerationParameters generationParameters)
    {
        var map = MapRepository.Map;
        map.GeneratedTerrain = await _terrainService.CalculateTerrain(generationParameters, map.TerrainGame);
        map.Terrain = _terrainService.BuildFeatures(map.GeneratedTerrain, map);
    }

    /// <summary>Builds the walls, paths and buildings into the generated terrain again, after one changed.</summary>
    private void RebuildFeatures()
    {
        var map = MapRepository.Map;
        if (map.GeneratedTerrain is null)
            return;
        map.Terrain = _terrainService.BuildFeatures(map.GeneratedTerrain, map);
        _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
    }

    /// <summary>Whether the entity is a wall, path or building, which can change the terrain when the level loads.</summary>
    private static bool ChangesTerrain(Entity? entity) => TerrainService.ShapesTerrain(entity);

    public Map GetMap()
    {
        return MapRepository.Map;
    }

    public Entity? GetEntity(ushort id)
    {
        try
        {
            return MapRepository.Map.Entities.FirstOrDefault(e => e.Id == id);
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Getting entity {id}:\n{ex.Message}");
            return default;
        }
    }

    public int AddEntity(Entity entity)
    {
        try
        {
            int id = MapRepository.Map.AddEntity(entity);
            if (ChangesTerrain(entity))
                RebuildFeatures();
            return id;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Adding:\n{ex.Message}");
            return default;
        }
    }

    public bool UpdateEntity(Entity entity)
    {
        try
        {
            bool wasFeature = ChangesTerrain(MapRepository.Map.GetEntity(entity.Id));
            MapRepository.Map.UpdateEntity(entity);
            if (wasFeature || ChangesTerrain(entity))
                RebuildFeatures();
            return true;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Updating entity {entity?.Id}:\n{ex.Message}");
            return default;
        }
    }

    public bool DeleteEntity(Entity entity)
    {
        try
        {
            bool wasFeature = ChangesTerrain(MapRepository.Map.GetEntity(entity.Id));
            MapRepository.Map.DeleteEntity(entity);
            if (wasFeature)
                RebuildFeatures();
            return true;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Deleting entity {entity?.Id}:\n{ex.Message}");
            return default;
        }
    }

    public List<Entity> GetEntities()
    {
        return MapRepository.Map.VisibleEntities.ToList();
    }

    public List<Entity> GetEntitiesByCoords(int x, int y)
    {
        return MapRepository.Map.VisibleEntities.Where(e => e.Position.X == x && e.Position.Y == y).ToList();
    }

    public List<Entity> GetEntitiesBySwitchId(ushort switchId, int excludeId = 0)
    {
        if (switchId > 0 && MapRepository.Map.Entities.Count > 0)
        {
            return MapRepository.Map.VisibleEntities.Where(e => e.Id != excludeId && e.SwitchId == switchId && e.DisId < ushort.MaxValue).ToList();
        }
        else
            return new List<Entity>();
    }

    public ushort GetMaxSwitchId()
    {
        var switches = MapRepository.Map.Entities.Where(e => e.EntityType.TypeId == TypeId.Switch);
        return switches.Any() ? switches.Max(e => e.SwitchId): (ushort)1;
    }

    public List<Entity> GetEntitiesByTypeId(TypeId typeId)
    {
        if (typeId > 0)
        {
            return MapRepository.Map.Entities.Where(e => e.EntityType.TypeId == typeId).ToList();
        }
        else
            return new List<Entity>();
    }

    public bool UpdateManaTotal(uint manaTotal)
    {
        MapRepository.Map.ManaTotal = manaTotal;
        return true;
    }

    public bool UpdateManaTarget(byte manaTarget)
    {
        MapRepository.Map.ManaTarget = manaTarget;
        return true;
    }

    public bool SetActiveWizards(byte numWizards)
    {
        if (numWizards == 0)
            numWizards = 1;

        if (numWizards > 8)
            numWizards = 8;

        foreach (var wizard in MapRepository.Map.Wizards)
        {
            wizard.IsActive = false;
            wizard.IsActive = numWizards > 0;

            if (numWizards > 0)
                numWizards--;
        }

        return true;
    }

    public List<Wizard> GetActiveWizards()
    {
        return MapRepository.Map.Wizards.Where(w => w.IsActive).ToList();
    }

    public bool UpdateWizard(Wizard wizard)
    {
        try
        {
            var wizardToUpdate = MapRepository.Map.Wizards.Where(w => w.Name.Equals(wizard.Name)).FirstOrDefault();

            if (wizardToUpdate != null)
            {
                wizardToUpdate.Agression = wizard.Agression;
                wizardToUpdate.Perception = wizard.Perception;
                wizardToUpdate.Reflexes = wizard.Reflexes;
                wizardToUpdate.CastleLevel = wizard.CastleLevel;

                // MC2 wizards also carry spell settings and Life (only taken when all 26 spells are supplied)
                if (wizardToUpdate is Mc2Wizard mc2Target && wizard is Mc2Wizard mc2Source
                    && mc2Source.StartingSpells?.Length == LevelFormat.MC2_SPELL_COUNT)
                {
                    mc2Target.Life = mc2Source.Life;
                    mc2Target.StartingSpells = mc2Source.StartingSpells;
                    mc2Target.SpellLevels = mc2Source.SpellLevels;
                    mc2Target.BlockedSpells = mc2Source.BlockedSpells;
                }

                if (wizardToUpdate.Spells != null)
                {
                    wizardToUpdate.Spells.Fireball = wizard.Spells.Fireball;
                    wizardToUpdate.Spells.Shield = wizard.Spells.Shield;
                    wizardToUpdate.Spells.Accelerate = wizard.Spells.Accelerate;
                    wizardToUpdate.Spells.Possess = wizard.Spells.Possess;
                    wizardToUpdate.Spells.Heal = wizard.Spells.Heal;
                    wizardToUpdate.Spells.BeyondSight = wizard.Spells.BeyondSight;
                    wizardToUpdate.Spells.Earthquake = wizard.Spells.Earthquake;
                    wizardToUpdate.Spells.Meteor = wizard.Spells.Meteor;
                    wizardToUpdate.Spells.Volcano = wizard.Spells.Volcano;
                    wizardToUpdate.Spells.Crater = wizard.Spells.Crater;
                    wizardToUpdate.Spells.Teleport = wizard.Spells.Teleport;
                    wizardToUpdate.Spells.Duel = wizard.Spells.Duel;
                    wizardToUpdate.Spells.Invisible = wizard.Spells.Invisible;
                    wizardToUpdate.Spells.StealMana = wizard.Spells.StealMana;
                    wizardToUpdate.Spells.Rebound = wizard.Spells.Rebound;
                    wizardToUpdate.Spells.LightningBolt = wizard.Spells.LightningBolt;
                    wizardToUpdate.Spells.Castle = wizard.Spells.Castle;
                    wizardToUpdate.Spells.UndeadArmy = wizard.Spells.UndeadArmy;
                    wizardToUpdate.Spells.LightningStorm = wizard.Spells.LightningStorm;
                    wizardToUpdate.Spells.ManaMagnet = wizard.Spells.ManaMagnet;
                    wizardToUpdate.Spells.WallofFire = wizard.Spells.WallofFire;
                    wizardToUpdate.Spells.ReverseAcceleration = wizard.Spells.ReverseAcceleration;
                    wizardToUpdate.Spells.GlobalDeath = wizard.Spells.GlobalDeath;
                    wizardToUpdate.Spells.RapidFireball = wizard.Spells.RapidFireball;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Updating Wizards:\n{ex.Message}");
            return false;
        }
    }

    public uint CalculateMana()
    {
        try
        {
            //Player starts with 1000 mana
            uint total = 1000;

            if (MapRepository.Map?.Entities.Count() > 0)
            {
                var modelCountsDic = new Dictionary<string, int>();

                foreach (var entity in MapRepository.Map.Entities)
                {
                    string key = $"Id {entity.EntityType.Model.Id}: {entity.EntityType.Model.Name}";
                    if (!modelCountsDic.ContainsKey(key))
                        modelCountsDic.Add(key, 1);
                    else
                        modelCountsDic[key]++;

                    if (entity.EntityType.TypeId == TypeId.Creature)
                    {
                        total += entity.EntityType.Model.Mana;
                    }

                    if (entity.EntityType.TypeId == TypeId.Effect)
                    {
                        if ((Effect)entity.EntityType.Model.Id == Effect.ManaBall || (Effect)entity.EntityType.Model.Id == Effect.VillagerBuilding)
                            total += 512;
                    }
                }
            }

            var activeWizards = MapRepository.Map.Wizards.Where(w => w.IsActive && w.Name != "Player");

            if (activeWizards?.Count() > 1)
            {
                foreach (var wizard in activeWizards)
                {
                    var wizardTotal = 1000u;
                    if (wizard.CastleLevel > 0)
                    {
                        var castleTotal = 5000u;
                        for (int i = 1; i < wizard.CastleLevel; i++)
                        {
                            castleTotal += castleTotal;
                        }
                        wizardTotal += castleTotal;
                    }
                    total += wizardTotal;
                }
            }

            return total;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Calculating mana:\n{ex.Message}");
            return default;
        }
    }

    public void UpdateMana(uint manaTotal)
    {
        MapRepository.Map.ManaTotal = manaTotal;
    }

    public IList<ValidationResult> GetValidationResults(Result filter = Result.None)
    {
        if (filter == Result.None)
            return MapRepository.Map.ValidationResults;
        else
            return MapRepository.Map.ValidationResults.Where(r => r.Result == filter).ToList();
    }
}