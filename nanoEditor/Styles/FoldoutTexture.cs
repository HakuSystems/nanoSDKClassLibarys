using UnityEditor;
using UnityEngine;

namespace nanoEditor.Styles;

public class FoldoutTexture
{
    //standard is : 30f, 0, 0, 12f, 5f
    public static bool MakeTextFoldout(string text, bool boolToggle, float floatHeight = 30f, float floatOffsetX = 0, float floatOffsetY = 0, float floatArrowOffsetY = 12f, float floatArrowOffsetX = 5f)
    {
        var formatting = new GUIStyle(GUIStyleTypes.ScriptText.ToString())
        {
            contentOffset = new Vector2(18.0f + floatOffsetX, -2.0f + floatOffsetY),
            fixedHeight = floatHeight
        };

        var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, floatHeight, formatting);
        GUI.Box(rect, text, formatting);
        return MakeTextureFoldoutState(boolToggle, rect, floatArrowOffsetY, floatArrowOffsetX);
    }
    private static bool MakeTextureFoldoutState(bool boolState, Rect rectSize, float floatOffsetY, float floatOffsetX)
    {
        var currentEvent = Event.current;
        var arrowRect = new Rect(rectSize.x + floatOffsetX, rectSize.y + floatOffsetY, 0.0f, 0.0f);
        switch (currentEvent.type)
        {
            case EventType.Repaint:
                EditorStyles.foldout.Draw(arrowRect, false, false, boolState, false);
                break;
            case EventType.MouseDown when rectSize.Contains(currentEvent.mousePosition):
                boolState = !boolState;
                currentEvent.Use();
                break;
        }

        return boolState;
    }
}