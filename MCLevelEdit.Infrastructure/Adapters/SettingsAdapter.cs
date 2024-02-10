using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Domain;
using System.Text.Json;

namespace MCLevelEdit.Infrastructure.Adapters
{
    public class SettingsAdapter : ISettingsPort
    {
        private const string SETTINGS_DIRECTORY = "MCLevelEdit/Settings";
        private const string SETTINGS_FILE = "Settings.json";

        private string AppSettingsPath { get; set; }
        public string CurrentLevelFilePath { get; set; }

        public SettingsAdapter() 
        {
            AppSettingsPath = string.Empty;
            CurrentLevelFilePath = string.Empty;

            try
            {
                var settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var appSettingDirectories = Path.Combine(settingsDirectory, SETTINGS_DIRECTORY);

                if (!Directory.Exists(appSettingDirectories))
                {
                    Directory.CreateDirectory(appSettingDirectories);
                }

                AppSettingsPath = Path.Combine(appSettingDirectories, SETTINGS_FILE);

                if (File.Exists(AppSettingsPath))
                {
                    LoadSettings();
                }
                else
                {
                    var settings = new Settings()
                    {
                        GameExeLocation = string.Empty,
                        GameLevelFolders = new string[0]
                    };

                    SaveSettings(settings);
                    LoadSettings();
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine("Error setting up Settings {0}", ex);
            }
        }

        public bool SaveSettings(Settings settings)
        {
            string jsonString = JsonSerializer.Serialize(settings);

            try
            {
                File.WriteAllText(AppSettingsPath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving: {0}", ex.Message);
            }
            return false;
        }

        public Settings? LoadSettings()
        {
            try
            {
                string jsonString = File.ReadAllText(AppSettingsPath);

                return JsonSerializer.Deserialize<Settings>(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving: {0}", ex.Message);
            }
            return null;
        }
    }
}
