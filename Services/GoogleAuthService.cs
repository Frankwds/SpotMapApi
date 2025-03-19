using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly MarkerContext _context;
    private readonly JwtService _jwtService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IConfiguration configuration,
        MarkerContext context,
        JwtService jwtService,
        ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> AuthenticateWithGoogleAsync(string authCode)
    {
        try
        {
            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeAuthCodeForTokensAsync(authCode);
            //log tokenResponse
            _logger.LogInformation(tokenResponse.ToString());
            // Validate the ID token
            var payload = await ValidateGoogleTokenAsync(tokenResponse.IdToken);

            // Find or create user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture
                };
                _context.Users.Add(user);
            }
            else
            {
                // Update user info if needed
                user.Name = payload.Name;
                user.Picture = payload.Picture;
            }

            // Generate JWT tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token to database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"]));

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryInMinutes"]) * 60,
                User = new UserProfileResponse(user.Id, user.Email, user.Name, user.Picture)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Google");
            throw;
        }
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["Google:ClientId"] }
        };

        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }

    private async Task<TokenResponse> ExchangeAuthCodeForTokensAsync(string authCode)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("code", authCode),
        new KeyValuePair<string, string>("client_id", _configuration["Google:ClientId"]),
        new KeyValuePair<string, string>("client_secret", _configuration["Google:ClientSecret"]),
        new KeyValuePair<string, string>("redirect_uri", "http://localhost:3000"),
        new KeyValuePair<string, string>("grant_type", "authorization_code")
    });
        request.Content = content;

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error exchanging auth code for tokens: {ResponseContent}", responseContent);
            response.EnsureSuccessStatusCode();
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
        _logger.LogInformation(responseContent);

        return tokenResponse;
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

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