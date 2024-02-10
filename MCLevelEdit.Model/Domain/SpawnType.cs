using Avalonia.Media;

namespace MCLevelEdit.Model.Domain;

public enum Spawn
{
    Flyer1 = 4,
    Flyer2 = 5,
    Flyer3 = 6,
    Flyer4 = 7,
    Flyer5 = 8,
    Flyer6 = 9,
    Flyer7 = 10,
    Flyer8 = 11
}

public class SpawnType : EntityType
{
    public SpawnType(Spawn spawn) : base(TypeId.Spawn, Color.FromRgb(255,255,0), ((int)spawn), spawn.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Spawn))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Spawn), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
