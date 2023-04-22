using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.MenuItems;
using nanoEditor.Models;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces.Tools;

public class PresetManager : EditorWindow
{
    private static readonly ConfigManager _configManager = new();
    private static string _presetName;
    private static string _presetDescription;
    private static readonly List<string> PresetPaths = new(); //this is the list of all the presets
    private static Vector2 _scrollPosition;

    public static void ShowWindow()
    {
        GetWindow(typeof(PresetManager), false, "PresetManager");
    }

    private void OnEnable()
    {
        minSize = new Vector2(700, 400);
        PresetPaths.Clear();
    }

    private void Update()
    {
        var distinct = PresetPaths.Distinct().ToList();
        if (distinct.Count == PresetPaths.Count) return;
        PresetPaths.Clear();
        PresetPaths.AddRange(distinct);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Report a Bug")) QuickReport.BugReport("PresetManager");

            EditorGUILayout.LabelField(
                "Create a Template Preset in order to make importing easier for other projects!");

            _presetName = EditorGUILayout.TextField("Preset Name", _presetName);
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Preset Description", GUILayout.Width(150));
                EditorGUILayout.LabelField("Description is Optional",
                    new GUIStyle(EditorStyles.textField) { normal = { textColor = Color.yellow } });
            }
            EditorGUILayout.EndHorizontal();
            _presetDescription = EditorGUILayout.TextArea(_presetDescription, GUILayout.Height(80));
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Unityackage"))
                {
                    var path = EditorUtility.OpenFilePanel("Select Unitypackage", "", "unitypackage");
                    if (path.Length != 0)
                        PresetPaths.Add(path);
                }

                var presetCountHigherNotNull = PresetPaths.Count > 0 && _presetName != null;
                switch (presetCountHigherNotNull)
                {
                    case true:
                    {
                        if (GUILayout.Button("Save Preset",
                                new GUIStyle(EditorStyles.miniButton) { normal = { textColor = Color.green } }))
                        {
                            if (string.IsNullOrEmpty(_presetName))
                            {
                                EditorUtility.DisplayDialog("Error", "Please enter a name for the preset first",
                                    "Ok");
                                return;
                            }

                            CreateNewPreset(_presetName, _presetDescription, PresetPaths.ToList());
                        }

                        if (GUILayout.Button("Clear List"))
                            PresetPaths.Clear();
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField(PresetPaths.Count + " Unitypackage(s) Added",
                                new GUIStyle(EditorStyles.textField) { normal = { textColor = Color.green } });
                            foreach (var path in PresetPaths)
                                EditorGUILayout.LabelField(
                                    path.Substring(path.LastIndexOf("/", StringComparison.Ordinal) + 1));
                        }
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    default:
                        EditorGUILayout.LabelField("No Unitypackages added",
                            new GUIStyle(EditorStyles.textField) { normal = { textColor = Color.yellow } });
                        break;
                }

                EditorGUILayout.BeginVertical();
                {
                    if (_configManager.Config.PresetManager.Presets != null)
                    {
                        EditorGUILayout.LabelField("Saved Presets",
                            new GUIStyle(EditorStyles.textField) { normal = { textColor = Color.green } });
                        ShowPresets();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Presets found",
                            new GUIStyle(EditorStyles.textField) { normal = { textColor = Color.yellow } });
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        var evt = Event.current;

        var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "You can also Drag and Drop Multiple Unitypackages here");

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (!dropArea.Contains(evt.mousePosition))
                return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var draggedObject in DragAndDrop.paths)
                    if (draggedObject.EndsWith(".unitypackage"))
                        PresetPaths.Add(draggedObject);
            }
        }

        EditorGUILayout.BeginHorizontal();
        {
            if (_configManager.Config.PresetManager.Presets != null)
            {
                EditorGUILayout.LabelField("Total Presets: " + _configManager.Config.PresetManager.Presets.Count);
                if (GUILayout.Button("Remove All Presets"))
                    _configManager.UpdateConfig(config =>
                    {
                        config.PresetManager.Presets.Clear();
                        config.PresetManager.Presets = null;
                    });
            }

            if (GUILayout.Button("Add Project to Preset"))
                AddProjectToPreset();
        }
        EditorGUILayout.EndHorizontal();

        _configManager.UpdateConfig(config =>
        {
            config.PresetManager.Presets = _configManager.Config.PresetManager.Presets;
        });
    }

    private static void AddProjectToPreset()
    {
        if (_presetName == null)
        {
            EditorUtility.DisplayDialog("Error", "Please enter a name for the preset first", "Ok");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to add this project to a preset?",
                "Yes", "No")) return;
        var defaultPath = $"{_configManager.Config.DefaultPath.DefaultSet}/ProjectPreset/";
        if (!Directory.Exists(defaultPath))
            Directory.CreateDirectory(defaultPath);
        EditorUtility.DisplayProgressBar("Adding Project to Preset", "Please Wait", 1f);
        foreach (var file in Directory.GetFiles(defaultPath))
            File.Delete(file);
        var assets = new List<string>();
        AssetDatabase.ExportPackage(assets.ToArray(), defaultPath + "ProjectPreset.unitypackage",
            ExportPackageOptions.Recurse);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        PresetPaths.Add(defaultPath + "ProjectPreset.unitypackage");
        EditorUtility.DisplayDialog("Success", "Project has been added to a preset", "Ok");
    }

    private static void ShowPresets()
    {
        if (_configManager.Config.PresetManager.Presets == null) return;
        foreach (var preset in _configManager.Config.PresetManager.Presets.Values)
        {
            EditorGUILayout.HelpBox(preset.Name, MessageType.None);
            EditorGUILayout.LabelField(preset.Description.Length > 20
                ? "Description: " + preset.Description.Substring(0, 20) + " ..."
                : "Description: " + preset.Description);
            EditorGUILayout.LabelField("CreatedAt: " + preset.CreatedAt);
            EditorGUILayout.LabelField("Total Unitypackage(s): " + preset.Paths.Count);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Import"))
                    ImportByName(preset.Name);
                if (GUILayout.Button("Add to MassImporter"))
                    foreach (var paths in preset.Paths.ToList())
                        MassImporter.AddToMassImporter(paths);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void ImportByName(string name)
    {
        foreach (var path in _configManager.Config.PresetManager.Presets.Where(preset =>
                     preset.Value.Name == name).SelectMany(preset => preset.Value.Paths))
        {
            if (Directory.Exists(path))
                continue;
            AssetDatabase.ImportPackage(path, false);
            MassImporter.AddToMassImporter(path);
        }

        NanoLog.Log("PresetManager", "Imported all packages");
    }

    private static void CreateNewPreset(string name, string description, List<string> path)
    {
        var preset = new PresetData
        {
            Name = name,
            Description = description ?? "NULL",
            Paths = path,
            CreatedAt = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
        };

        if (_configManager.Config.PresetManager.Presets == null)
            _configManager.UpdateConfig(config =>
            {
                config.PresetManager.Presets = new Dictionary<int, PresetData>();
            });

        _configManager.Config.PresetManager.Presets.Add(_configManager.Config.PresetManager.Presets.Count + 1, preset);


        NanoLog.Log("PresetManager", "Preset Created: " + name);
        PresetPaths.Clear();
        _presetName = string.Empty;
        _presetDescription = string.Empty;
    }
}