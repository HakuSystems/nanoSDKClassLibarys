using nanoEditor.Auth;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces;

public class SignupGUI
{
    public static bool DrawSignupGUI(ref string usernameInput, ref string passwordInput, ref string emailInput)
    {
        usernameInput = EditorGUILayout.TextField("Username", usernameInput);
        passwordInput = EditorGUILayout.PasswordField("Password", passwordInput);
        emailInput = EditorGUILayout.TextField("Email", emailInput);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Account"))
        {
            if (string.IsNullOrEmpty(usernameInput) || string.IsNullOrEmpty(passwordInput) ||
                string.IsNullOrEmpty(emailInput))
            {
                EditorUtility.DisplayDialog("SignUp", "Credentials cant be Empty", "Okay");
            }
            else
            {
                ApiHelper.SignUp(usernameInput, passwordInput, emailInput);
                return true;
            }
        }
        EditorGUILayout.EndHorizontal();
        return false;
    }

}