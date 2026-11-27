namespace MCLevelEdit.Model.Domain.MC2;

// Sources for Magic Carpet 2 names: remc2 (docs/structures_info.txt and engine code) and the
// MagicBalls MC2 editor (github.com/turican0/MagicBalls, godot-code/scenes-editor/editor.gd).
// Kinds with no known name that appear in the original levels are listed as UnknownNN.

public enum Scenery
{
    Tree = 0,
    StandingStone = 1,
    Statue = 2,
    BadStone = 3,
    SmallCarpet1 = 4,
    SmallCarpet2 = 5,
    Unknown6 = 6,
    Barrel = 7,
    Vase = 8
}

public class SceneryType : EntityType
{
    public SceneryType(Scenery scenery) : base(TypeId.Scenery, (int)scenery, scenery.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Scenery>();
}
