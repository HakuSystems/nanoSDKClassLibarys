using nanoEditor.Models;
using Newtonsoft.Json;

namespace nanoEditor.Configs;

public class ConfigManager
{
    private static readonly string ConfigFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nanoSDK");
    private static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, "config.json");
    
    public ConfigData Config { get; private set; }

    public ConfigManager()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            string json = File.ReadAllText(ConfigFilePath);
            Config = JsonConvert.DeserializeObject<ConfigData>(json);
        }
        else
        {
            Config = new ConfigData();
            SaveConfig();
        }
    }
    public static string GetCurrentVersion()
    {
        //Packages\com.lyze.nanosdk\Version.txt
        var versionPath = Path.Combine("Packages/com.lyze.nanosdk/Version.txt");
        if (!File.Exists(versionPath)) return null;
        var version = File.ReadAllText(versionPath);
        return version;
    }

    private void SaveConfig()
    {
        Directory.CreateDirectory(ConfigFolderPath);
        string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
        File.WriteAllText(ConfigFilePath, json);
    }
    
    public void UpdateConfig(Action<ConfigData> updateAction)
    {
        updateAction?.Invoke(Config);
        SaveConfig();
    }
}