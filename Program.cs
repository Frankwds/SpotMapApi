using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using SpotMapApi.Data;
using SpotMapApi.Data.UnitOfWork;
using SpotMapApi.Services.Auth;
using SpotMapApi.Services.Markers;
using SpotMapApi.Features.Auth;
using SpotMapApi.Features.Markers;
using SpotMapApi.Infrastructure.Configuration;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure typed settings from environment variables
var jwtSettings = new JwtSettings
{
    SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["Jwt:SecretKey"] ?? "",
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "SpotMapApi",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "SpotMapClient",
    AccessTokenExpiryInMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES") ?? 
                                builder.Configuration["Jwt:AccessTokenExpiryInMinutes"] ?? "15"),
    RefreshTokenExpiryInDays = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS") ?? 
                              builder.Configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7")
};

var googleSettings = new GoogleSettings
{
    ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? builder.Configuration["Google:ClientId"] ?? "",
    ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? builder.Configuration["Google:ClientSecret"] ?? "",
    RedirectUri = Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI") ?? builder.Configuration["Google:RedirectUri"] ?? "http://localhost:3000"
};

// Add settings to configuration
builder.Services.Configure<JwtSettings>(opts => 
{
    opts.SecretKey = jwtSettings.SecretKey;
    opts.Issuer = jwtSettings.Issuer;
    opts.Audience = jwtSettings.Audience;
    opts.AccessTokenExpiryInMinutes = jwtSettings.AccessTokenExpiryInMinutes;
    opts.RefreshTokenExpiryInDays = jwtSettings.RefreshTokenExpiryInDays;
});

builder.Services.Configure<GoogleSettings>(opts =>
{
    opts.ClientId = googleSettings.ClientId;
    opts.ClientSecret = googleSettings.ClientSecret;
    opts.RedirectUri = googleSettings.RedirectUri;
});

// Manual configuration settings for backward compatibility
builder.Configuration["Jwt:SecretKey"] = jwtSettings.SecretKey;
builder.Configuration["Jwt:Issuer"] = jwtSettings.Issuer;
builder.Configuration["Jwt:Audience"] = jwtSettings.Audience;
builder.Configuration["Jwt:AccessTokenExpiryInMinutes"] = jwtSettings.AccessTokenExpiryInMinutes.ToString();
builder.Configuration["Jwt:RefreshTokenExpiryInDays"] = jwtSettings.RefreshTokenExpiryInDays.ToString();
builder.Configuration["Google:ClientId"] = googleSettings.ClientId;
builder.Configuration["Google:ClientSecret"] = googleSettings.ClientSecret;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

// Configure DbContext and repositories
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                      builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register infrastructure services
builder.Services.AddSingleton<IJwtService, SpotMapApi.Infrastructure.Auth.JwtService>();
builder.Services.AddScoped<SpotMapApi.Infrastructure.Auth.GoogleTokenService>();

// Register domain services
builder.Services.AddScoped<IGoogleAuthService, SpotMapApi.Services.Auth.GoogleAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMarkerService, MarkerService>();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT Secret Key is not configured")))
    };
});

builder.Services.AddAuthorization();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Early development - recreate database to ensure fresh schema
        logger.LogInformation("Recreating database with latest schema");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        logger.LogInformation("Database successfully initialized");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}


// Map API endpoints
app.MapAuthEndpoints();
app.MapMarkerEndpoints();

app.Run();