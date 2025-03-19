namespace SpotMapApi.Infrastructure.Configuration
{
    public class AppSettings
    {
        public JwtSettings Jwt { get; set; } = new JwtSettings();
        public GoogleSettings Google { get; set; } = new GoogleSettings();
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Provider { get; set; } = "SqlServer";
    }
}