namespace MCLevelEdit.Model.Domain.MC2;

public enum Creature
{
    Dragon = 0,
    Goat = 1,
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
    Unknown15 = 15,
    Wyvern = 16,
    Lion = 17,
    Head = 18,
    Firefly = 19,
    Spider = 20,
    Diablo = 21,
    FlyingMana = 22,
    Balloon = 23,
    Unknown24 = 24,
    FlyingCreature = 25,
    Mummy = 26,
    BigDragon = 27,
    ManaCreature = 28
}

public class CreatureType : EntityType
{
    public CreatureType(Creature creature) : base(TypeId.Creature, (int)creature, creature.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Creature>();
}
