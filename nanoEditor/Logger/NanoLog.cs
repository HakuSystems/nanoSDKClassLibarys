using UnityEditor;
using UnityEngine;

namespace nanoEditor.Logger;

public class NanoLog
{
    public static void Log(string className, string message)
    {
        string formattedMesasge = FormatLogMessage(className, message);
        Debug.Log(formattedMesasge);
    }
    public static void LogWarning(string className, string message)
    {
        string formattedMesasge = FormatLogMessage(className, message);
        Debug.LogWarning(formattedMesasge);
    }
    private static string FormatLogMessage(string className, string message)
    {
        string logPrefix = $"[{className}]";
        string fancyMessage = WrapTextInBox(logPrefix + message);
        return fancyMessage;
    }

    private static string WrapTextInBox(string text)
    {
        string horizontalLine = new string('-', text.Length +2);
        string wrappedText = $"{horizontalLine}\n|{text}|\n{horizontalLine}";
        return wrappedText;
    }
    public static void ExceptionHandler(string file_name, string message)
    {
        LogWarning(file_name, message);
        EditorUtility.DisplayDialog(file_name, message, "Ok");
    }
}