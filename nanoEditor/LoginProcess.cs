using nanoEditor.Auth;
using nanoEditor.Styles;
using UnityEditor;
using UnityEngine;

namespace nanoEditor;

public class LoginProcess : EditorWindow
{

    private bool _loginOpen = true;
    private string _usernameInput;
    private string _passwordInput;
    private bool _signUpOpen;
    private string _emailInput;


    [MenuItem("nanoSDK/Dashboard")]
    public static void ShowWindow()
    {
        GetWindow(typeof(LoginProcess), false, "nanoSDK Account");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        DrawLoginAndSignUp();
        EditorGUILayout.EndVertical();
    }

    private void OnLostFocus()
    {
        Focus();
    }

    private async void DrawLoginAndSignUp()
    {
        if (await ApiHelper.IsUserLoggedIn() || await ApiHelper.IsLoggedInAndVerified())
        {
            ApiHelper.CheckUserSelf();
            NanoDashboard.ShowDashboard();
            Close();
            return;
        }
        _loginOpen = FoldoutTexture.MakeTextFoldout("Login", _loginOpen);
        if (_loginOpen)
        {
            UserInterfaces.LoginGUI.DrawLoginGUI(ref _usernameInput, ref _passwordInput);
        }
        _signUpOpen = FoldoutTexture.MakeTextFoldout("Sign Up", _signUpOpen);
        if (_signUpOpen)
        {
            var signUpSuccessful = UserInterfaces.SignupGUI.DrawSignupGUI(ref _usernameInput, ref _passwordInput, ref _emailInput);
            if (!signUpSuccessful) return;
            EditorUtility.DisplayDialog("SignUp", "Check you email, and activate your account!", "Okay");
            Close();
        }
    }

}