using Mc1 = MCLevelEdit.Model.Domain.MC1;
using Mc2 = MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.Model.Domain;

/// <summary>
/// The entity types and kinds each game supports, for lists, dropdowns and lookups.
/// Magic Carpet 1 entities live in <see cref="Mc1"/>, Magic Carpet 2 entities in <see cref="Mc2"/>.
/// </summary>
public static class EntityCatalog
{
    private static readonly TypeId[] Mc1TypeIds =
        { TypeId.Scenery, TypeId.Spawn, TypeId.Creature, TypeId.Weather, TypeId.Effect, TypeId.Switch, TypeId.Spell };

    private static readonly TypeId[] Mc2TypeIds =
        { TypeId.Scenery, TypeId.Spawn, TypeId.Creature, TypeId.Effect, TypeId.Switch, TypeId.Special, TypeId.Vase };

    public static IReadOnlyList<TypeId> GetTypeIds(GameVersion game) =>
        game == GameVersion.MagicCarpet2 ? Mc2TypeIds : Mc1TypeIds;

    /// <summary>The known kinds of an entity type for the given game (empty if the game has no such type).</summary>
    public static ModelType[] GetModelTypes(GameVersion game, TypeId typeId) =>
        GetKinds(game, typeId)?.Values.FirstOrDefault()?.ModelTypes ?? Array.Empty<ModelType>();

    /// <summary>The entity types of one type, keyed by kind, or null if the game has no such type.</summary>
    public static Dictionary<int, EntityType>? GetKinds(GameVersion game, TypeId typeId) =>
        GetEntityTypes(game).ForType(typeId);

    /// <summary>
    /// The entity type lookup tables for a game. Magic Carpet format levels marked as Hidden Worlds
    /// (see <see cref="Map.IsHiddenWorlds"/>) get the Hidden Worlds tables, which name one spell differently.
    /// </summary>
    public static EntityTypesBase GetEntityTypes(GameVersion game) =>
        game == GameVersion.MagicCarpet2 ? Mc2.EntityTypes.I : GetMc1EntityTypes(Repository.MapRepository.Map?.IsHiddenWorlds ?? false);

    /// <summary>The Magic Carpet tables, or the Hidden Worlds ones.</summary>
    public static Mc1.EntityTypes GetMc1EntityTypes(bool hiddenWorlds) =>
        hiddenWorlds ? Mc1.HiddenWorldsEntityTypes.I : Mc1.EntityTypes.I;
}
