using System.Security.Claims;
using SpotMapApi.Data.UnitOfWork;
using SpotMapApi.Models.DTOs;

namespace SpotMapApi.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryInMinutes"]) * 60,
                User = new SpotMapApi.Models.DTOs.UserProfileResponse(user.Id, user.Email, user.Name, user.Picture)
            };
        }

        public async Task<bool> LogoutAsync(string userId, string refreshToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user != null && user.RefreshToken == refreshToken)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}