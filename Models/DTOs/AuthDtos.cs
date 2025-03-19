using System.Text.Json.Serialization;

namespace SpotMapApi.Models.DTOs
{
    public class GoogleAuthRequest
    {
        public string AuthCode { get; set; } = string.Empty;
    }

    public record UserProfileResponse(string Id, string Email, string Name, string? Picture);
    
    public record RefreshRequest(string AccessToken, string RefreshToken);
    
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
    
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public UserProfileResponse User { get; set; } = null!;
    }
}