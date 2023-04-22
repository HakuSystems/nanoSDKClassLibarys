using nanoEditor.Configs;
using nanoEditor.MenuItems;
using nanoEditor.Models;
using nanoEditor.Version;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces;

public class SettingGUI : EditorWindow
{
    private static readonly ConfigManager _configManager = new();
    private static bool _checkForUpdates;

    public static bool DrawSettingsGUI()
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Report a Bug"))
        {
            QuickReport.BugReport("Settings");
        }
        EditorGUILayout.EndVertical();  
        DiscordSettings(); 
        GUILayout.Space(20);
        AutoSaverSettings();
        GUILayout.Space(20);
        DefaultPathSettings();
        GUILayout.Space(20);
        UpdatesSettings();

        return false;
    }

    private static void UpdatesSettings()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Update Settings", EditorStyles.toolbarTextField);
        if (GUILayout.Button("Check for Updates"))
            Updater.CheckForUpdates();
        _configManager.UpdateConfig(config =>
        {
            config.NanoVersion.CheckForUpdates = EditorGUILayout.ToggleLeft("Check for updates on Start",_configManager.Config.NanoVersion.CheckForUpdates)
                ? true
                : false;
        });
        EditorGUILayout.EndHorizontal();
        var buttonName = _configManager.Config.NanoVersion.Branch == NanoVersionData.BranchType.Release
            ? "Use Beta"
            : "Use Release";

        _configManager.UpdateConfig(config =>
        {
            config.NanoVersion.Branch = GUILayout.Button(buttonName)
                ? (buttonName == "Use Release" ? NanoVersionData.BranchType.Release : NanoVersionData.BranchType.Beta)
                : config.NanoVersion.Branch;
        });

        EditorGUILayout.LabelField($"Current: {_configManager.Config.NanoVersion.Branch}", EditorStyles.toolbarButton);

    }

    private static void AutoSaverSettings()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto Saver", EditorStyles.toolbarTextField);
        _configManager.Config.SceneSaver.SaveEnabled = EditorGUILayout.ToggleLeft("Auto Save Scenes", _configManager.Config.SceneSaver.SaveEnabled);
        EditorGUILayout.LabelField("Only saves in Edit Mode!", new GUIStyle(EditorStyles.textField) {normal = {textColor = Color.yellow}});
        EditorGUILayout.EndHorizontal();
    }

    private static void DiscordSettings()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Discord", EditorStyles.toolbarTextField);
        _configManager.Config.DiscordPresence.PresenceEnabled = EditorGUILayout.ToggleLeft("Enable Discord RPC", _configManager.Config.DiscordPresence.PresenceEnabled);
        EditorGUILayout.EndHorizontal();
    }

    private static void DefaultPathSettings()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default path", EditorStyles.toolbarTextField);
        if (GUILayout.Button("Change"))
        {
            var path = EditorUtility.OpenFolderPanel("Select Default Path", _configManager.Config.DefaultPath.DefaultSet,
                "");
            if (!string.IsNullOrEmpty(path))
            {
                _configManager.UpdateConfig(config => { config.DefaultPath.DefaultSet = path; });
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Current: {_configManager.Config.DefaultPath.DefaultSet}", EditorStyles.toolbarButton);
    }
}