using System.Drawing;

namespace MCLevelEdit.Model.Domain;

public enum Creature
{
    Dragon = 0,
    Vulture = 1,
    Bee = 2,
    Worm = 3,
    Archer = 4,
    Crab = 5,
    Kraken = 6,
    TrollOrApe = 7,
    Griffin = 8,
    Skeleton = 9,
    Emu = 10,
    Genie = 11,
    Builder = 12,
    Townie = 13,
    Trader = 14,
    Unknown = 15,
    Wyvern = 16
}

public class CreatureType : EntityType
{
    private static Dictionary<Creature, uint> ManaCost = new Dictionary<Creature, uint>()
    {
        { Creature.Dragon, 4500 },
        { Creature.Vulture, 1000 },
        { Creature.Bee, 1500 },
        { Creature.Worm, 4500 },
        { Creature.Archer, 500 },
        { Creature.Crab, 500 },
        { Creature.Kraken, 4500 },
        { Creature.TrollOrApe, 1500 },
        { Creature.Griffin, 5000 },
        { Creature.Skeleton, 500 },
        { Creature.Emu, 1000 },
        { Creature.Genie, 10000 },
        { Creature.Wyvern, 50000 },
    };

    public CreatureType(Creature creature) : base(TypeId.Creature,((int)creature), creature.ToString(), (ManaCost.ContainsKey(creature) ? ManaCost[creature] : 0)) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Creature))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, 
                        Name = Enum.GetName(typeof(Creature), x), 
                        Mana = (ManaCost.ContainsKey((Creature)x) ? ManaCost[(Creature)x] : 0) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
