namespace MCLevelEdit.Model.Domain
{
    public class Settings
    {
        public bool IsForClassic { get; set; } = false;
        public string GameExeLocation { get; set; } = string.Empty;
        public string[] GameLevelFolders { get; set; } = new string[0];
    }
}
