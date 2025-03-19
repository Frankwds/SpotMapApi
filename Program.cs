using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using System.Security.Claims;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration["Jwt:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "SpotMapApi";
builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "SpotMapClient";
builder.Configuration["Jwt:AccessTokenExpiryInMinutes"] = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES") ?? "15";
builder.Configuration["Jwt:RefreshTokenExpiryInDays"] = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS") ?? "7";

// Set Google configuration from environment variables
builder.Configuration["Google:ClientId"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
builder.Configuration["Google:ClientSecret"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

// Add DbContext
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                      builder.Configuration.GetConnectionString("MarkerContext");

builder.Services.AddDbContext<MarkerContext>(options =>
    options.UseSqlServer(connectionString));

// Register auth services
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<GoogleAuthService>();

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SpotMapApi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SpotMapClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured")))
    };
});

builder.Services.AddAuthorization();

// Configure the HTTP request pipeline.
var app = builder.Build();
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
    var context = services.GetRequiredService<MarkerContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Run database migrations
        context.Database.Migrate();

        // Check if we need to migrate marker data
        if (context.Markers.Any() && !context.Users.Any())
        {
            logger.LogInformation("Migrating existing markers to user accounts...");
            await DataMigration.MigrateMarkersToUserIds(context);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}


// Auth endpoints
app.MapPost("/api/auth/google", async (GoogleAuthRequest request, GoogleAuthService authService) =>
{
    try
    {
        var authResponse = await authService.AuthenticateWithGoogleAsync(request.AuthCode);
        return Results.Ok(authResponse);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = "Invalid authentication code", details = ex.Message });
    }
}).WithName("GoogleAuth").WithOpenApi();

// Additional models



app.MapPost("/api/auth/refresh", async (RefreshRequest refreshRequest, MarkerContext context, JwtService jwtService) =>
{
    var refreshToken = refreshRequest.RefreshToken;
    var accessToken = refreshRequest.AccessToken;

    var principal = jwtService.GetPrincipalFromExpiredToken(accessToken);
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

    var user = await context.Users
        .FirstOrDefaultAsync(u => u.Id == userId && u.RefreshToken == refreshToken);

    if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
    {
        return Results.BadRequest(new { error = "Invalid token" });
    }

    var newAccessToken = jwtService.GenerateAccessToken(user);
    var newRefreshToken = jwtService.GenerateRefreshToken();

    user.RefreshToken = newRefreshToken;
    await context.SaveChangesAsync();

    return Results.Ok(new
    {
        accessToken = newAccessToken,
        refreshToken = newRefreshToken,
        expiresIn = Convert.ToInt32(builder.Configuration["Jwt:AccessTokenExpiryInMinutes"]) * 60
    });
}).WithName("RefreshToken").WithOpenApi();

app.MapPost("/api/auth/logout", async (MarkerContext context, HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.RefreshToken == request.RefreshToken);

    if (user != null)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await context.SaveChangesAsync();
    }

    return Results.Ok(new { message = "Logged out successfully" });
}).WithName("Logout").RequireAuthorization().WithOpenApi();

// Marker endpoints
app.MapGet("/markers", async (MarkerContext context, ILogger<Program> logger) =>
{
    logger.LogInformation("Markers endpoint was called.");
    return await context.Markers.ToListAsync();
}).WithName("GetMarkers").WithOpenApi();

app.MapPost("/markers", async (MarkerPost markerPost, MarkerContext context, HttpContext httpContext, ILogger<Program> logger) =>
{
    // Get the user ID from the JWT claims
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    var newMarker = new Marker
    {
        Name = markerPost.Name,
        Position = markerPost.Position,
        Type = markerPost.Type,
        UserId = userId
    };
    context.Markers.Add(newMarker);
    await context.SaveChangesAsync();
    logger.LogInformation($"Marker was added: {newMarker}");
    return newMarker;
}).WithName("AddMarker").RequireAuthorization().WithOpenApi();

app.MapDelete("/markers/{id}", async (int id, MarkerContext context, HttpContext httpContext, ILogger<Program> logger) =>
{
    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    var marker = await context.Markers.FindAsync(id);
    if (marker == null)
    {
        return Results.NotFound();
    }

    // Verify ownership
    if (marker.UserId != userId)
    {
        return Results.Forbid();
    }

    context.Markers.Remove(marker);
    await context.SaveChangesAsync();
    logger.LogInformation($"Marker was deleted. id: {id}");
    return Results.Ok(marker);
}).WithName("DeleteMarker").RequireAuthorization().WithOpenApi();

app.Run();

// Additional models
public record RefreshRequest(string AccessToken, string RefreshToken);

public class GoogleAuthRequest
{
    public string AuthCode { get; set; }
}