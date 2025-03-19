using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Services.Auth;

namespace SpotMapApi.Features.Auth
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/auth/google", async (GoogleAuthRequest request, IGoogleAuthService authService) =>
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

            endpoints.MapPost("/api/auth/refresh", async (RefreshRequest refreshRequest, IAuthService authService) =>
            {
                try
                {
                    var authResponse = await authService.RefreshTokenAsync(refreshRequest.AccessToken, refreshRequest.RefreshToken);
                    return Results.Ok(authResponse);
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.BadRequest(new { error = "Invalid token" });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = "An error occurred", details = ex.Message });
                }
            }).WithName("RefreshToken").WithOpenApi();

            endpoints.MapPost("/api/auth/logout", async (SpotMapApi.Models.DTOs.RefreshRequest request, IAuthService authService, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest(new { error = "Invalid user" });
                }

                var success = await authService.LogoutAsync(userId, request.RefreshToken);
                
                if (success)
                {
                    return Results.Ok(new { message = "Logged out successfully" });
                }
                
                return Results.BadRequest(new { error = "Invalid token" });
            }).WithName("Logout").RequireAuthorization().WithOpenApi();
        }
    }
}