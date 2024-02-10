using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Infrastructure.Interfaces;

public interface ISettingsPort
{
    public string CurrentLevelFilePath { get; set; }
    public bool SaveSettings(Settings settings);
    public Settings? LoadSettings();

}
