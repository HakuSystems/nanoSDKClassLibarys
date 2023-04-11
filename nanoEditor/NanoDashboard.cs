using nanoEditor.Auth;
using nanoEditor.Configs;
using nanoEditor.Discord;
using nanoEditor.Logger;
using nanoEditor.Models;
using nanoEditor.Styles;
using nanoEditor.UserInterfaces;
using nanoEditor.Version;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorApplication;

namespace nanoEditor;

public class NanoDashboard : EditorWindow
{
    private static readonly ConfigManager _configManager = new();
    private string _keyInput;
    private readonly Uri _nanoSDKDiscord = new("https://nanosdk.net/discord");
    private bool _settingsOpen;
    private bool _toolsOpen;
    private bool _plusFoldout;
    private bool _importFoldout;
    private bool _saveInProject;
    private Vector2 _scrollPosition;

    public static void ShowDashboard()
    {
        GetWindow(typeof(NanoDashboard), false, "nanoSDK Dashboard").Show();
    }

    private void OnDisable()
    {
        //DiscordRpc.Shutdown(); todo
    }

    private void OnGUI()
    {
        if(ApiHelper.user == null) 
            ApiHelper.CheckUserSelf();
        
        if (AutoSaveScene.IsPlayMode())
        {
            EditorGUILayout.LabelField("You can't use the Dashboard in Play Mode", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        EditorGUILayout.BeginVertical();
        if (!ApiHelper.user.IsVerified)
        {
            DrawLicenseRedeem();
            return;
        }

        if (_configManager.Config.DefaultPath.DefaultSet == null)
        {
            DrawDefaultPath();
            return;
        }

        EditorGUILayout.EndVertical();

        //top
        EditorGUILayout.BeginHorizontal();
        GUILayout.Button(ApiHelper.user.Username, GUIStyleTypes.toolbarbutton.ToString());
        // ReSharper disable once TooManyChainedReferences
        var isPlus = _configManager.Config.PremiumCheck.IsPremiumSinceLastCheck ? "Yes" : "No";
        EditorGUILayout.LabelField($"Plus: {isPlus}", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Logout", GUIStyleTypes.toolbarbutton.ToString()))
        {
            GetWindow(typeof(LoginProcess), false, "nanoSDK Account").Show();
            ApiHelper.Logout();
            Close();
        }

        EditorGUILayout.EndHorizontal();

        //Tools
        EditorGUILayout.BeginVertical();
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("Welcome to the nanoSDK Dashboard!", EditorStyles.centeredGreyMiniLabel);

        _toolsOpen = FoldoutTexture.MakeTextFoldout("Tools", _toolsOpen);
        if (_toolsOpen)
            ToolsGUI.DrawToolsGUI();

        _settingsOpen = FoldoutTexture.MakeTextFoldout("Settings", _settingsOpen);
        if (_settingsOpen)
            SettingGUI.DrawSettingsGUI();
        _importFoldout = FoldoutTexture.MakeTextFoldout("Importables", _importFoldout);
        if (_importFoldout)
            ImportGUI.DrawImportGUI();
        _plusFoldout = FoldoutTexture.MakeTextFoldout("nanoSDK Plus", _plusFoldout);
        if (_plusFoldout && _configManager.Config.PremiumCheck.IsPremiumSinceLastCheck) PlusGUI.DrawPlusGUI();


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        //footer
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("nanoSDK", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField($"V{_configManager.Config.NanoVersion.Version}", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDefaultPath()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Default Path", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Please Choose a Default Path for nanoSDK related stuff.\n" +
                                   "THIS IS NEEDED IN ORDER TO USE NANOSDK!",
            new GUIStyle(EditorStyles.foldout) { normal = { textColor = Color.green } });
        EditorGUILayout.Space();
        _saveInProject = EditorGUILayout.Toggle("Save In Project", _saveInProject);
        if (!_saveInProject)
        {
            GUI.backgroundColor = Color.green;
            if (!GUILayout.Button("Select Default Path..")) return;
            var path = EditorUtility.OpenFolderPanel("Select Default Path", "", "");
            if (string.IsNullOrEmpty(path)) return;
            _configManager.UpdateConfig(config => { config.DefaultPath.DefaultSet = path; });
        }

        GUI.backgroundColor = Color.red;
        EditorGUILayout.LabelField("NOT RECOMMENDED",
            new GUIStyle(EditorStyles.toolbarButton) { normal = { textColor = Color.yellow } });
        if (!GUILayout.Button("Save in Project")) return;
        EditorGUILayout.LabelField("NOT RECOMMENDED",
            new GUIStyle(EditorStyles.toolbar) { normal = { textColor = Color.yellow } });
        var projectPath = $"{Application.dataPath}/nanoSDK/DEFAULTPATH";
        if (!Directory.Exists(projectPath))
            Directory.CreateDirectory(projectPath);
        _configManager.UpdateConfig(config => { config.DefaultPath.DefaultSet = projectPath; });
        EditorGUILayout.EndVertical();
    }

    private void DrawLicenseRedeem()
    {
        EditorGUILayout.LabelField("You need to redeem a license key in order to use nanoSDK.",
            EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.BeginHorizontal();
        _keyInput = EditorGUILayout.TextField("License Key", _keyInput);
        if (GUILayout.Button("?", GUILayout.Width(20)))
        {
            var redirect = EditorUtility.DisplayDialog("Login",
                "To receive your License, you have to join our discord server!",
                "Lead me there");

            if (!redirect) return;

            Application.OpenURL(_nanoSDKDiscord.ToString());
        }

        if (GUILayout.Button("Redeem"))
        {
            if (string.IsNullOrEmpty(_keyInput))
                EditorUtility.DisplayDialog("nanoSDKDashboard", "License Key cant be Empty", "Okay");
            else
                ApiHelper.RedeemLicense(_keyInput);
        }

        EditorGUILayout.EndHorizontal();
    }

    private async void OnEnable()
    {
        titleContent = new GUIContent("nanoSDK Dashboard");
        minSize = new Vector2(1200, 300);

        if (_configManager.Config.NanoVersion.CheckForUpdates)
            await Updater.CheckForUpdates();
        _configManager.UpdateConfig(config => { config.NanoVersion.Version = ConfigManager.GetCurrentVersion(); });
        //if(!AutoSaveScene.IsPlayMode())
          //  nanoSDKDiscordRpc.InitializeDiscordRpc(); Todo: Fix Discord RPC

    }
}