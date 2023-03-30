using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.UserInterfaces.Tools;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.Models;
[InitializeOnLoad]
public class NanoInit
{
    private static readonly ConfigManager _configManager = new();
    public NanoInit()
    {
        NanoLog.Log("NANOSDK","nanoSDK Initialized");
        if(_configManager.Config.BackupManager.AutoBackup)
            BackupManager.CreateBackup(_configManager.Config.BackupManager.SaveAsUnitypackage,
                _configManager.Config.BackupManager.DeleteOldBackups);
    }
}