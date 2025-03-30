using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.IO;

namespace SpotMapApi.Features.Images
{
    public static class ImageEndpoints
    {
        public static void MapImageEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Simple and direct endpoint for marker images
            endpoints.MapGet("/api/marker-image/{filename}", async (string filename, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(filename))
                    {
                        return Results.BadRequest(new { error = "Filename is required" });
                    }
                    
                    // Sanitize the filename to prevent directory traversal attacks
                    filename = Path.GetFileName(filename);
                    
                    // Log the request for debugging
                    logger.LogInformation($"Received request for image: {filename}");
                    logger.LogInformation($"WebRootPath: {webHostEnv.WebRootPath}");
                    
                    var uploadsFolder = Path.Combine(webHostEnv.WebRootPath, "uploads", "markers");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    
                    var imagePath = Path.Combine(uploadsFolder, filename);
                    logger.LogInformation($"Looking for image at: {imagePath}");
                    
                    if (!File.Exists(imagePath))
                    {
                        logger.LogWarning($"Image not found: {imagePath}");
                        return Results.NotFound(new { error = "Image not found" });
                    }
                    
                    var contentType = GetContentType(filename);
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    
                    // Set cache control headers for better performance
                    httpContext.Response.Headers.CacheControl = "public, max-age=3600"; // Cache for 1 hour
                    
                    logger.LogInformation($"Serving marker image: {filename}, Size: {imageBytes.Length} bytes, Content-Type: {contentType}");
                    return Results.File(imageBytes, contentType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error serving marker image");
                    return Results.Problem(
                        title: "Error serving image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("GetMarkerImage").WithOpenApi();
            
            // Wildcard catch-all for any other image URLs
            endpoints.MapGet("/api/images/{*path}", async (string path, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return Results.BadRequest(new { error = "Image path is required" });
                    }
                    
                    logger.LogInformation($"Received request for image path: {path}");
                    
                    // Security: Sanitize the path to prevent directory traversal attacks
                    path = path.Replace("..", "");  // Remove attempts to navigate up directories
                    path = path.TrimStart('/');     // Remove leading slashes
                    
                    // For backward compatibility - extract filename if path is uploads/markers/filename
                    var filename = path;
                    if (path.StartsWith("uploads/markers/"))
                    {
                        filename = path.Substring("uploads/markers/".Length);
                    }
                    
                    // Determine full physical path
                    var imagePath = Path.Combine(webHostEnv.WebRootPath, "uploads", "markers", filename);
                    logger.LogInformation($"Looking for image at: {imagePath}");
                    
                    // Check if file exists
                    if (!File.Exists(imagePath))
                    {
                        logger.LogWarning($"Image not found: {imagePath}");
                        return Results.NotFound(new { error = "Image not found" });
                    }
                    
                    var contentType = GetContentType(filename);
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    
                    // Set cache control headers for better performance
                    httpContext.Response.Headers.CacheControl = "public, max-age=3600"; // Cache for 1 hour
                    
                    logger.LogInformation($"Serving image: {filename}, Size: {imageBytes.Length} bytes, Content-Type: {contentType}");
                    return Results.File(imageBytes, contentType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error serving image");
                    return Results.Problem(
                        title: "Error serving image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("GetImageByPath").WithOpenApi();
            
            // New preferred image path
            endpoints.MapGet("/uploads/images/{filename}", async (string filename, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(filename))
                    {
                        return Results.BadRequest(new { error = "Filename is required" });
                    }
                    
                    logger.LogInformation($"Received image request: {filename}");
                    
                    // Sanitize the filename to prevent directory traversal attacks
                    filename = Path.GetFileName(filename);
                    
                    // First try to find in uploads/images folder
                    var imagePath = Path.Combine(webHostEnv.WebRootPath, "uploads", "images", filename);
                    
                    // If not found, check the markers folder as fallback
                    if (!File.Exists(imagePath)) 
                    {
                        imagePath = Path.Combine(webHostEnv.WebRootPath, "uploads", "markers", filename);
                    }
                    
                    logger.LogInformation($"Looking for image at: {imagePath}");
                    
                    if (!File.Exists(imagePath))
                    {
                        logger.LogWarning($"Image not found: {imagePath}");
                        return Results.NotFound(new { error = "Image not found" });
                    }
                    
                    var contentType = GetContentType(filename);
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    
                    // Set cache control headers for better performance
                    httpContext.Response.Headers.CacheControl = "public, max-age=3600"; // Cache for 1 hour
                    
                    logger.LogInformation($"Serving image: {filename}, Size: {imageBytes.Length} bytes, Content-Type: {contentType}");
                    return Results.File(imageBytes, contentType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error serving image");
                    return Results.Problem(
                        title: "Error serving image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("GetImageByNewPath").WithOpenApi();
            
            // Legacy path support
            endpoints.MapGet("/uploads/markers/{filename}", async (string filename, HttpContext httpContext, IWebHostEnvironment webHostEnv, ILogger<Program> logger) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(filename))
                    {
                        return Results.BadRequest(new { error = "Filename is required" });
                    }
                    
                    logger.LogInformation($"Received legacy request for image: {filename}");
                    
                    // Sanitize the filename to prevent directory traversal attacks
                    filename = Path.GetFileName(filename);
                    
                    var imagePath = Path.Combine(webHostEnv.WebRootPath, "uploads", "markers", filename);
                    logger.LogInformation($"Looking for image at: {imagePath}");
                    
                    if (!File.Exists(imagePath))
                    {
                        logger.LogWarning($"Image not found: {imagePath}");
                        return Results.NotFound(new { error = "Image not found" });
                    }
                    
                    var contentType = GetContentType(filename);
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    
                    // Set cache control headers for better performance
                    httpContext.Response.Headers.CacheControl = "public, max-age=3600"; // Cache for 1 hour
                    
                    logger.LogInformation($"Serving marker image: {filename}, Size: {imageBytes.Length} bytes, Content-Type: {contentType}");
                    return Results.File(imageBytes, contentType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error serving marker image");
                    return Results.Problem(
                        title: "Error serving image",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            }).WithName("GetMarkerImageByLegacyPath").WithOpenApi();
        }
        
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }
    }
}