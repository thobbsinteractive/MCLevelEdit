namespace MCLevelEdit.Model.Domain.Extensions;

public static class EntityTypeExtensions
{
    public static EntityType GetEntityFromTypeId(this TypeId typeId)
    {
        switch (typeId)
        {
            case TypeId.Scenary:
                return new ScenaryType(Scenary.Tree);
            case TypeId.Spawn:
                return new SpawnType(Spawn.Flyer1);
            case TypeId.Creature:
                return new CreatureType(Creature.Vulture);
            case TypeId.Effect:
                return new EffectType(Effect.Unknown0);
            case TypeId.Weather:
                return new WeatherType(Weather.Wind);
            case TypeId.Spell:
                return new SpellType(Spell.Fireball);
            case TypeId.Switch:
                return new SwitchType(Switch.HiddenInside);
        }
        return null;
    }

    public static EntityType GetEntityTypeFromTypeIdAndModelId(this TypeId typeId, int modelId)
    {
        switch (typeId)
        {
            case TypeId.Scenary:
                return EntityTypes.I.Sceneries[modelId];
            case TypeId.Spawn:
                return EntityTypes.I.Spawns[modelId];
            case TypeId.Creature:
                return EntityTypes.I.Creatures[modelId];
            case TypeId.Weather:
                return EntityTypes.I.Weathers[modelId];
            case TypeId.Effect:
                return EntityTypes.I.Effects[modelId];
            case TypeId.Switch:
                return EntityTypes.I.Switches[modelId];
            case TypeId.Spell:
                return EntityTypes.I.Spells[modelId];
        }
        return null;
    }
}
