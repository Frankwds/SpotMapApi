using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Services.Markers;

namespace SpotMapApi.Features.Markers
{
    public static class MarkerEndpoints
    {
        public static void MapMarkerEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/markers", async (IMarkerService markerService, ILogger<Program> logger) =>
            {
                logger.LogInformation("Get all markers endpoint was called");
                var markers = await markerService.GetAllMarkersAsync();
                return Results.Ok(markers);
            }).WithName("GetMarkers").WithOpenApi();

            endpoints.MapGet("/api/markers/me", async (IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest(new { error = "Invalid user" });
                }
                
                logger.LogInformation($"Get user markers endpoint was called by user {userId}");
                var markers = await markerService.GetMarkersByUserIdAsync(userId);
                return Results.Ok(markers);
            }).WithName("GetUserMarkers").RequireAuthorization().WithOpenApi();
            
            endpoints.MapGet("/api/markers/user", async (IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                
                logger.LogInformation($"Get user markers endpoint '/api/markers/user' was called by user {userId}");
                var markers = await markerService.GetMarkersByUserIdAsync(userId);
                return Results.Ok(markers);
            }).WithName("GetCurrentUserMarkers").RequireAuthorization().WithOpenApi();

            endpoints.MapGet("/api/markers/{id}", async (int id, IMarkerService markerService) =>
            {
                var marker = await markerService.GetMarkerByIdAsync(id);
                if (marker == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(marker);
            }).WithName("GetMarkerById").WithOpenApi();

            endpoints.MapPost("/api/markers", async (MarkerPost markerPost, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                
                var newMarker = await markerService.CreateMarkerAsync(markerPost, userId);
                logger.LogInformation($"Marker was added: {newMarker.Id}");
                
                return Results.Created($"/api/markers/{newMarker.Id}", newMarker);
            }).WithName("AddMarker").RequireAuthorization().WithOpenApi();

            endpoints.MapDelete("/api/markers/{id}", async (int id, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest(new { error = "Invalid user" });
                }
                
                var result = await markerService.DeleteMarkerAsync(id, userId);
                
                if (!result)
                {
                    return Results.NotFound();
                }
                
                logger.LogInformation($"Marker was deleted. id: {id}");
                return Results.Ok();
            }).WithName("DeleteMarker").RequireAuthorization().WithOpenApi();
        }
    }
}