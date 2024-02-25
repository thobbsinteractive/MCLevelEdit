using Avalonia.Media;

namespace MCLevelEdit.Model.Domain;

public enum Scenery
{
    Tree = 0,
    StandingStone = 1,
    Dolmen = 2,
    BadStone = 3,
    Dome1 = 4,
    Dome2 = 5
}

public class SceneryType : EntityType
{
    public SceneryType(Scenery scenery) : base(TypeId.Scenery, Color.FromRgb(0,255,0), ((int)scenery), scenery.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Scenery))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Scenery), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
