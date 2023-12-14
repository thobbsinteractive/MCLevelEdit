namespace MCLevelEdit.Model.Domain;

public class EntityTypes
{
    private static EntityTypes instance;

    public readonly Dictionary<int, EntityType> Spawns;
    public readonly Dictionary<int, EntityType> Sceneries;
    public readonly Dictionary<int, EntityType> Creatures;
    public readonly Dictionary<int, EntityType> Effects;
    public readonly Dictionary<int, EntityType> Spells;
    public readonly Dictionary<int, EntityType> Switches;
    public readonly Dictionary<int, EntityType> Weathers;

    public static EntityTypes I
    {
        get
        {
            if (instance == null)
                instance = new EntityTypes();

            return instance;
        }
    }

    public EntityTypes()
    {
        Spawns = new Dictionary<int, EntityType>();
        foreach (Spawn spawn in (Spawn[])Enum.GetValues(typeof(Spawn)))
        {
            Spawns.Add((int)spawn, new SpawnType(spawn));
        }

        Sceneries = new Dictionary<int, EntityType>();
        foreach (Scenary scenary in (Scenary[])Enum.GetValues(typeof(Scenary)))
        {
            Sceneries.Add((int)scenary, new ScenaryType(scenary));
        }

        Creatures = new Dictionary<int, EntityType>();
        foreach (Creature creature in (Creature[])Enum.GetValues(typeof(Creature)))
        {
            Creatures.Add((int)creature, new CreatureType(creature));
        }

        Effects = new Dictionary<int, EntityType>();
        foreach (Effect effect in (Effect[])Enum.GetValues(typeof(Effect)))
        {
            Effects.Add((int)effect, new EffectType(effect));
        }

        Spells = new Dictionary<int, EntityType>();
        foreach (Spell spell in (Spell[])Enum.GetValues(typeof(Spell)))
        {
            Spells.Add((int)spell, new SpellType(spell));
        }

        Switches = new Dictionary<int, EntityType>();
        foreach (Switch gameSwitch in (Switch[])Enum.GetValues(typeof(Switch)))
        {
            Switches.Add((int)gameSwitch, new SwitchType(gameSwitch));
        }

        Weathers = new Dictionary<int, EntityType>();
        foreach (Weather weather in (Weather[])Enum.GetValues(typeof(Weather)))
        {
            Weathers.Add((int)weather, new WeatherType(weather));
        }
    }
}

