using SpotMapApi.Models.DTOs;

namespace SpotMapApi.Services.Auth
{
    public interface IGoogleAuthService
    {
        Task<AuthResponse> AuthenticateWithGoogleAsync(string authCode);
    }
}