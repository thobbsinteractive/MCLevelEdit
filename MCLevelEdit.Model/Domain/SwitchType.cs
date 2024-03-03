namespace MCLevelEdit.Model.Domain;

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
    Dragon = 13,
    Vulture = 14,
    Bee = 15,
    None = 16,
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
    Unknown6 = 28,
    Wyvern = 29,
    CreatureAll = 30,
    Unknown = 31
}

public class SwitchType : EntityType
{
    public SwitchType(Switch gameSwitch) : base(TypeId.Switch, ((int)gameSwitch), gameSwitch.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Switch))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Switch), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
