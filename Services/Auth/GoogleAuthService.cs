using Google.Apis.Auth;
using SpotMapApi.Data.UnitOfWork;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Models.Entities;
using SpotMapApi.Infrastructure.Auth;

namespace SpotMapApi.Services.Auth
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly GoogleTokenService _googleTokenService;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            GoogleTokenService googleTokenService,
            ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _googleTokenService = googleTokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> AuthenticateWithGoogleAsync(string authCode)
        {
            try
            {
                // Exchange authorization code for tokens using infrastructure service
                var tokenResponse = await _googleTokenService.ExchangeAuthCodeForTokensAsync(authCode);

                // Validate the ID token using infrastructure service
                var payload = await _googleTokenService.ValidateGoogleTokenAsync(tokenResponse.IdToken);

                // Find or create user - this is business logic
                var user = await _unitOfWork.Users.GetByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                        Picture = payload.Picture
                    };
                    await _unitOfWork.Users.AddAsync(user);
                }
                else
                {
                    // Update user info if needed
                    user.Name = payload.Name;
                    user.Picture = payload.Picture;
                    await _unitOfWork.Users.UpdateAsync(user);
                }

                // Generate JWT tokens using infrastructure service
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Save refresh token to database
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                    Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"]));

                await _unitOfWork.SaveChangesAsync();

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryInMinutes"]) * 60,
                    User = new SpotMapApi.Models.DTOs.UserProfileResponse(user.Id, user.Email, user.Name, user.Picture)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Google");
                throw;
            }
        }
    }
}