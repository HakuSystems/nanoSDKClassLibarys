namespace nanoEditor.Models;

public class ImportData
{
    public static string ServerUrl { get; set; } = "https://nanoSDK.net/assets/";
    public Dictionary<string, AssetsData> Assets { get; set; }
}

public class AssetsData
{
    public string Name { get; set; }
    public string File { get; set; }
}