using System.Collections;
using System.Text;
using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.Models;
using Newtonsoft.Json;
using UnityEditor;

namespace nanoEditor.Auth;

public static class ApiHelper
{
    private static ConfigManager _configManager = new();
    private static readonly HttpClient Client = new();
    public static ApiData.ApiUser user;

    private const string BaseURL = "https://api.naxokit.com";
    private static readonly Uri UserSelfUri = new Uri($"{BaseURL}/user/self");
    private static readonly Uri RedeemUri = new Uri($"{BaseURL}/user/redeemables/redeem");
    private static readonly Uri LoginUri = new Uri($"{BaseURL}/user/login");
    private static readonly Uri SignupUri = new Uri($"{BaseURL}/user/signup");
    private static bool _running;
    private const string AppJson = "application/json";

    public static async Task<bool> IsLoggedInAndVerified()
    {
        return await IsUserLoggedIn() && user.IsVerified;
    }

    public static async Task<bool> IsUserLoggedIn()
    {
        if (user == null && !string.IsNullOrEmpty(_configManager.Config.NanoAuth.AuthKey) && !_running)
            await CheckUserSelf();
        return user != null; 
    }

    private static void ClearLogin()
    {
        NanoLog.Log("ApiHelper", "Clearing Login Data");
        user = null;
        _configManager.UpdateConfig(config => config.NanoAuth.AuthKey = null);
        //DiscordRPC.naxokitRPC.UpdateRPC(); Todo
    }

    private static async Task<HttpResponseMessage> MakeApiCall(HttpRequestMessage request)
    {
        AddAuthKeyHeader(request);
        var response = await Client.SendAsync(request);
        var data = DeserializeApiResponse<object>(await response.Content.ReadAsStringAsync());

        if (response.IsSuccessStatusCode) return response;

        NanoLog.LogWarning("ApiHelper", "Failed to make Api Call: " + data.Message);
        ClearLogin();
        return response;
    }
    
    private static void AddAuthKeyHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_configManager.Config.NanoAuth.AuthKey))
            request.Headers.Add("Auth-Key", _configManager.Config.NanoAuth.AuthKey);
    }

    public static async Task CheckUserSelf()
    {
        if (_running)
        {
            return;
        }
        _running = true;
        var request = new HttpRequestMessage(HttpMethod.Get, UserSelfUri);
        var response = await MakeApiCall(request);
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<ApiData.ApiUser>>(json);
        if (data != null) user = data.Data;
        NanoLog.Log("ApiHelper", "Sucsessfully got User: " + user.Username);
        _configManager.UpdateConfig(config =>
        {
            config.PremiumCheck.IsPremiumSinceLastCheck = user.IsPremium;
            config.PremiumCheck.LastCheck = DateTime.Now;
        });
        _running = false;
    }

    public static async void RedeemLicense(string code)
    {
        var response = await SendRedeemLicenseRequest(code);
        var data = DeserializeApiResponse<object>(await response.Content.ReadAsStringAsync());

        if (!response.IsSuccessStatusCode)
        {
            NanoLog.LogWarning("ApiHelper", "License Redeem Failed: " + data.Message);
            EditorUtility.DisplayDialog("ApiHelper", data.Message, "OK");
            return;
        }

        NanoLog.Log("ApiHelper", "License Redeemed Successfully");
        EditorUtility.DisplayDialog("ApiHelper", "License Redeemed Successfully", "OK");
        await CheckUserSelf();
    }

    public static async void Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            NanoLog.LogWarning("ApiHelper", "Credentials can't be empty");
            return;
        }

        var response = await SendLoginRequest(username, password);
        var data = DeserializeApiResponse<ApiData.ApiLoginResponse>(await response.Content.ReadAsStringAsync());

        if (!response.IsSuccessStatusCode)
        {
            NanoLog.LogWarning("ApiHelper", "Login Failed: " + data.Message);
            EditorUtility.DisplayDialog("ApiHelper", data.Message, "OK");
            return;
        }

        HandleSuccessfulLogin(data);
        await CheckUserSelf();
    }
    
    private static async Task<HttpResponseMessage> SendRedeemLicenseRequest(string code)
    {
        var content = new StringContent(JsonConvert.SerializeObject(new ApiData.ApiLicenseData { Key = code }),
            Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, RedeemUri) { Content = content };
        return await MakeApiCall(request);
    }

    private static async Task<HttpResponseMessage> SendLoginRequest(string username, string password)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(new ApiData.ApiLoginData { Username = username, Password = password }),
            Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, LoginUri) { Content = content };
        return await MakeApiCall(request);
    }
    
    private static void HandleSuccessfulLogin(ApiData.ApiBaseResponse<ApiData.ApiLoginResponse> data)
    {
        NanoLog.Log("ApiHelper", "Login Successful");

        _configManager.UpdateConfig(config =>
        {
            config.NanoAuth.AuthKey = data.Data.AuthKey;
            config.TermsPolicy.Accepted = true;
        });
    }
    
    public static void Logout() => ClearLogin();

    public static async void SignUp(string username, string password, string email)
    {
        var response = await SendSignupRequest(username, password, email);
        var data = DeserializeApiResponse<ApiData.ApiSanityCheckResponse>(await response.Content.ReadAsStringAsync());

        if (data != null && data.Message.Contains("Sanity checks"))
        {
            HandleSignupFailure(data);
        }

        if (!response.IsSuccessStatusCode)
        {
            if (data != null) NanoLog.LogWarning("ApiHelper", $"Signup Failed: {data.Message}");
            return;
        }

        NanoLog.Log("ApiHelper", "Signup Successful");
    }

    private static void HandleSignupFailure(ApiData.ApiBaseResponse<ApiData.ApiSanityCheckResponse> data)
    {
        var usernameErrors = FormatErrorMessages(data.Data.UsernameSanityCheck);
        var passwordErrors = FormatErrorMessages(data.Data.PasswordSanityCheck);
        var emailErrors = FormatErrorMessages(data.Data.EmailSanityCheck);
        
        EditorUtility.DisplayDialog("ApiHelper", $"Signup Failed: {data.Message}\n\n" +
                                                 $"Username Errors:\n{usernameErrors}\n\n" +
                                                 $"Password Errors:\n{passwordErrors}\n\n" +
                                                 $"Email Errors:\n{emailErrors}", "OK");
    }

    private static object FormatErrorMessages(Dictionary<string, string> dataUsernameSanityCheck)
    {
        var sb = new StringBuilder();
        foreach (var item in dataUsernameSanityCheck)
        {
            sb.AppendLine(item.Value);
        }

        var errors = sb.ToString();
        return string.IsNullOrEmpty(errors) ? "No Errors" : errors;
    }

    private static ApiData.ApiBaseResponse<T> DeserializeApiResponse<T>(string content)
    {
        return JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<T>>(content);
    }

    private static async Task<HttpResponseMessage> SendSignupRequest(string username, string password, string email)
    {
        var content = new StringContent(JsonConvert.SerializeObject(new ApiData.ApiSignupData
            {Username = username, Password = password, Email = email}), Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, SignupUri) {Content = content};
        return await MakeApiCall(request);
    }
}