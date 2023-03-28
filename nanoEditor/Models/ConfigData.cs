using JetBrains.Annotations;

namespace nanoEditor.Models;

public class ConfigData
{
    public PresetManagerData PresetManager { get; set; } = new();
    public DefaultPathData DefaultPath { get; set; } = new();
    public NanoVersionData NanoVersion { get; set; } = new();
    public TermsPolicyData TermsPolicy { get; set; } = new();
    public NanoAuthData NanoAuth { get; set; } = new();
    public PremiumCheckData PremiumCheck { get; set; } = new();
    public DiscordPresenceData DiscordPresence { get; set; } = new();
    public SceneSaverData SceneSaver { get; set; } = new();
    public BackupManagerData BackupManager { get; set; } = new();

}