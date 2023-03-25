using nanoEditor.Auth;
using UnityEditor;
using UnityEngine;

namespace nanoEditor;

public class NanoDashboard : EditorWindow
{
    private string _keyInput;
    private readonly Uri _nanoSDKDiscord = new("https://nanosdk.net/discord");
    
    public static void ShowDashboard()
    {
        GetWindow(typeof(NanoDashboard), false, "nanoSDK Dashboard").Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        if (ApiHelper.user == null)
            Close();
        
        if (!ApiHelper.user.IsVerified)
        {
            DrawLicenseRedeem();
            return;
        }
        EditorGUILayout.EndVertical();
        
        //Version check aka server availability todo

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Welcome to the nanoSDK Dashboard!", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Logout"))
        {
            GetWindow(typeof(LoginProcess), false, "nanoSDK Account").Show();
            ApiHelper.Logout();
            Close();
        }
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

    private void OnEnable()
    {
        titleContent = new GUIContent("nanoSDK Dashboard");
    }
    
}