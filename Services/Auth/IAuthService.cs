using SpotMapApi.Models.DTOs;

namespace SpotMapApi.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<bool> LogoutAsync(string userId, string refreshToken);
    }
}