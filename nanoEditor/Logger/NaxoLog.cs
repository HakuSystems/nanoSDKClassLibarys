using UnityEngine;

namespace nanoEditor.Logger;

public class NaxoLog
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
    public static void LogError(string className, string message)
    {
        string formattedMesasge = FormatLogMessage(className, message);
        Debug.LogError(formattedMesasge);
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
}