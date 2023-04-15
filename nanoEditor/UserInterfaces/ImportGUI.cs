using System.ComponentModel;
using System.Net;
using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.Models;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace nanoEditor.UserInterfaces;

public class ImportGUI
{
    private static readonly ConfigManager ConfigManager = new();
    private static bool _doneMemoryAllocation;
    private static string[] _importableAssets;
    private static readonly Uri ImportUrl = new("https://nanosdk.net/assets/importConfig.json");

    public static void DrawImportGUI()
    {
        if (!_doneMemoryAllocation)
        {
            using (var timer = new Timer(Callback, null, 60000, Timeout.Infinite))
            {
                EditorGUILayout.LabelField("Loading Importable Packages...", EditorStyles.centeredGreyMiniLabel);
                GetImportablePackages();
                NanoLog.Log("nanoSDKImporter", "Done loading importable packages");
            }

            return;
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("If the window has been refreshed, then everything should be good.",
            EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("Importable Packages", EditorStyles.boldLabel);
        foreach (var asset in _importableAssets)
        {
            if (asset == null) continue;
            EditorGUILayout.BeginHorizontal();
            var assetName = asset.Substring(0, asset.Length - 13); //removes .unitypackage for better visual
            EditorGUILayout.LabelField(assetName, EditorStyles.toolbarTextField);
            if (GUILayout.Button("Import")) DownloadAndImport(asset);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private static void Callback(object state)
    {
        if (!_doneMemoryAllocation)
            EditorUtility.DisplayDialog("nanoSDK Importer",
                "Couldn't load importable packages. Check your internet connection!", "Ok");
    }

    private static void DownloadAndImport(string asset)
    {
        EditorUtility.DisplayProgressBar("Please Wait", "Loading...",
            0); // fake loading for bad internet or no internet
        var tempDirectory = Path.Combine(ConfigManager.Config.DefaultPath.DefaultSet, "tmp");
        if (!Directory.Exists(tempDirectory))
            Directory.CreateDirectory(tempDirectory);
        using var client = new WebClient();
        try
        {
            client.Headers.Set(HttpRequestHeader.UserAgent, "Webkit Gecko wHTTPS (Keep Alive 55)");
            client.QueryString.Add("asset", asset);
            client.DownloadFileAsync(new Uri($"{ImportData.ServerUrl}{asset}"), Path.Combine(tempDirectory, asset));
            client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            client.DownloadFileCompleted += ClientOnDownloadFileCompletedAsync;
        }
        catch (Exception ex)
        {
            NanoLog.ExceptionHandler("nanoSDKImporter", $"Error downloading {asset}: {ex.Message}");
            EditorUtility.ClearProgressBar();
            if (File.Exists(Path.Combine(ConfigManager.Config.DefaultPath.DefaultSet, "tmp", asset)))
                File.Delete(Path.Combine(ConfigManager.Config.DefaultPath.DefaultSet, "tmp", asset));
        }
    }

    private static void ClientOnDownloadFileCompletedAsync(object sender, AsyncCompletedEventArgs e)
    {
        var assetName = ((WebClient)sender).QueryString["asset"];
        if (e.Error != null)
        {
            NanoLog.ExceptionHandler("nanoSDKImporter", $"Failed to download {assetName}: {e.Error.Message}");
            return;
        }

        NanoLog.Log("nanoSDKImporter", $"Downloaded {assetName}!");
        EditorUtility.ClearProgressBar();
        AssetDatabase.ImportPackage(
            Path.Combine(ConfigManager.Config.DefaultPath.DefaultSet, "tmp", assetName), false);
        AssetDatabase.importPackageCompleted += AssetDatabaseOnImportPackageCompleted;
    }

    private static void AssetDatabaseOnImportPackageCompleted(string packagename)
    {
        File.Delete(Path.Combine(ConfigManager.Config.DefaultPath.DefaultSet, "tmp", packagename));
    }

    private static void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        EditorUtility.DisplayProgressBar("nanoSDK Importer", "Downloading...", e.ProgressPercentage / 100f);
    }

    private static void GetImportablePackages()
    {
        EditorUtility.DisplayProgressBar("Please Wait", "Loading...",
            0); // fake loading for bad internet or no internet
        try
        {
            using (var client = new WebClient())
            {
                var json = client.DownloadString(ImportUrl); //reads and memory allocates the whole file
                var config = JsonConvert.DeserializeObject<ImportData>(json);
                _importableAssets = config.Assets.Values.Select(x => x.File).ToArray();
                _doneMemoryAllocation = true;
                EditorUtility.ClearProgressBar();
            }
        }
        catch (Exception ex)
        {
            NanoLog.ExceptionHandler("nanoSDKImporter", $"Error fetching importable packages: {ex.Message}");
            EditorUtility.ClearProgressBar();
        }
    }
}