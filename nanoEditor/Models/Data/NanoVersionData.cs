namespace nanoEditor.Models;

public class NanoVersionData
{
    public string Url { get; set; }
    public string Version { get; set; }
    public BranchType Branch { get; set; }
    public string Commit { get; set; }
    public string CommitUrl { get; set; }
    public string CreatedOn { get; set; }
    public  bool CheckForUpdates { get; set; }
    public enum BranchType
    {
        Release = 0,
        Beta = 1
    }
}