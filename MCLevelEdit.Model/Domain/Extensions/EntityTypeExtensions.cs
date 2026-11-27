using MCLevelEdit.Model.Repository;

namespace MCLevelEdit.Model.Domain.Extensions;

/// <summary>
/// Entity type lookups. When no game is given, the game of the map that is currently open is used.
/// </summary>
public static class EntityTypeExtensions
{
    private static GameVersion CurrentGame => MapRepository.Map?.GameVersion ?? GameVersion.MagicCarpet1;

    /// <summary>A default entity of the given type (its first kind), or null if the game has no such type.</summary>
    public static EntityType? GetEntityFromTypeId(this TypeId typeId, GameVersion? game = null) =>
        EntityCatalog.GetKinds(game ?? CurrentGame, typeId)?.Values.FirstOrDefault();

    /// <summary>
    /// Like <see cref="GetEntityTypeFromTypeIdAndModelId"/>, but never returns null: entity types the
    /// editor has no name for yet are kept with their raw IDs so they survive a load and save.
    /// </summary>
    public static EntityType GetEntityTypeOrUnknown(this TypeId typeId, int modelId, GameVersion? game = null) =>
        typeId.GetEntityTypeFromTypeIdAndModelId(modelId, game)
        ?? new EntityType(typeId, modelId, $"Type{(int)typeId}-{modelId}") { Game = game ?? CurrentGame };

    public static EntityType? GetEntityTypeFromTypeIdAndModelId(this TypeId typeId, int modelId, GameVersion? game = null)
    {
        if (modelId < 0)
            return null;

        var version = game ?? CurrentGame;
        var kinds = EntityCatalog.GetKinds(version, typeId);
        if (kinds is null)
            return null;

        // Model IDs the editor has no name for yet are kept as-is so they survive a load and save.
        return kinds.TryGetValue(modelId, out var entityType)
            ? entityType
            : new EntityType(typeId, modelId, $"Unknown{modelId}") { Game = version };
    }
}
