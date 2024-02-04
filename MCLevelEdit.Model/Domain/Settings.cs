namespace MCLevelEdit.Model.Domain
{
    public class Settings
    {
        public string GameExeLocation { get; set; } = string.Empty;
        public string[] GameLevelFolders { get; set; } = new string[0];
    }
}
