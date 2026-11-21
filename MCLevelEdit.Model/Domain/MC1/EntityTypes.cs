using MCLevelEdit.Model.Domain.Shared;

namespace MCLevelEdit.Model.Domain.MC1;

/// <summary>Magic Carpet 1 entity types by type and kind, built from the MC1 enums.</summary>
public class EntityTypes : EntityTypesBase
{
    private static EntityTypes? instance;

    public static EntityTypes I => instance ??= new EntityTypes();

    public readonly Dictionary<int, EntityType> Weathers;

    protected override TypeId SpellTypeId => TypeId.Spell;

    public EntityTypes() : base(
        GameVersion.MagicCarpet1,
        sceneries: Build<Scenery>(x => new SceneryType(x)),
        creatures: Build<Creature>(x => new CreatureType(x)),
        effects: Build<Effect>(x => new EffectType(x)),
        switches: Build<Switch>(x => new SwitchType(x)),
        spells: Build<Spell>(x => new SpellType(x)))
    {
        Weathers = Build<Weather>(x => new WeatherType(x));
    }

    public override Dictionary<int, EntityType>? ForType(TypeId typeId) =>
        typeId == TypeId.Weather ? Weathers : base.ForType(typeId);
}
