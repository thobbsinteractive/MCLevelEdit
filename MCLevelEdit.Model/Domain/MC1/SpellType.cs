using System.Drawing;

namespace MCLevelEdit.Model.Domain.MC1;

public enum Spell
{
    Fireball = 0,
    Heal = 1,
    SpeedUp = 2,
    Possession = 3,
    Shield = 4,
    BeyondSight = 5,
    Earthquake = 6,
    Meteor = 7,
    Volcano = 8,
    Crater = 9,
    Teleport = 10,
    Duel = 11,
    Invisible = 12,
    StealMana = 13,
    Rebound = 14,
    Lightning = 15,
    Castle = 16,
    Skeleton = 17,
    Thunderbolt = 18,
    ManaMagnet = 19,
    FireWall = 20,
    ReverseSpeed = 21,
    GlobalDeath = 22,
    RapidFireball = 23
}

public class SpellType : EntityType
{
    public SpellType(Spell spell) : this(spell, GetName) { }

    private readonly Func<Spell, string> _getName;

    protected SpellType(Spell spell, Func<Spell, string> getName) : base(TypeId.Spell, ((int)spell), getName(spell))
    {
        _getName = getName;
    }

    /// <summary>The spell's name in Magic Carpet.</summary>
    public static string GetName(Spell spell) => spell.ToString();

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues<Spell>()
                    .Select(x => new ModelType() { Id = (int)x, Name = _getName(x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}

/// <summary>
/// Hidden Worlds spells: the same as Magic Carpet's except that Wall of Fire (<see cref="Spell.FireWall"/>)
/// is Homing Meteor.
/// </summary>
public class HiddenWorldsSpellType : SpellType
{
    public HiddenWorldsSpellType(Spell spell) : base(spell, GetHiddenWorldsName) { }

    /// <summary>The spell's name in Hidden Worlds.</summary>
    public static string GetHiddenWorldsName(Spell spell) => spell == Spell.FireWall ? "HomingMeteor" : GetName(spell);
}
