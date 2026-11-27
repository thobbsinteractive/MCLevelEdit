namespace MCLevelEdit.Model.Domain.MC1;

/// <summary>
/// Hidden Worlds (the Magic Carpet data disk) entity types. Its levels use the Magic Carpet format and
/// entities; the only difference is one spell, see <see cref="HiddenWorldsSpellType"/>.
/// </summary>
public class HiddenWorldsEntityTypes : EntityTypes
{
    private static HiddenWorldsEntityTypes? instance;

    public static new HiddenWorldsEntityTypes I => instance ??= new HiddenWorldsEntityTypes();

    public HiddenWorldsEntityTypes() : base(x => new HiddenWorldsSpellType(x)) { }
}
