using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using nanoEditor.Logger;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace nanoEditor;

public class NanoDashboard : EditorWindow
{
    [MenuItem("nanoSDK/Dashboard")]
    public static void ShowWindow() => GetWindow(typeof(NanoDashboard), false, "nanoSDK Dashboard");

    private void OnGUI()
    {
        NaxoLog.LogWarning("NanoDashboard", "This is a WIP");
    }
}