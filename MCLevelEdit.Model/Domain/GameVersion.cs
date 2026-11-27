namespace MCLevelEdit.Model.Domain;

public enum GameVersion
{
    MagicCarpet1 = 1,
    MagicCarpet2 = 2
}

public static class LevelFormat
{
    /// <summary>Uncompressed size of a Magic Carpet 1 / Hidden Worlds level file.</summary>
    public const int MC1_FILE_SIZE = 38812;

    /// <summary>Uncompressed size of a Magic Carpet 2 level file (Type_Level_2FECE in remc2, 0x6604 bytes).</summary>
    public const int MC2_FILE_SIZE = 26116;

    /// <summary>Number of entity slots after the unused slot 0.</summary>
    public const int MC1_MAX_ENTITIES = 1999;
    public const int MC2_MAX_ENTITIES = 1199;

    public const int MC2_OBJECTIVE_COUNT = 8;
    public const int MC2_STAGE_VAR_COUNT = 11;
    public const int MC2_SPELL_COUNT = 26;

    public static int GetFileSize(GameVersion version) =>
        version == GameVersion.MagicCarpet2 ? MC2_FILE_SIZE : MC1_FILE_SIZE;

    public static int GetMaxEntities(GameVersion version) =>
        version == GameVersion.MagicCarpet2 ? MC2_MAX_ENTITIES : MC1_MAX_ENTITIES;

    public static GameVersion? DetectFromFileSize(long size) => size switch
    {
        MC1_FILE_SIZE => GameVersion.MagicCarpet1,
        MC2_FILE_SIZE => GameVersion.MagicCarpet2,
        _ => null
    };
}
