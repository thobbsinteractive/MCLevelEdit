using Avalonia.Media;

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
    public CreatureType(Creature creature) : base(TypeId.Creature, Color.FromRgb(255, 0, 0),((int)creature), creature.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Creature))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Creature), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
