using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface IMapService
{
    Task<bool> CreateNewMap(bool randomTerrain = false, ushort size = Globals.MAX_MAP_SIZE, GameVersion game = GameVersion.MagicCarpet1, MapType mapType = MapType.Day, bool hiddenWorlds = false);
    Task<bool> LoadMapFromFileAsync(string filePath);
    Task<bool> SaveMapToFileAsync(string filePath);
    Task<bool> ValidateMapAsync();
    Task<bool> RecalculateTerrain(GenerationParameters generationParameters);
    Map GetMap();
    Entity? GetEntity(ushort id);
    List<Entity> GetEntities();
    List<Entity> GetEntitiesByCoords(int x, int y);
    List<Entity> GetEntitiesBySwitchId(ushort switchId, int excludeId = 0);
    ushort GetMaxSwitchId();
    List<Entity> GetEntitiesByTypeId(TypeId typeId);
    int AddEntity(Entity entity);
    bool UpdateEntity(Entity entity);
    /// <summary>
    /// Updates the entity; with <paramref name="deferTerrain"/> a wall, path or building leaves the terrain as it is
    /// (while it is being dragged) until <see cref="CommitTerrain"/>.
    /// </summary>
    bool UpdateEntity(Entity entity, bool deferTerrain);
    /// <summary>Builds the terrain again if an update deferred it (the end of a drag).</summary>
    void CommitTerrain();
    bool DeleteEntity(Entity entity);
    bool UpdateManaTotal(uint manaTotal);
    bool UpdateManaTarget(byte manaTarget);
    bool SetActiveWizards(byte numOfWizards);
    List<Wizard> GetActiveWizards();
    bool UpdateWizard(Wizard wizard);
    uint CalculateMana();
    void UpdateMana(uint manaTotal);
    IList<ValidationResult> GetValidationResults(Result filter = Result.None);
}
