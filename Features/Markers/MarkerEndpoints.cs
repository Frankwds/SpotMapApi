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
            
            // New endpoints for frontend functionality
            
            // Update marker endpoint
            endpoints.MapPut("/api/markers/{id}", async (int id, MarkerUpdateRequest request, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                
                var marker = await markerService.UpdateMarkerAsync(id, request, userId);
                
                if (marker == null)
                {
                    return Results.NotFound();
                }
                
                logger.LogInformation($"Marker {id} was updated by user {userId}");
                return Results.Ok(marker);
            }).WithName("UpdateMarker").RequireAuthorization().WithOpenApi();
            
            // Rate marker endpoint
            endpoints.MapPost("/api/markers/{id}/rate", async (int id, MarkerRatingRequest ratingRequest, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                
                var marker = await markerService.RateMarkerAsync(id, ratingRequest.Rating, userId);
                
                if (marker == null)
                {
                    return Results.NotFound();
                }
                
                logger.LogInformation($"Marker {id} was rated {ratingRequest.Rating} by user {userId}");
                return Results.Ok(marker);
            }).WithName("RateMarker").RequireAuthorization().WithOpenApi();
            
            // Upload image to marker
            endpoints.MapPost("/api/markers/{id}/images", async (int id, HttpRequest request, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                
                var form = await request.ReadFormAsync();
                var file = form.Files["image"];
                bool.TryParse(form["isMainImage"], out bool isMainImage);
                
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "No image file provided" });
                }
                
                var marker = await markerService.UploadImageAsync(id, file, isMainImage, userId);
                
                if (marker == null)
                {
                    return Results.NotFound();
                }
                
                logger.LogInformation($"Image uploaded for marker {id}, isMainImage: {isMainImage}");
                return Results.Ok(marker);
            }).WithName("UploadMarkerImage").RequireAuthorization().WithOpenApi();
            
            // Delete image from marker
            endpoints.MapDelete("/api/markers/{id}/images", async (int id, string imageUrl, IMarkerService markerService, HttpContext httpContext, ILogger<Program> logger) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Results.BadRequest(new { error = "Image URL is required" });
                }
                
                var marker = await markerService.DeleteImageAsync(id, imageUrl, userId);
                
                if (marker == null)
                {
                    return Results.NotFound();
                }
                
                logger.LogInformation($"Image deleted for marker {id}: {imageUrl}");
                return Results.Ok(marker);
            }).WithName("DeleteMarkerImage").RequireAuthorization().WithOpenApi();
        }
    }
}