namespace MCLevelEdit.Model.Domain.MC2;

public enum Special
{
    WallGate = 1,
    CaveDoor = 2,
    ExitGate = 3,
    ExitGate2 = 4,
    Scroll = 5
}

public class SpecialType : EntityType
{
    public SpecialType(Special special) : base(TypeId.Special, (int)special, special.ToString())
    {
        Game = GameVersion.MagicCarpet2;
    }

    public override ModelType[] ModelTypes => _modelTypes ??= Mc2ModelTypes.From<Special>();
}
