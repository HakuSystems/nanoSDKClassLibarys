using System.ComponentModel;
using System.Net;
using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.Models;
using nanoEditor.UserInterfaces.Tools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace nanoEditor.Version;

public class Updater : MonoBehaviour
{
    private static ConfigManager _configManager = new();
    private const string ScriptName = "nanoSDK Updater";

    private static readonly HttpClient HttpClient = new();
    private static readonly WebClient WebClient = new();

    private const string BaseURL = "https://api.naxokit.com";

    private static readonly Uri ReleaseVersionUri = new($"{BaseURL}/public/naxokit/version"); //Only for Branch: release
    private static readonly Uri VersionListUri = new($"{BaseURL}/public/naxokit/version/list"); //Lists all Branches

    private static NanoVersionData LatestVersion { get; set; }
    

    public static async Task CheckForUpdates(Enum branch = null)
    {
        NanoLog.Log(ScriptName, "Checking for updates...");
        if (branch == null)
            branch = _configManager.Config.NanoVersion.Branch;
        LatestVersion = await GetLatestVersion(branch);
        if (LatestVersion == null)
        {
            NanoLog.Log(ScriptName, "No updates found.");
            return;
        }

        if (_configManager.Config.NanoVersion.Version != LatestVersion.Version)
        {
            NanoLog.Log(ScriptName, "Update available!");
            
            var update = EditorUtility.DisplayDialog(ScriptName,
                $"Version V{LatestVersion.Version} is available. Do you want to update?", "Yes", "No");
            if (update)
            {
                BackupManager.CreateBackup(_configManager.Config.BackupManager.SaveAsUnitypackage,
                    _configManager.Config.BackupManager.SaveAsUnitypackage);
                await DownloadVersion();
            }
            return;
        }
        
        NanoLog.Log(ScriptName, "You are up to date!");
    }

    private static async Task<NanoVersionData> GetLatestVersion(Enum branch)
    {
        if (branch.Equals(NanoVersionData.BranchType.Release))
            return await GetReleaseVersion();
        return await GetBetaVersion();
    }

    private static async Task<NanoVersionData> GetBetaVersion()
    {
        NanoLog.Log(ScriptName, "Getting latest beta version...");
        
        var response = await HttpClient.GetAsync(VersionListUri);
        var content = await response.Content.ReadAsStringAsync();
        var versionList = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<List<NanoVersionData>>>(content);
        
        Debug.Assert(versionList != null, nameof(versionList) + " != null");
        var latestVersion = versionList.Data.OrderByDescending(x => x.Version).FirstOrDefault(x => x.Branch.Equals(NanoVersionData.BranchType.Beta));
        
        return latestVersion;
    }

    private static async Task<NanoVersionData> GetReleaseVersion()
    {
        NanoLog.Log(ScriptName, "Getting latest release version...");
        
        var response = HttpClient.GetAsync(ReleaseVersionUri);
        var content = await response.Result.Content.ReadAsStringAsync();
        var versionResponse = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<NanoVersionData>>(content);
        
        return versionResponse?.Data;
    }

    private static Task DownloadVersion(string url = null, string version = null, Enum branch = null)
    {
        try
        {
            WebClient.DownloadFileAsync(new Uri((LatestVersion?.Url ?? url) ?? string.Empty),
                $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/nanoSDK V{LatestVersion?.Version ?? version}.unitypackage");
            
            WebClient.DownloadProgressChanged += (sender, args) =>
            {
                EditorUtility.DisplayProgressBar("Downloading",
                    $"Downloading naxokit V{LatestVersion?.Version ?? version}", args.ProgressPercentage);
            };

            void OnWebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
            {
                var files = Directory.GetFiles(Application.dataPath + "/nanoSDK", "*.*", SearchOption.AllDirectories).Where(s => !s.EndsWith("Updater.cs"));


                EditorUtility.ClearProgressBar();
                var alleles = files as string[] ?? files.ToArray();
                foreach (var file in alleles)
                {
                    if (file.EndsWith(".dll") || file.EndsWith(".meta") || file.EndsWith(".tmp")) continue;
                    File.Delete(file);
                }

                AssetDatabase.Refresh();

                AssetDatabase.ImportPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/nanoSDK V{LatestVersion?.Version}.unitypackage", false);

                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/nanoSDK V{LatestVersion?.Version ?? version}.unitypackage");

                EditorUtility.DisplayDialog(ScriptName, "Update complete!", "Ok");

                UpdaterConfigChange(branch);
                WebClient.Dispose();
            }

            // ReSharper disable once HeapView.ObjectAllocation.Possible
            WebClient.DownloadFileCompleted += OnWebClientOnDownloadFileCompleted;
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog(ScriptName, e.Message, "ok");
        }

        return Task.CompletedTask;
    }

    private static void UpdaterConfigChange(Enum branch)
    {
        _configManager.UpdateConfig(config =>
        {
            _configManager.Config.NanoVersion.Url = LatestVersion?.Url;
            _configManager.Config.NanoVersion.Version = LatestVersion?.Version;
            _configManager.Config.NanoVersion.Branch = (NanoVersionData.BranchType)(LatestVersion?.Branch ?? branch);
            _configManager.Config.NanoVersion.Commit = LatestVersion?.Commit;
            _configManager.Config.NanoVersion.CommitUrl = LatestVersion?.CommitUrl;
            _configManager.Config.NanoVersion.CreatedOn = LatestVersion?.CreatedOn;
        });
    }
}