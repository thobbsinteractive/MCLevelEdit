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
