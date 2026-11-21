namespace MCLevelEdit.Model.Domain.MC2;

public enum Special
{
    Unknown1 = 1,
    Unknown2 = 2,
    Gate = 3,
    Unknown4 = 4,
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
