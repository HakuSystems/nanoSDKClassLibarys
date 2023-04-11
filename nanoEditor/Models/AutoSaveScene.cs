using nanoEditor.Configs;
using nanoEditor.Logger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace nanoEditor.Models;
[InitializeOnLoad]
public class AutoSaveScene
{
    private static readonly ConfigManager _configManager = new();
    private static void SceneSaver()
    {
        float lastSaveTime = 0;

        EditorApplication.hierarchyChanged += () =>
        { 
            if (!_configManager.Config.SceneSaver.SaveEnabled || IsPlayMode()) return; //only works in Edit Mode

            // only save if at least 5 seconds have passed since the last save
            if (Time.realtimeSinceStartup - lastSaveTime >= 5)
            {
                if(EditorSceneManager.sceneCount <= 0) return;
                EditorSceneManager.SaveOpenScenes();
                NanoLog.Log("SceneSaver", "Open Scenes saved");
                lastSaveTime = Time.realtimeSinceStartup;
            }
        };
    }

    public static bool IsPlayMode()
    {
        return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
    }
}