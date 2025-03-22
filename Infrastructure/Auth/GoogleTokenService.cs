using Google.Apis.Auth;
using System.Text.Json;
using SpotMapApi.Models.DTOs;

namespace SpotMapApi.Infrastructure.Auth
{
    public class GoogleTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleTokenService> _logger;
        private readonly HttpClient _httpClient;

        public GoogleTokenService(
            IConfiguration configuration,
            ILogger<GoogleTokenService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }

        public async Task<TokenResponse> ExchangeAuthCodeForTokensAsync(string authCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("client_id", _configuration["Google:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["Google:ClientSecret"]),
                new KeyValuePair<string, string>("redirect_uri", _configuration["Google:RedirectUri"]),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error exchanging auth code for tokens: {ResponseContent}", responseContent);
                response.EnsureSuccessStatusCode();
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
            return tokenResponse;
        }
    }
}