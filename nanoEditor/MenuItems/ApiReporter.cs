using System.Text;
using nanoEditor.Auth;
using nanoEditor.Configs;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.MenuItems;

public class ApiReporter : EditorWindow
{
    //Bugs
    private string _bugMessage;
    private string _scriptName;
    //Features
    private string _featureTitle;
    private string _featureDescription;

    [MenuItem("nanoSDK/Support", false, 0)]
    private static void OpenReportWindow()
    {
        GetWindow(typeof(ApiReporter), false, "nanoSDK Support").Show();
    }

    private void OnEnable()
    {
        _bugMessage = "";
        _scriptName = null;
        _featureTitle = "";
        _featureDescription = "";
        minSize = new Vector2(400, 300);
        
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Report a bug", EditorStyles.boldLabel);
        _bugMessage = EditorGUILayout.TextField("Describe the bug", _bugMessage, GUILayout.Height(100));
        if (GUILayout.Button("Report bug"))
        {
            ApiHelper.ReportBug(_scriptName, _bugMessage);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Request a feature", EditorStyles.boldLabel);
        _featureTitle = EditorGUILayout.TextField("Feature Title", _featureTitle);
        _featureDescription = EditorGUILayout.TextField("Feature description", _featureDescription, GUILayout.Height(100));
        if (GUILayout.Button("Request feature"))
        {
            FeatureRequest(_featureTitle, _featureDescription);
        }
        EditorGUILayout.EndVertical();
    }

    private static void FeatureRequest(string featureTitle, string description)
    {
        ApiHelper.RequestFeature(featureTitle, description);
    }
}