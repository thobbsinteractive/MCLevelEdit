using MagicCarpet2Terrain.Model;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Abstractions;

public interface IMapService
{
    Task<bool> CreateNewMap(bool randomTerrain = false, ushort size = Globals.MAX_MAP_SIZE);
    Task<bool> LoadMapFromFileAsync(string filePath);
    Task<bool> SaveMapToFileAsync(string filePath);
    Task<bool> ValidateMapAsync();
    Task<bool> RecalculateTerrain(GenerationParameters generationParameters);
    Map GetMap();
    Entity? GetEntity(ushort id);
    List<Entity> GetEntitiesByCoords(int x, int y);
    List<Entity> GetEntitiesBySwitchId(ushort switchId, int excludeId = 0);
    List<Entity> GetEntitiesByTypeId(TypeId typeId);
    int AddEntity(Entity entity);
    bool UpdateEntity(Entity entity);
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
