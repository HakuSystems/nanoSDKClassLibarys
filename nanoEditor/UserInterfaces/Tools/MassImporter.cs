﻿using nanoEditor.Logger;
using nanoEditor.Styles;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces.Tools;

public class MassImporter : EditorWindow
{
    private readonly List<string> _paths = new();
    private Vector2 _scrollPosition;
    public static void ShowWindow()
    {
        GetWindow(typeof(MassImporter), false, "MassImporter");
    }

    private void OnEnable()
    {
        minSize = new Vector2(300, 300);
        _paths.Clear();
    }

    private void Update()
    {
        foreach (var path in _paths.Where(path=> !File.Exists(path)))
        {
            _paths.Remove(path);
            NanoLog.Log("MassImporter", $"Removed {path} from list because it doesn't exist anymore.");
            break;
        }
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.DragUpdated)
        {
            var visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }
        if(Event.current.type == EventType.DragPerform)
        {
            foreach (var path in DragAndDrop.paths)
            {
                _paths.Add(path);
                NanoLog.Log("MassImporter", $"Added {path} to list.");
            }
            Event.current.Use();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Waiting for files to be dragged in.", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Search on Computer"))
        {
            var filePanelPath = EditorUtility.OpenFilePanel("Select Unitypackage", "", "unitypackage");
            if(filePanelPath.EndsWith(".unitypackage"))
                _paths.Add(filePanelPath);
            NanoLog.Log("MassImporter", $"Added {filePanelPath} to list.");
        }
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        foreach (var path in _paths)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove", new GUIStyle(GUIStyleTypes.ButtonMid.ToString())))
            {
                _paths.Remove(path);
                NanoLog.Log("MassImporter", $"Removed {path} from list.");
                break;
            }
            EditorGUILayout.LabelField(path, new GUIStyle(GUIStyleTypes.HeaderLabel.ToString()));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.BeginHorizontal();
        if (_paths.Count != 0 && GUILayout.Button("Import All"))
        {
            foreach (var path in _paths)
            {
                AssetDatabase.ImportPackage(path, false);
            }
            _paths.Clear();
            NanoLog.Log("MassImporter", "Imported all packages.");
        }
        if(_paths.Count != 0 && GUILayout.Button("Remove All"))
            _paths.Clear();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
}