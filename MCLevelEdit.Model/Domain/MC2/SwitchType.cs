namespace MCLevelEdit.Model.Domain.MC2;

public enum Switch
{
    HiddenInside = 0,
    HiddenOutside = 1,
    HiddenInsideRe = 2,
    HiddenOutsideRe = 3,
    OnVictory = 4,
    DeathInside = 5,
    DeathOutside = 6,
    DeathInsideRe = 7,
    DeathOutsideRe = 8,
    ObviousInside = 9,
    ObviousOutside = 10,
    ObviousInsideRe = 11,
    ExitLevel = 12,
    // 13 + creature ID (0-16), 33 + creature ID - 17 (17-28): triggers when every creature of that type is dead
    Dragon = 13,
    Goat = 14,
    Bee = 15,
    Worm = 16,
    Archer = 17,
    Crab = 18,
    Kraken = 19,
    TrollApe = 20,
    Griffin = 21,
    Skeletons = 22,
    Emu = 23,
    Genie = 24,
    Builder = 25,
    Townie = 26,
    Trader = 27,
    Unknown28 = 28,
    Wyvern = 29,
    CreatureAll = 30,
    ExitLevel2 = 31,
    StageComplete = 32,
    Lion = 33,
    Head = 34,
    Firefly = 35,
    Spider = 36,
    Diablo = 37,
    FlyingMana = 38,
    Balloon = 39,
    Unknown40 = 40,
    FlyingCreature = 41,
    Mummy = 42,
    BigDragon = 43,
    Unknown44 = 44
}

public class SwitchType : EntityType
{
    public SwitchType(Switch gameSwitch) : base(TypeId.Switch, (int)gameSwitch, gameSwitch.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Switch>();
}
