using nanoEditor.Auth;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.MenuItems;

public class QuickReport : EditorWindow
{
    private static string ScriptName;
    private string bugDescription;
    public static void BugReport(string scriptName)
    {
        ScriptName = scriptName;
        var window = GetWindow<QuickReport>();
        window.titleContent = new GUIContent("Bug Report");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Quick Bug Reporter", EditorStyles.boldLabel);
        bugDescription = EditorGUILayout.TextField("Describe the bug", bugDescription, GUILayout.Height(100));
        if (GUILayout.Button("Send Report"))
        {
            ApiHelper.ReportBug(ScriptName, bugDescription);
        }
        EditorGUILayout.EndVertical();
        
    }

    private void OnLostFocus()
    {
        Focus();
    }

    private void OnEnable()
    {
        bugDescription = "";
        Focus();
    }
}