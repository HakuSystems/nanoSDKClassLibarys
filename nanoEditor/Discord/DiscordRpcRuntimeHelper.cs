using UnityEditor;
using UnityEngine.SceneManagement;

namespace nanoEditor.Discord;

[InitializeOnLoad]
public static class DiscordRpcRuntimeHelper
{
    // register an event handler when the class is initialized
    static DiscordRpcRuntimeHelper()
    {
        Selection.selectionChanged += UpdateHierachySelection;
    }

    private static void UpdateHierachySelection()
    {
        var selectedObject = Selection.activeGameObject;
        if(selectedObject == null) return;
        nanoSDKDiscordRpc.UpdateHierarchyElement(selectedObject.name);
    }
}