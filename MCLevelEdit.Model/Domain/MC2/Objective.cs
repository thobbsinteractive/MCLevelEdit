namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>
/// Magic Carpet 2 objective types, from InitStages_58940 in remc2 (Level.cpp).
/// </summary>
public enum ObjectiveType : sbyte
{
    None = -1,
    CollectMana = 0,          // Value = mana goal %
    TargetEntity1 = 1,        // Value = entity index
    TargetEntity2 = 2,        // Value = entity index
    KillPlayer = 3,
    EntityAtPosition = 4,     // Value = entity index, plus X/Y
    ReleasePoint = 5,         // Value plus X/Y
    TargetEntity6 = 6,        // Value = entity index
    KillCreatureType = 7,     // Value = entity index; the target is that entity's creature subtype
    KillAllPlayers = 8,
    DestroyBuilding = 9       // Value = entity index of the building
}

/// <summary>
/// One of the 8 Magic Carpet 2 level objectives (type_str_0x36442 in remc2, 7 bytes).
/// The spoken narration for an objective is not stored here: the game picks a CD audio
/// segment from the level slot number and the objective's position in this list.
/// </summary>
public class Objective
{
    public ObjectiveType Type { get; set; } = ObjectiveType.None;
    public short Value { get; set; }
    public short X { get; set; }
    public short Y { get; set; }

    public Objective Copy() => new Objective { Type = Type, Value = Value, X = X, Y = Y };
}

/// <summary>What each objective type's Value, X and Y mean, for the editor and the level checks.</summary>
public static class ObjectiveTypeInfo
{
    /// <summary>Value is an entity's id (its slot in the level).</summary>
    public static bool UsesEntity(ObjectiveType type) => type is ObjectiveType.TargetEntity1 or ObjectiveType.TargetEntity2
        or ObjectiveType.TargetEntity6 or ObjectiveType.EntityAtPosition or ObjectiveType.KillCreatureType or ObjectiveType.DestroyBuilding;

    /// <summary>X and Y are a place on the map (tiles, 0 to 255).</summary>
    public static bool UsesPosition(ObjectiveType type) => type is ObjectiveType.EntityAtPosition or ObjectiveType.ReleasePoint;

    /// <summary>Value is a wizard's number, from 1 (the player): 2 is Wizard 2.</summary>
    public static bool UsesWizard(ObjectiveType type) => type == ObjectiveType.KillPlayer;

    /// <summary>Value is the share of the map's mana to collect, in percent.</summary>
    public static bool UsesPercent(ObjectiveType type) => type == ObjectiveType.CollectMana;

    /// <summary>Value is used, but what it means is not known (ReleasePoint: 0 in every level seen).</summary>
    public static bool UsesRawValue(ObjectiveType type) => type == ObjectiveType.ReleasePoint;

    public static string Name(ObjectiveType type) => type switch
    {
        ObjectiveType.None => "(empty)",
        ObjectiveType.CollectMana => "Collect mana",
        ObjectiveType.TargetEntity1 => "Target entity (1)",
        ObjectiveType.TargetEntity2 => "Target entity (2)",
        ObjectiveType.KillPlayer => "Kill wizard",
        ObjectiveType.EntityAtPosition => "Entity at position",
        ObjectiveType.ReleasePoint => "Release point",
        ObjectiveType.TargetEntity6 => "Target entity (6)",
        ObjectiveType.KillCreatureType => "Kill creature type",
        ObjectiveType.KillAllPlayers => "Kill all wizards",
        ObjectiveType.DestroyBuilding => "Destroy building",
        _ => type.ToString()
    };

    public static string Help(ObjectiveType type) => type switch
    {
        ObjectiveType.None => "An empty slot: no objective.",
        ObjectiveType.CollectMana => "Collect this share of the map's mana (percent).",
        ObjectiveType.TargetEntity1 or ObjectiveType.TargetEntity2 or ObjectiveType.TargetEntity6 =>
            "A goal about one entity (usually a creature). How types 1, 2 and 6 differ is not known yet.",
        ObjectiveType.KillPlayer => "Kill one wizard (its number: 1 is the player, 2 is Wizard 2).",
        ObjectiveType.EntityAtPosition => "An entity has to reach a place on the map.",
        ObjectiveType.ReleasePoint => "A place on the map. Its Value is 0 in every level seen.",
        ObjectiveType.KillCreatureType => "Kill every creature of the same kind as the entity chosen.",
        ObjectiveType.KillAllPlayers => "Kill all the other wizards.",
        ObjectiveType.DestroyBuilding => "Destroy one building (a VillagerBuilding effect).",
        _ => string.Empty
    };
}
