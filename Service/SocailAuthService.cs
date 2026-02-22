using Empath_AI.DTO.User;
using Google.Apis.Auth;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class SocialAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SocialAuthService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    
    public async Task<UserSocialLoginDTO> ValidateSocialTokenAsync(string provider, string token)
    {
        provider = provider?.ToLower();

        return provider switch
        {
            "google" => await ValidateGoogleTokenAsync(token),
            "facebook" => await ValidateFacebookTokenAsync(token),
            _ => throw new Exception("Unsupported provider")
        };
    }

    // ================= GOOGLE =================
    private async Task<UserSocialLoginDTO> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            /*// 🔒 Extra security: verify audience (your Google client id)
            var clientId = _config["Authentication:Google:ClientId"];
            if (payload.Audience != clientId)
                throw new Exception("Invalid Google audience");*/

            if (!payload.EmailVerified)
                throw new Exception("Google email not verified");

            return new UserSocialLoginDTO
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                ImageUrl = payload.Picture,
                Provider = "Google"
            };
        }
        catch
        {
            throw new Exception("Invalid Google token");
        }
    }

    // ================= FACEBOOK =================
    private async Task<UserSocialLoginDTO> ValidateFacebookTokenAsync(string accessToken)
    {
        var url =
            $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={accessToken}";

        var response = await _httpClient.GetAsync(url);
        /*
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Invalid Facebook token");*/
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Facebook response: {json}");

        //var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var pictureUrl = root
            .GetProperty("picture")
            .GetProperty("data")
            .GetProperty("url")
            .GetString();

        return new UserSocialLoginDTO
        {
            Email = root.GetProperty("email").GetString(),
            FirstName = root.GetProperty("first_name").GetString(),
            LastName = root.GetProperty("last_name").GetString(),
            ImageUrl = pictureUrl,
            Provider = "Facebook"
        };
    }
}