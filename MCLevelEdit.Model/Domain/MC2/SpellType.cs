namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>
/// Magic Carpet 2's 26 spells, in game order. Used for spell vases (entity type 15)
/// and for the order of each wizard's spell arrays.
/// </summary>
public enum Spell
{
    Fireball = 0,
    Possession = 1,
    Castle = 2,
    SpeedUp = 3,
    Morph = 4,
    Heal = 5,
    Shield = 6,
    Lightning = 7,
    Rebound = 8,
    Meteor = 9,
    Teleport = 10,
    Invisible = 11,
    StealMana = 12,
    BeyondSight = 13,
    Duel = 14,
    Tremor = 15,
    Crater = 16,
    Earthquake = 17,
    Volcano = 18,
    SummonArmy = 19,
    GravityWell = 20,
    Whirlwind = 21,
    FoolsMana = 22,
    MagicMine = 23,
    Alliance = 24,
    CaveIn = 25
}

/// <summary>A spell vase: MC2's equivalent of MC1's spell jar.</summary>
public class SpellType : EntityType
{
    public SpellType(Spell spell) : base(TypeId.Vase, (int)spell, spell.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Spell>();
}
