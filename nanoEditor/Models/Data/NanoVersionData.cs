using nanoEditor.Configs;
using UnityEditor;

namespace nanoEditor.Models;

public class NanoVersionData
{
    public string Url { get; set; }
    public string Version { get; set; } = ConfigManager.GetCurrentVersion();  //Assets/Version.txt
    public BranchType Branch { get; set; }  = BranchType.Release;
    public string Commit { get; set; }
    public string CommitUrl { get; set; }
    public string CreatedOn { get; set; }
    public  bool CheckForUpdates { get; set; } = true;
    public enum BranchType
    {
        Release = 0,
        Beta = 1
    }
}