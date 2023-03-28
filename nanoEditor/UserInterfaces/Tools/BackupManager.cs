using UnityEditor;

namespace nanoEditor.UserInterfaces.Tools;

public class BackupManager : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow(typeof(BackupManager), false, "BackupManager");
    }
}