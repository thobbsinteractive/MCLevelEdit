﻿using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
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
            MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(MapRepository.Map.Terrain.GenerationParameters);
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
            if (MapRepository.Map.ManaTotal == 0)
            {
                MapRepository.Map.ManaTotal = CalculateMana();
            }

            if (MapRepository.Map.ManaTarget == 0)
            {
                MapRepository.Map.ManaTarget = 35;
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

    public async Task<bool> CreateNewMap(bool randomTerrain = false, ushort size = Globals.MAX_MAP_SIZE)
    {
        try
        {
            MapRepository.Map = new Map();
            _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
            _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
            if (randomTerrain)
                MapRepository.Map.Terrain.GenerationParameters = _terrainService.GetRandomGeneratorParamters();

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
            MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(generationParameters);
            _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Error Recalculating level:\n{ex.Message}");
            return false;
        }
        return true;
    }

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
            return MapRepository.Map.AddEntity(entity);
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
            MapRepository.Map.UpdateEntity(entity);
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
            MapRepository.Map.DeleteEntity(entity);
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
        return MapRepository.Map.Entities.ToList();
    }

    public List<Entity> GetEntitiesByCoords(int x, int y)
    {
        return MapRepository.Map.Entities.Where(e => e.Position.X == x && e.Position.Y == y).ToList();
    }

    public List<Entity> GetEntitiesBySwitchId(ushort switchId, int excludeId = 0)
    {
        if (switchId > 0 && MapRepository.Map.Entities.Count > 0)
        {
            return MapRepository.Map.Entities.Where(e => e.Id != excludeId && e.SwitchId == switchId && e.DisId < ushort.MaxValue).ToList();
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