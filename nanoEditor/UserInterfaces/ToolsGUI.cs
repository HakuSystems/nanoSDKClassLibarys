using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces;

public class ToolsGUI : EditorWindow
{
    public static bool DrawToolsGUI()
    {
        EditorGUILayout.BeginHorizontal();
        var tools = new Dictionary<string, string>
        {
            { "AudioSourceVolumeControl", "https:///naxokit.com/discord" },
            { "MassImporter", "https:///naxokit.com/discord" },
            { "PresetManager", "https:///naxokit.com/discord" },
            { "EasySearch", "https:///naxokit.com/discord" },
            { "BackupManager", "https:///naxokit.com/discord" },
            { "SortFiles", "https:///naxokit.com/discord" }
        };
        
        foreach (var tool in tools)
        {
            if (GUILayout.Button(tool.Key, GUILayout.Width(120)))
            {
                switch (tool.Key)
                {
                    case "PresetManager":
                        GetWindow(typeof(Tools.PresetManager), false, "PresetManager");
                        break;
                    case "EasySearch":
                        GetWindow(typeof(Tools.EasySearch), false, "EasySearch");
                        break;
                    case "SortFiles":
                        GetWindow(typeof(Tools.SortFiles), false, "SortFiles");
                        break;
                    case "AudioSourceVolumeControl":
                        GetWindow(typeof(Tools.AudioSourceVolumeControl), false, "AudioSourceVolumeControl");
                        break;
                    case "MassImporter":
                        GetWindow(typeof(Tools.MassImporter), false, "MassImporter");
                        break;
                    case "BackupManager":
                        GetWindow(typeof(Tools.BackupManager), false, "BackupManager");
                        break;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        return false;
    }
}