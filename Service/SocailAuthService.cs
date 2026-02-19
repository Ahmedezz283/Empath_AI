using Empath_AI.DTO.User;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class SocialAuthService
{
    private readonly HttpClient _httpClient;

    public SocialAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Validates the social login token and returns the user info.
    /// Throws an exception if token is invalid.
    /// </summary>
    public async Task<UserSocialLoginDTO> ValidateSocialTokenAsync(string provider, string token)
    {
        provider = provider?.ToLower();

        switch (provider)
        {
            case "google":
                return await ValidateGoogleTokenAsync(token);
            case "facebook":
                return await ValidateFacebookTokenAsync(token);
            default:
                throw new Exception("Unsupported provider");
        }
    }

    private async Task<UserSocialLoginDTO> ValidateGoogleTokenAsync(string idToken)
    {
        // Google token validation endpoint
        var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Invalid Google token");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new UserSocialLoginDTO
        {
            Email = root.GetProperty("email").GetString(),
            FirstName = root.GetProperty("given_name").GetString(),
            LastName = root.GetProperty("family_name").GetString(),
            ImageUrl = root.GetProperty("picture").GetString(),
            Provider = "Google"
        };
    }

    private async Task<UserSocialLoginDTO> ValidateFacebookTokenAsync(string accessToken)
    {
        // Facebook Graph API endpoint
        var url = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={accessToken}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Invalid Facebook token");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var pictureUrl = root.GetProperty("picture").GetProperty("data").GetProperty("url").GetString();

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
