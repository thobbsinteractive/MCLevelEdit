using Avalonia.Media.Imaging;
using MagicCarpet2Terrain.Model;
using MCLevelEdit.Application.Extensions;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Repository;

namespace MCLevelEdit.Application.Services;

public class MapService : IMapService
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
        MapRepository.Map = await _filePort.LoadMapAsync(filePath);
        MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(MapRepository.Map.Terrain.GenerationParameters);
        return true;
    }

    public Task<WriteableBitmap> DrawBitmapAsync(WriteableBitmap bitmap, IList<Entity> entities)
    {
        return Task.Run(() =>
        {
            for (int i = 0; i < entities.Count; i++)
            {
                DrawEntity(entities[i], bitmap);
            }

            //var result = SaveBitmap(bitmap).Result;

            return bitmap;
        });
    }

    public void DrawEntity(Entity entity, WriteableBitmap bitmap)
    {
        using (var fb = bitmap.Lock())
        {
            for (int sx = 0; sx < Globals.SQUARE_SIZE; sx++)
            {
                for (int sy = 0; sy < Globals.SQUARE_SIZE; sy++)
                {
                    fb.SetPixel((entity.Position.X * Globals.SQUARE_SIZE) + sx, (entity.Position.Y * Globals.SQUARE_SIZE) + sy, entity.EntityType.Colour);
                }
            }
        }
    }

    public Task<bool> CreateNewMap(ushort size = Globals.MAX_MAP_SIZE)
    {
        MapRepository.Map = new Map();
        _eventAggregator.RaiseEvent("RefreshEntities", this, new PubSubEventArgs<object>("RefreshEntities"));
        _eventAggregator.RaiseEvent("RefreshWorld", this, new PubSubEventArgs<object>("RefreshWorld"));
        return RecalculateTerrain(MapRepository.Map.Terrain.GenerationParameters);
    }

    public async Task<bool> RecalculateTerrain(GenerationParameters generationParameters)
    {
        MapRepository.Map.Terrain = await _terrainService.CalculateMc2Terrain(generationParameters);
        _eventAggregator.RaiseEvent("RefreshTerrain", this, new PubSubEventArgs<object>("RefreshTerrain"));
        return true;
    }

    public Map GetMap()
    {
        return MapRepository.Map;
    }

    public Entity? GetEntity(ushort id)
    {
        return MapRepository.Map.Entities.First(e => e.Id == id);
    }

    public int AddEntity(Entity entity)
    {
        return MapRepository.Map.AddEntity(entity);
    }

    public bool UpdateEntity(Entity entity)
    {
        MapRepository.Map.UpdateEntity(entity);
        return true;
    }

    public bool DeleteEntity(Entity entity)
    {
        MapRepository.Map.DeleteEntity(entity);
        return true;
    }

    public List<Entity> GetEntitiesByCoords(int x, int y)
    {
        return MapRepository.Map.Entities.Where(e => e.Position.X == x && e.Position.Y == y).ToList();
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

    public bool UpdateWizard(Wizard wizard)
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
                wizardToUpdate.Spells.Possession = wizard.Spells.Possession;
                wizardToUpdate.Spells.Health = wizard.Spells.Health;
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
                wizardToUpdate.Spells.Lightning = wizard.Spells.Lightning;
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

    public uint CalculateMana()
    {
        uint total = 0;

        if (MapRepository.Map?.Entities.Count() > 0)
        {
            foreach (var entity in MapRepository.Map.Entities)
            {
                if (entity.EntityType.TypeId == TypeId.Creature)
                {
                    total += entity.EntityType.Model.Mana;
                }

                if (entity.EntityType.TypeId == TypeId.Effect)
                {
                    if (((Effect)entity.EntityType.Model.Id) == Effect.ManaBall || ((Effect)entity.EntityType.Model.Id) == Effect.VillagerBuilding)
                        total += 512;
                }
            }
        }

        return total;
    }

    public void UpdateMana(uint manaTotal)
    {
        MapRepository.Map.ManaTotal = manaTotal;
    }
}