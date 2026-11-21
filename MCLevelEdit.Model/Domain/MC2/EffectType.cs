namespace MCLevelEdit.Model.Domain.MC2;

// IDs shared with Magic Carpet 1 (Wall, Path, Canyon, Teleport, VillagerBuilding, RidgeNode, ...)
// keep the same values, so the editor's wall/path tools and validation work for both games.
public enum Effect
{
    Explosion = 0,
    BigExplosion = 1,
    Blood = 3,
    Wizard = 4,
    Splash = 5,
    Fire = 6,
    Freeze = 7,
    MiniVolcano = 8,
    Volcano = 9,
    MiniCrater = 10,
    Crater = 11,
    Possession = 12,
    WhiteSmoke = 13,
    BlackSmoke = 14,
    Earthquake = 15,
    VolcanoFireball = 16,
    Meteor = 17,
    VolcanoEruption = 18,
    VolcanoSmoke = 19,
    StealMana = 21,
    Wind = 22,
    Lightning = 23,
    RainOfFire = 24,
    StealMana2 = 25,
    DuelRubberBand = 26,
    WallSection = 27,
    Wall = 28,
    Path = 29,
    Canyon = 31,
    CanyonNode = 32,
    Unknown33 = 33,
    Teleport = 34,
    Skeletons = 36,
    SpinningCircle = 38,
    ManaBall = 39,
    Corpse = 40,
    ThrowRock = 43,
    VillagerBuilding = 45,
    RollingRock = 49,
    RidgeNode = 50,
    Unknown51 = 51,
    CrabEgg = 52,
    Auxiliary = 54,
    Unknown57 = 57,
    ManaBall2560 = 58,
    Smoke1 = 59,
    Smoke2 = 60,
    Unknown63 = 63,
    Unknown64 = 64,
    Unknown67 = 67,
    Unknown71 = 71,
    FireSpheres = 76,
    Unknown80 = 80,
    Event = 82,
    Unknown83 = 83,
    Unknown84 = 84,
    Unknown85 = 85,
    Unknown86 = 86
}

public class EffectType : EntityType
{
    public EffectType(Effect effect) : base(TypeId.Effect, (int)effect, effect.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Effect>();
}
