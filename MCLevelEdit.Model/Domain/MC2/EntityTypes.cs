namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>Magic Carpet 2 entity types by type and kind, built from the MC2 enums.</summary>
public class EntityTypes : EntityTypesBase
{
    private static EntityTypes? instance;

    public static EntityTypes I => instance ??= new EntityTypes();

    public readonly Dictionary<int, EntityType> Specials;

    // MC2 spells are found in vases
    protected override TypeId SpellTypeId => TypeId.Vase;

    public EntityTypes() : base(
        GameVersion.MagicCarpet2,
        sceneries: Build<Scenery>(x => new SceneryType(x)),
        creatures: Build<Creature>(x => new CreatureType(x)),
        effects: Build<Effect>(x => new EffectType(x)),
        switches: Build<Switch>(x => new SwitchType(x)),
        spells: Build<Spell>(x => new SpellType(x)))
    {
        Specials = Build<Special>(x => new SpecialType(x));
    }

    public override Dictionary<int, EntityType>? ForType(TypeId typeId) =>
        typeId == TypeId.Special ? Specials : base.ForType(typeId);
}
