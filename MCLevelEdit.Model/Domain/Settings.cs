namespace MCLevelEdit.Model.Domain
{
    /// <summary>
    /// The game "Run Level" launches. Separate from <see cref="GameVersion"/>, which is the level
    /// format: Hidden Worlds levels use the Magic Carpet 1 format but run from their own install.
    /// </summary>
    public enum GameTarget
    {
        MagicCarpet1 = 1,
        MagicCarpet2 = 2,
        HiddenWorlds = 3
    }

    /// <summary>
    /// The packed levels file each DOSBox game reads. Hidden Worlds (the data disk) keeps its levels in
    /// DDLEVELS.DAT/.TAB, next to Magic Carpet's LEVELS.DAT/.TAB in the same LEVELS folder.
    /// </summary>
    public static class LevelsFile
    {
        public const string MAGIC_CARPET = "LEVELS.DAT";
        public const string HIDDEN_WORLDS = "DDLEVELS.DAT";

        public static string For(GameTarget game) => game == GameTarget.HiddenWorlds ? HIDDEN_WORLDS : MAGIC_CARPET;
    }

    /// <summary>Settings for a game run through DOSBox (GOG or original install).</summary>
    public class DosGameSettings
    {
        public bool IsForClassic { get; set; } = false;
        public string GameExeLocation { get; set; } = string.Empty;
        public string GameArgs { get; set; } = string.Empty;
        public string[] GameLevelFolders { get; set; } = new string[0];
        public string GameBackupFolder { get; set; } = string.Empty;
    }

    public class Settings
    {
        /// <summary>The game that "Run Level" launches.</summary>
        public GameTarget SelectedGame { get; set; } = GameTarget.MagicCarpet1;

        // Magic Carpet 1 (kept as top-level fields so existing settings files still load)
        public bool IsForClassic { get; set; } = false;
        public string GameExeLocation { get; set; } = string.Empty;
        public string GameArgs { get; set; } = string.Empty;
        public string[] GameLevelFolders { get; set; } = new string[0];
        public string GameBackupFolder { get; set; } = string.Empty;

        // Magic Carpet: Hidden Worlds
        public DosGameSettings HiddenWorlds { get; set; } = new DosGameSettings();

        // Magic Carpet 2 (Remc2)
        public string Mc2ExeLocation { get; set; } = string.Empty;

        /// <summary>A view over the top-level Magic Carpet 1 fields; not stored separately.</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public DosGameSettings MagicCarpet1
        {
            get => new DosGameSettings
            {
                IsForClassic = IsForClassic,
                GameExeLocation = GameExeLocation,
                GameArgs = GameArgs,
                GameLevelFolders = GameLevelFolders,
                GameBackupFolder = GameBackupFolder
            };
            set
            {
                IsForClassic = value.IsForClassic;
                GameExeLocation = value.GameExeLocation;
                GameArgs = value.GameArgs;
                GameLevelFolders = value.GameLevelFolders;
                GameBackupFolder = value.GameBackupFolder;
            }
        }

        /// <summary>The DOSBox settings for the selected game, or null for Magic Carpet 2.</summary>
        public DosGameSettings? GetSelectedDosSettings() => SelectedGame switch
        {
            GameTarget.HiddenWorlds => HiddenWorlds,
            GameTarget.MagicCarpet2 => null,
            _ => MagicCarpet1
        };

        public string GetSelectedExeLocation() =>
            GetSelectedDosSettings()?.GameExeLocation ?? Mc2ExeLocation;
    }
}
