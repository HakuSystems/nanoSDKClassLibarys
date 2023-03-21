using JetBrains.Annotations;

namespace nanoEditor.Models;

public class PresetManagerData
{
    [CanBeNull] public Dictionary<int, PresetData> Presets { get; set; }
}
public class PresetData
{
    public string Name { get; set; }
    public List<string> Paths { get; set; }
    public string Description { get; set; }
        
    public DateTime CreatedAt { get; set; }
        
}