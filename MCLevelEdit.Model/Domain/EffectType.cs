using System.Drawing;

namespace MCLevelEdit.Model.Domain;

public enum Effect
{
    Explosion = 0,  
    BigExplosion = 1, 
    Dust = 2,
    Blood = 3,
    Spark = 4,
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
    Unknown20 = 20,
    StealMana = 21,
    Unknown22 = 22,
    Lightning = 23,
    RainOfFire = 24,
    StealMana2 = 25,
    DuelRubberBand = 26,
    WallSection = 27,
    Wall = 28,
    Path = 29,
    PathNode = 30,
    Canyon = 31,
    CanyonNode = 32,
    Unknown33 = 33,
    Teleport = 34,
    CircleBall = 35,
    Skeletons = 36,
    Alliance = 37,
    SpinningCircle = 38,
    ManaBall = 39,
    Corpse = 40,
    Unknown41 = 41,
    Unknown42 = 42,
    ThrowRock = 43,
    Unknown44 = 44,
    VillagerBuilding = 45,
    Castle = 46,
    CastleAdjust = 47,
    CastleRepair = 48,
    RollingRock = 49,
    RidgeNode = 50,
    Unknown51 = 51,
    CrabEgg = 52
}

public class EffectType : EntityType
{
    public EffectType(Effect effect) : base(TypeId.Effect, ((int)effect), effect.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Effect))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Effect), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
