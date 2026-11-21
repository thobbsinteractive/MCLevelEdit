namespace MCLevelEdit.Model.Domain.Shared;

/// <summary>Wizard spawn points. Identical in Magic Carpet 1 and 2.</summary>
public enum Spawn
{
    Flyer1 = 4, // the player
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
    public SpawnType(Spawn spawn, GameVersion game = GameVersion.MagicCarpet1)
        : base(TypeId.Spawn, (int)spawn, spawn.ToString())
    {
        Game = game;
    }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues<Spawn>()
                    .Select(x => new ModelType() { Id = (int)x, Name = x.ToString() })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
