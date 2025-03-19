namespace SpotMapApi.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "SpotMapApi";
        public string Audience { get; set; } = "SpotMapClient";
        public int AccessTokenExpiryInMinutes { get; set; } = 15;
        public int RefreshTokenExpiryInDays { get; set; } = 7;
    }
}