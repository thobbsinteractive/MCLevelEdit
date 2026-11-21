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
    ObviousOutsideRe = 12,
    // 13 + creature ID: triggers when that creature type is killed
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
    ExitLevel = 31,
    Unknown32 = 32,
    Unknown33 = 33,
    Unknown34 = 34,
    Unknown35 = 35,
    Unknown36 = 36,
    Unknown37 = 37,
    Unknown38 = 38,
    Unknown39 = 39,
    Unknown41 = 41,
    Unknown42 = 42,
    Unknown43 = 43,
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
