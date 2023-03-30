using System.Diagnostics;
using System.Linq; // CHANGED: Added Linq for ordering
using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.Styles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using System.IO; // CHANGED: Added for DirectoryInfo and File classes
using System.Collections.Generic; // CHANGED: Added for List class
using System; // CHANGED: Added for DateTime class

namespace nanoEditor.UserInterfaces.Tools
{
    public class BackupManager : EditorWindow
    {
        private static readonly ConfigManager _configManager = new();
        private Vector2 _scrollPosition;

        [FormerlySerializedAs("Assets")] [SerializeField]
        private static readonly List<string>
            assets = new(); //list of assets to backup its always empty until AssetDatabase.ExportPackage is called

        private static readonly string BackupPath = $"{_configManager.Config.DefaultPath.DefaultSet}/Backups/";

        public static void ShowWindow()
        {
            GetWindow(typeof(BackupManager), false, "BackupManager");
        }

        private void OnEnable()
        {
            minSize = new Vector2(500, 200);
            if (!Directory.Exists(BackupPath))
                Directory.CreateDirectory(BackupPath);
        }

        private void OnDestroy()
        {
            _configManager.UpdateConfig(config =>
            {
                config.BackupManager.SaveAsUnitypackage = _configManager.Config.BackupManager.SaveAsUnitypackage;
                config.BackupManager.DeleteOldBackups = _configManager.Config.BackupManager.DeleteOldBackups;
                config.BackupManager.AutoBackup = _configManager.Config.BackupManager.AutoBackup;
            });
            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            GUILayout.Button("Backup Your UnityProject", EditorStyles.toolbarButton);

            EditorGUILayout.LabelField("Backup", EditorStyles.boldLabel);
            _configManager.Config.BackupManager.SaveAsUnitypackage = EditorGUILayout.Toggle("Save as Unitypackage",
                _configManager.Config.BackupManager.SaveAsUnitypackage);
            _configManager.Config.BackupManager.DeleteOldBackups = EditorGUILayout.Toggle("Clean Disk Space",
                _configManager.Config.BackupManager.DeleteOldBackups);
            _configManager.Config.BackupManager.AutoBackup =
                EditorGUILayout.Toggle("Auto Backup", _configManager.Config.BackupManager.AutoBackup);

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Backup", new GUIStyle(GUIStyleTypes.toolbarbutton.ToString())))
                    CreateBackup(_configManager.Config.BackupManager.SaveAsUnitypackage,
                        _configManager.Config.BackupManager.DeleteOldBackups);
                if (GUILayout.Button("Delete All Backups", new GUIStyle(GUIStyleTypes.toolbarbutton.ToString())))
                    if (EditorUtility.DisplayDialog("BackupManager", "Are you sure you want to delete all backups?", "Yes",
                            "No"))
                    {
                        foreach (var file in Directory.GetFiles(BackupPath)) File.Delete(file);
                        NanoLog.Log("BackupManager", "All backups deleted");
                        AssetDatabase.Refresh();
                    }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Last Backup: " + GetLastBackupDate(), EditorStyles.boldLabel);
                if (GUILayout.Button("Open Backup Folder", new GUIStyle(GUIStyleTypes.toolbarbutton.ToString())))
                {
                    if (!Directory.Exists(BackupPath))
                        Directory.CreateDirectory(BackupPath);
                    Process.Start(BackupPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("BackupList", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                EditorGUILayout.BeginVertical();
                {
                    foreach (var file in Directory.GetFiles(BackupPath))
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Path.GetFileName(file));
                            if (_configManager.Config.BackupManager.DeleteOldBackups)
                            {
                                GUI.color = Color.red;
                                GUILayout.Label("Deletion Pending", new GUIStyle(GUIStyleTypes.toolbarbutton.ToString()));
                                GUI.color = Color.white;
                            }

                            if (GUILayout.Button("Delete", new GUIStyle(GUIStyleTypes.toolbarbutton.ToString())))
                                if (EditorUtility.DisplayDialog("BackupManager",
                                        "Are you sure you want to delete this backup?", "Yes", "No"))
                                {
                                    File.Delete(file);
                                    NanoLog.Log("BackupManager", "Backup deleted");
                                    AssetDatabase.Refresh();
                                }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        public static void CreateBackup(bool _saveAsUnitypackage, bool _deleteOldBackups)
        {
            NanoLog.Log("BackupManager", "Creating Backup");

            var results = 0f;
            EditorUtility.DisplayCancelableProgressBar("BackupManager", "Creating Backup", results); // CHANGED: ProgressBar with cancel option
            var backupName = Application.productName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            Directory.CreateDirectory(BackupPath);
            var files = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);
            if (!_saveAsUnitypackage)
            {
                foreach (var file in files)
                {
                    if (file.Contains(".meta")) continue;
                    var relativePath = file.Replace(Application.dataPath, "");
                    var destination = BackupPath + backupName + "/" + relativePath;
                    var destinationDir = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(destinationDir))
                        Directory.CreateDirectory(destinationDir);
                    File.Copy(file, destination, true);
                    results += 1f / files.Length;
                    if (EditorUtility.DisplayCancelableProgressBar("BackupManager", "Creating Backup", results)) // CHANGED: ProgressBar with cancel option
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }
            }
            else
            {
                EditorUtility.DisplayCancelableProgressBar("BackupManager", "Creating Unitypackage", results); // CHANGED: ProgressBar with cancel option
                AssetDatabase.ExportPackage(assets.ToArray(), BackupPath + backupName + ".unitypackage",
                    ExportPackageOptions.Recurse);
            }

            if (_deleteOldBackups)
            {
                var backupFolders = Directory.GetDirectories(BackupPath);
                var backupFiles = Directory.GetFiles(BackupPath);
                var backupFoldersSorted = backupFolders.OrderBy(Directory.GetCreationTime).ToArray();
                var backupFilesSorted = backupFiles.OrderBy(File.GetCreationTime).ToArray();
                if (backupFoldersSorted.Length > 1)
                    for (var i = 0; i < backupFoldersSorted.Length - 1; i++)
                        Directory.Delete(backupFoldersSorted[i], true);
                if (backupFilesSorted.Length > 1)
                    for (var i = 0; i < backupFilesSorted.Length - 1; i++)
                        if (backupFilesSorted[i].Contains(".unitypackage"))
                            File.Delete(backupFilesSorted[i]);
            }

            NanoLog.Log("BackupManager", "Backup created");
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private static string GetLastBackupDate()
        {
            //get last backup folder by date or unitypackage by date
            var backupFolders = Directory.GetDirectories(BackupPath);
            var backupFiles = Directory.GetFiles(BackupPath);
            var backupFoldersSorted = backupFolders.OrderBy(Directory.GetCreationTime).ToArray();
            var backupFilesSorted = backupFiles.OrderBy(File.GetCreationTime).ToArray();
            if (backupFoldersSorted.Length > 0)
                return Directory.GetCreationTime(backupFoldersSorted.Last()).ToString("yyyy-MM-dd-HH-mm-ss");
            if (backupFilesSorted.Length > 0)
                return File.GetCreationTime(backupFilesSorted.Last()).ToString("yyyy-MM-dd-HH-mm-ss");
            return "No Backups";
        }
    }
}
