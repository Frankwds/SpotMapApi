using System.Security.Claims;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Services.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}