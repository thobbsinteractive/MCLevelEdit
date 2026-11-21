namespace MCLevelEdit.Model.Domain.Shared;

/// <summary>
/// Entity types by type and kind, for the lookup tables both games have.
/// Each game's EntityTypes adds its own: Weathers for MC1, Specials for MC2.
/// </summary>
public abstract class EntityTypesBase
{
    public readonly Dictionary<int, EntityType> Spawns;
    public readonly Dictionary<int, EntityType> Sceneries;
    public readonly Dictionary<int, EntityType> Creatures;
    public readonly Dictionary<int, EntityType> Effects;
    public readonly Dictionary<int, EntityType> Switches;

    /// <summary>Spell pickups: MC1 spell jars (type 12) or MC2 spell vases (type 15).</summary>
    public readonly Dictionary<int, EntityType> Spells;

    /// <summary>The entity type ID used for <see cref="Spells"/> in this game.</summary>
    protected abstract TypeId SpellTypeId { get; }

    protected EntityTypesBase(
        GameVersion game,
        Dictionary<int, EntityType> sceneries,
        Dictionary<int, EntityType> creatures,
        Dictionary<int, EntityType> effects,
        Dictionary<int, EntityType> switches,
        Dictionary<int, EntityType> spells)
    {
        // Spawns are identical in both games
        Spawns = Build<Spawn>(spawn => new SpawnType(spawn, game));
        Sceneries = sceneries;
        Creatures = creatures;
        Effects = effects;
        Switches = switches;
        Spells = spells;
    }

    /// <summary>The kinds of the given entity type, or null if this game has no such type.</summary>
    public virtual Dictionary<int, EntityType>? ForType(TypeId typeId)
    {
        if (typeId == SpellTypeId)
            return Spells;

        return typeId switch
        {
            TypeId.Scenery => Sceneries,
            TypeId.Spawn => Spawns,
            TypeId.Creature => Creatures,
            TypeId.Effect => Effects,
            TypeId.Switch => Switches,
            _ => null
        };
    }

    /// <summary>One entity type per value of a kind enum, keyed by the value.</summary>
    protected static Dictionary<int, EntityType> Build<TEnum>(Func<TEnum, EntityType> create) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().ToDictionary(x => Convert.ToInt32(x), create);
}
