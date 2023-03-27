using System.Collections;
using nanoEditor.Auth;
using nanoEditor.Configs;
using nanoEditor.Styles;
using nanoEditor.Version;
using UnityEditor;
using UnityEngine;

namespace nanoEditor;

public class NanoDashboard : EditorWindow
{
    private static ConfigManager _configManager = new();
    private string _keyInput;
    private readonly Uri _nanoSDKDiscord = new("https://nanosdk.net/discord");
    private bool _settingsOpen;
    private bool _toolsOpen;
    private bool _plusFoldout;

    public static void ShowDashboard()
    {
        GetWindow(typeof(NanoDashboard), false, "nanoSDK Dashboard").Show();
    }
    private void OnGUI()
    {
        //login related stuff
        if (ApiHelper.user == null)
            Close();
        
        EditorGUILayout.BeginVertical();
        if (!ApiHelper.user.IsVerified)
        {
            DrawLicenseRedeem();
            return;
        }
        EditorGUILayout.EndVertical();

        //top
        EditorGUILayout.BeginHorizontal();
        GUILayout.Button(ApiHelper.user.Username,GUIStyleTypes.toolbarbutton.ToString());
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
        EditorGUILayout.LabelField("Welcome to the nanoSDK Dashboard!", EditorStyles.centeredGreyMiniLabel);
        _toolsOpen = FoldoutTexture.MakeTextFoldout("Tools", _toolsOpen);
        _settingsOpen = FoldoutTexture.MakeTextFoldout("Settings", _settingsOpen);
        _plusFoldout = FoldoutTexture.MakeTextFoldout("nanoSDK Plus", _plusFoldout);
        
        if (_toolsOpen)
            UserInterfaces.ToolsGUI.DrawToolsGUI();
        

        if (_settingsOpen)
            UserInterfaces.SettingGUI.DrawSettingsGUI();
        

        if (_plusFoldout && _configManager.Config.PremiumCheck.IsPremiumSinceLastCheck)
            UserInterfaces.PlusGUI.DrawPlusGUI();
        
        EditorGUILayout.EndVertical();
        
        //footer
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("nanoSDK", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField($"V{_configManager.Config.NanoVersion.Version}", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.EndHorizontal();

    }

    private void DrawLicenseRedeem()
    {
        EditorGUILayout.LabelField("You need to redeem a license key in order to use nanoSDK.", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.BeginHorizontal();
        _keyInput = EditorGUILayout.TextField("License Key", _keyInput);
        if (GUILayout.Button("?", GUILayout.Width(20)))
        {
            var redirect = EditorUtility.DisplayDialog("Login", "To receive your License, you have to join our discord server!",
                "Lead me there");
            
            if(!redirect) return;
            
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

        if (_configManager.Config.NanoVersion.CheckForUpdates)
            await Updater.CheckForUpdates();
    }
    
}