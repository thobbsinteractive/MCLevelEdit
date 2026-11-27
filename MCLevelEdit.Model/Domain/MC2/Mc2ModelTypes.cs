namespace MCLevelEdit.Model.Domain.MC2;

internal static class Mc2ModelTypes
{
    /// <summary>The kinds of an MC2 entity type, from its enum.</summary>
    public static ModelType[] From<TEnum>() where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(x => new ModelType { Id = Convert.ToInt32(x), Name = x.ToString() })
            .ToArray();
}
