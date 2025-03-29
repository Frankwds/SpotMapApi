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
            endpoints.MapPost("/api/markers/{id}/images", async (int id, HttpRequest request, IMarkerService markerService, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }
                    
                    // Log debug info about environment
                    logger.LogInformation($"WebRootPath: {webHostEnv.WebRootPath}");
                    logger.LogInformation($"ContentRootPath: {webHostEnv.ContentRootPath}");
                    
                    // Ensure upload directory exists
                    var uploadsPath = Path.Combine(webHostEnv.WebRootPath, "uploads", "markers");
                    Directory.CreateDirectory(uploadsPath);
                    
                    var form = await request.ReadFormAsync();
                    var file = form.Files["image"];
                    bool.TryParse(form["isMainImage"], out bool isMainImage);
                    
                    if (file == null || file.Length == 0)
                    {
                        return Results.BadRequest(new { error = "No image file provided" });
                    }
                    
                    logger.LogInformation($"Received file: {file.FileName}, Size: {file.Length} bytes, Content-Type: {file.ContentType}");
                    
                    var marker = await markerService.UploadImageAsync(id, file, isMainImage, userId);
                    
                    if (marker == null)
                    {
                        return Results.NotFound(new { error = "Marker not found or you don't have permission to modify it" });
                    }
                    
                    logger.LogInformation($"Image uploaded for marker {id}, isMainImage: {isMainImage}");
                    return Results.Ok(marker);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error uploading image");
                    return Results.Problem(
                        title: "Error uploading image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("UploadMarkerImage").RequireAuthorization().WithOpenApi();
            
            // Delete image from marker
            endpoints.MapDelete("/api/markers/{id}/images", async (int id, string imageUrl, IMarkerService markerService, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Results.Unauthorized();
                    }
                    
                    // Log debug info
                    logger.LogInformation($"WebRootPath: {webHostEnv.WebRootPath}");
                    logger.LogInformation($"ContentRootPath: {webHostEnv.ContentRootPath}");
                    
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        return Results.BadRequest(new { error = "Image URL is required" });
                    }
                    
                    logger.LogInformation($"Attempting to delete image: {imageUrl} for marker {id}");
                    
                    var marker = await markerService.DeleteImageAsync(id, imageUrl, userId);
                    
                    if (marker == null)
                    {
                        return Results.NotFound(new { error = "Marker or image not found, or you don't have permission to modify it" });
                    }
                    
                    logger.LogInformation($"Image deleted for marker {id}: {imageUrl}");
                    return Results.Ok(marker);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting image");
                    return Results.Problem(
                        title: "Error deleting image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("DeleteMarkerImage").RequireAuthorization().WithOpenApi();
        }
    }
}