using UnityEditor;

namespace nanoEditor.UserInterfaces.Tools;

public class PresetManager : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow(typeof(PresetManager), false, "PresetManager");
    }
}