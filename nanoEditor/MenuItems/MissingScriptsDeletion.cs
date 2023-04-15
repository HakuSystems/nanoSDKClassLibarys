using nanoEditor.Logger;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.MenuItems;

public class MissingScriptsDeletion : EditorWindow
{
    [MenuItem("nanoSDK/Tools/Delete Missing Scripts")]
    public static void DeleteMissingScripts()
    {
        var allGameObjects = FindObjectsOfType<GameObject>();
        var gameObjectsWithMissingScripts = (from go in allGameObjects
            let components = go.GetComponents<Component>()
            where components.Any(component => component == null)
            select go).ToList();

        foreach (var go in gameObjectsWithMissingScripts)
        {
            var serializedObject = new SerializedObject(go);
            var prop = serializedObject.FindProperty("m_Component");

            for (var i = prop.arraySize - 1; i >= 0; i--)
            {
                var componentProperty = prop.GetArrayElementAtIndex(i);
                componentProperty = componentProperty.FindPropertyRelative("component");
                if (componentProperty.objectReferenceValue == null) prop.DeleteArrayElementAtIndex(i);
            }

            serializedObject.ApplyModifiedProperties();
        }

        NanoLog.Log("MissingScriptsDeletion",
            $"Deleted {gameObjectsWithMissingScripts.Count} GameObjects with missing scripts");
    }
}