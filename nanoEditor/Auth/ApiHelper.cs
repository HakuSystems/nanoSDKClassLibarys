using System.Text;
using nanoEditor.Configs;
using nanoEditor.Logger;
using nanoEditor.Models;
using Newtonsoft.Json;
using UnityEditor;
using Random = UnityEngine.Random;

namespace nanoEditor.Auth;

public class ApiHelper
{
    private static ConfigManager _configManager = new();
    private static readonly HttpClient Client = new();
    public static ApiData.ApiUser user;

    private const string BaseURL = "https://api.naxokit.com";
    private static readonly Uri UserSelfUri = new(BaseURL + "/user/self");
    private static readonly Uri RedeemUri = new(BaseURL + "/user/redeemables/redeem");
    private static readonly Uri LoginUri = new(BaseURL + "/user/login");
    private static readonly Uri SignupUri = new(BaseURL + "/user/signup");
    private static bool _running;
    private const string AppJson = "application/json";

    public static bool IsLoggedInAndVerified()
    {
        return IsUserLoggedIn() && user.IsVerified;
    }

    public static bool IsUserLoggedIn()
    {
        if (user == null && !string.IsNullOrEmpty(_configManager.Config.NanoAuth.AuthKey) && !_running) CheckUserSelf();
        return user != null;
    }


    private static void ClearLogin()
    {
        NaxoLog.Log("ApiHelper", "Clearing Login Data");
        //NanoDashboard.finallyLoggedIn = false; Todo
        user = null;
        _configManager.UpdateConfig(config => config.NanoAuth.AuthKey = null);
        //DiscordRPC.naxokitRPC.UpdateRPC(); Todo
    }

    private static async Task<HttpResponseMessage> MakeApiCall(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_configManager.Config.NanoAuth.AuthKey))
            request.Headers.Add("Auth-Key", _configManager.Config.NanoAuth.AuthKey);

        var response = await Client.SendAsync(request);
        var data = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<object>>(
            await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode) return response;
        NaxoLog.LogWarning("ApiHelper", "Failed to make Api Call: " + data.Message);
        ClearLogin();
        return response;
    }

    private static async void CheckUserSelf()
    {
        _running = true;
        NaxoLog.Log("ApiHelper", "Checking User");
        var request = new HttpRequestMessage(HttpMethod.Get, UserSelfUri);
        var response = await MakeApiCall(request);
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<ApiData.ApiUser>>(json);
        if (data != null) user = data.Data;
        NaxoLog.Log("ApiHelper", "Sucsessfully got User: " + user.Username);
        //NanoDashboard.SetFinallyLoggedIn(true); Todo
        _configManager.UpdateConfig(config =>
        {
            config.PremiumCheck.IsPremiumSinceLastCheck = user.IsPremium;
            config.PremiumCheck.LastCheck = DateTime.Now;
        });
        _running = false;
    }

    public static async void RedeemLicense(string code)
    {
        var content = new StringContent(JsonConvert.SerializeObject(new ApiData.ApiLicenseData { Key = code }),
            Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, RedeemUri) { Content = content };
        var response = await MakeApiCall(request);
        var data = JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<object>>(
            await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode)
        {
            NaxoLog.LogWarning("ApiHelper", "License Redeem Failed: " + data.Message);
            EditorUtility.DisplayDialog("ApiHelper", data.Message, "OK");
            return;
        }

        NaxoLog.Log("ApiHelper", "License Redeemed Successfully");
        EditorUtility.DisplayDialog("ApiHelper", "License Redeemed Successfully", "OK");
        CheckUserSelf();
    }

    public static async void Login(string username, string password)
    {
        var content =
            new StringContent(
                JsonConvert.SerializeObject(new ApiData.ApiLoginData { Username = username, Password = password }),
                Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, LoginUri) { Content = content };
        var response = await MakeApiCall(request);
        var data =
            JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<ApiData.ApiLoginResponse>>(
                await response.Content.ReadAsStringAsync());
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            NaxoLog.LogWarning("ApiHelper", "Credentials cant be empty");
        if (!response.IsSuccessStatusCode)
        {
            NaxoLog.LogWarning("ApiHelper", "Login Failed: " + data.Message);
            EditorUtility.DisplayDialog("ApiHelper", data.Message, "OK");
            return;
        }

        NaxoLog.Log("ApiHelper", "Login Successful");
        
        // if (NanoDashboard.savePasswordLocally) Todo
        //     SaveRecivedPassword(password); Todo
        
        _configManager.UpdateConfig(config =>
        {
            config.NanoAuth.AuthKey = data.Data.AuthKey;
            config.TermsPolicy.Accepted = true;
        });
        
        CheckUserSelf();
        //NanoDashboard.SetFinallyLoggedIn(true); Todo
        //naxokit.DiscordRPC.naxokitRPC.UpdateRPC(); Todo
    }

    private static void SaveRecivedPassword(string password)
    {
        NaxoLog.Log("ApiHelper", "Saving Password");
        _configManager.UpdateConfig(config =>
        {
            config.NanoAuth.Password = password;
        });
    }

    public static string GetSavedPassword()
    {
        if (string.IsNullOrEmpty(_configManager.Config.NanoAuth.Password))
            return "";
        return _configManager.Config.NanoAuth.Password;
    }

    public static void Logout()
    {
        ClearLogin();
    }

    public static async void SignUp(string username, string password, string email)
    {
        var content =
            new StringContent(
                JsonConvert.SerializeObject(new ApiData.ApiSignupData
                    { Username = username, Password = password, Email = email }), Encoding.UTF8, AppJson);
        var request = new HttpRequestMessage(HttpMethod.Post, SignupUri) { Content = content };
        var response = await MakeApiCall(request);
        var data =
            JsonConvert.DeserializeObject<ApiData.ApiBaseResponse<ApiData.ApiSanityCheckResponse>>(
                await response.Content.ReadAsStringAsync());
        if (data != null && data.Message.Contains("Sanity checks"))
        {
            var sb = new StringBuilder();
            string usernameArray = null;
            string passwordArray = null;
            string emailArray = null;
            foreach (var item in data.Data.UsernameSanityCheck)
            {
                sb.AppendLine(item.Value);
                usernameArray = sb.ToString();
            }

            sb.Clear();
            foreach (var item in data.Data.PasswordSanityCheck)
            {
                sb.AppendLine(item.Value);
                passwordArray = sb.ToString();
            }

            sb.Clear();
            foreach (var item in data.Data.EmailSanityCheck)
            {
                sb.AppendLine(item.Value);
                emailArray = sb.ToString();
            }

            if (string.IsNullOrEmpty(usernameArray)) usernameArray = "No Errors";
            if (string.IsNullOrEmpty(passwordArray)) passwordArray = "No Errors";
            if (string.IsNullOrEmpty(emailArray)) emailArray = "No Errors";
            EditorUtility.DisplayDialog("ApiHelper",
                "Signup Failed: " + data.Message + "\n\nUsername Errors:\n" + usernameArray + "\n\nPassword Errors:\n" +
                passwordArray + "\n\nEmail Errors:\n" + emailArray, "OK");
        }

        if (!response.IsSuccessStatusCode)
        {
            NaxoLog.LogWarning("ApiHelper", "Signup Failed: " + data.Message);
            return;
        }

        NaxoLog.Log("ApiHelper", "Signup Successful");
        EditorUtility.DisplayDialog("ApiHelper", "Signup Successful", "OK");
    }

    public static string ApiGenerateStrongPassword()
    {
        const int minLenght = 8;
        const int maxLenght = 128;
        const int minLowerCase = 1;
        const int minNumber = 1;
        const int minSpecialChar = 1;
        const string allowedSpecials = "@#$%/.!'_-";
        const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";

        var password = "";

        var allowedCharsLenght = allowedChars.Length;
        var allowedSpecialsLenght = allowedSpecials.Length;
        var passwordLenght = Random.Range(minLenght, maxLenght);
        var lowerCaseLetters = Random.Range(minLowerCase, passwordLenght);
        var numbers = Random.Range(minNumber, passwordLenght);
        var specialChars = Random.Range(minSpecialChar, passwordLenght);
        for (var i = 0; i < lowerCaseLetters; i++) password += allowedChars[Random.Range(0, allowedCharsLenght)];
        for (var i = 0; i < numbers; i++) password += allowedChars[Random.Range(0, allowedCharsLenght)];
        for (var i = 0; i < specialChars; i++) password += allowedSpecials[Random.Range(0, allowedSpecialsLenght)];
        var passwordArray = password.ToCharArray();
        var rng = new System.Random();
        var n = passwordArray.Length;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (passwordArray[k], passwordArray[n]) = (passwordArray[n], passwordArray[k]);
        }

        NaxoLog.Log("ApiHelper", "Generated Strong Password!");
        return new string(passwordArray);
    }
}