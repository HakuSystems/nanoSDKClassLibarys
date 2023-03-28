using UnityEditor;

namespace nanoEditor.UserInterfaces.Tools;

public class SortFiles : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow(typeof(SortFiles), false, "SortFiles");
    }
}