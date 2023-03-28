using UnityEditor;

namespace nanoEditor.UserInterfaces.Tools;

public class EasySearch : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow(typeof(EasySearch), false, "EasySearch");
    }
}