using nanoEditor.Configs;
using nanoEditor.Logger;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace nanoEditor.Discord;

[InitializeOnLoad]
public class nanoSDKDiscordRpc
{
    private static readonly ConfigManager _configManager = new();
    private static readonly DiscordRpc.RichPresence presence = new();

    private static TimeSpan _time = DateTime.UtcNow - new DateTime(1970, 1, 1);
    private static long _timestamp = (long)_time.TotalSeconds;

    private static readonly string GameName = Application.productName;
    public static void InitializeDiscordRpc() // gets called outsite of the dll
    {
        if (!_configManager.Config.DiscordPresence.PresenceEnabled) return;
        var eventHandlers = default(DiscordRpc.EventHandlers);
        eventHandlers.readyCallback = delegate { NanoLog.Log("DiscordRPC", "Ready"); };
        eventHandlers.disconnectedCallback += delegate(int errorCode, string message)
        {
            NanoLog.Log("DiscordRPC", $"Disconnected: {errorCode} - {message}");
        };
        eventHandlers.errorCallback += delegate(int errorCode, string message)
        {
            NanoLog.Log("DiscordRPC", $"Error: {errorCode} - {message}");
        };
        DiscordRpc.Initialize(_configManager.Config.DiscordPresence.ClientId, ref eventHandlers, false,
            string.Empty);
        UpdateDRPC();
    }

    private static void UpdateDRPC()
    {
        NanoLog.Log("DiscordRPC", "Updating everything");
        presence.details = $"Project: {GameName}";
        presence.state = "Working in nanoSDK";
        presence.startTimestamp = _timestamp;
        presence.largeImageKey = "nanosdk";
        presence.largeImageText = "In Unity with nanoSDK";
        presence.button1Text = "Download SDK";
        presence.button1Url = "https://nanosdk.net/";
        presence.button2Text = "Join Discord";
        presence.button2Url = "https://nanosdk.net/discord";
        DiscordRpc.UpdatePresence(presence);
    }

    public static void UpdateHierarchyElement(string elementName)
    {
        presence.largeImageText = $"Selected: {elementName}";
        DiscordRpc.UpdatePresence(presence);
    }
}