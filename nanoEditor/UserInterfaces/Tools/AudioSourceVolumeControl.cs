using UnityEditor;

namespace nanoEditor.UserInterfaces.Tools;

public class AudioSourceVolumeControl : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow(typeof(AudioSourceVolumeControl), false, "AudioSourceVolumeControl");
    }
}