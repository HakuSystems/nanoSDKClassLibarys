using nanoEditor.Auth;
using nanoEditor.Styles;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces;

public class LoginGUI
{
    public static bool DrawLoginGUI(ref string usernameInput, ref string passwordInput)
    {
        EditorGUILayout.LabelField("Login");
        usernameInput = EditorGUILayout.TextField("Username", usernameInput);
        passwordInput = EditorGUILayout.PasswordField("Password", passwordInput);
        var termsOfService = new GUIStyle(GUIStyleTypes.toolbarbutton.ToString());
        EditorGUILayout.BeginHorizontal();
        {
                
            EditorGUILayout.LabelField("By logging in you agree to our ");
            if (GUILayout.Button("Terms of Service", termsOfService))
                Application.OpenURL("https://nanosdk.net/terms-of-service.html");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Login"))
        {
            if (string.IsNullOrEmpty(usernameInput) || string.IsNullOrEmpty(passwordInput))
                EditorUtility.DisplayDialog("Login", "Credentials cant be Empty", "Okay");
            else
                ApiHelper.Login(usernameInput, passwordInput);
        }
        return false;
    }
}