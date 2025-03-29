using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SpotMapApi.Data.UnitOfWork;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Models.Entities;
using System.IO;
using System.Linq;

namespace SpotMapApi.Services.Markers
{
    public class MarkerService : IMarkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkerService> _logger;
        private readonly IWebHostEnvironment _environment;

        public MarkerService(IUnitOfWork unitOfWork, ILogger<MarkerService> logger, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IEnumerable<MarkerResponse>> GetAllMarkersAsync()
        {
            _logger.LogInformation("Getting all markers");
            var markers = await _unitOfWork.Markers.GetAllAsync();
            return markers.Select(MapToMarkerResponse);
        }

        public async Task<IEnumerable<MarkerResponse>> GetMarkersByUserIdAsync(string userId)
        {
            var markers = await _unitOfWork.Markers.GetMarkersByUserIdAsync(userId);
            return markers.Select(MapToMarkerResponse);
        }

        public async Task<MarkerResponse?> GetMarkerByIdAsync(int id)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            return marker != null ? MapToMarkerResponse(marker) : null;
        }

        public async Task<MarkerResponse> CreateMarkerAsync(MarkerPost markerPost, string userId)
        {
            var newMarker = new Marker
            {
                Name = markerPost.Name,
                Position = markerPost.Position,
                Type = markerPost.Type,
                UserId = userId
            };

            await _unitOfWork.Markers.AddAsync(newMarker);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Marker was added: {newMarker.Id}");
            return MapToMarkerResponse(newMarker);
        }

        public async Task<bool> DeleteMarkerAsync(int id, string userId)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            
            if (marker == null)
            {
                return false;
            }

            if (marker.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized attempt to delete marker {id} by user {userId}");
                return false;
            }

            // Delete image files if exists
            if (!string.IsNullOrEmpty(marker.ImageUrl))
            {
                DeleteImageFile(marker.ImageUrl);
            }
            
            // Delete additional image files
            foreach (var additionalImage in marker.AdditionalImages)
            {
                DeleteImageFile(additionalImage.ImageUrl);
            }

            await _unitOfWork.Markers.DeleteAsync(marker);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Marker was deleted. id: {id}");
            return true;
        }
        
        public async Task<MarkerResponse?> UpdateMarkerAsync(int id, MarkerUpdateRequest request, string userId)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            
            if (marker == null)
            {
                return null;
            }

            if (marker.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized attempt to update marker {id} by user {userId}");
                return null;
            }

            // Update only the properties that are not null in the request
            if (request.Name != null)
                marker.Name = request.Name;
                
            if (request.Position != null)
                marker.Position = request.Position;
                
            if (request.Type != null)
                marker.Type = request.Type;
                
            if (request.Description != null)
                marker.Description = request.Description;

            await _unitOfWork.Markers.UpdateAsync(marker);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Marker was updated: {marker.Id}");
            return MapToMarkerResponse(marker);
        }

        public async Task<MarkerResponse?> RateMarkerAsync(int id, double rating, string userId)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            
            if (marker == null)
            {
                return null;
            }

            // Allow any authenticated user to rate a marker
            marker.Rating = rating;

            await _unitOfWork.Markers.UpdateAsync(marker);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Marker {id} was rated {rating} by user {userId}");
            return MapToMarkerResponse(marker);
        }

        public async Task<MarkerResponse?> UploadImageAsync(int id, IFormFile image, bool isMainImage, string userId)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            
            if (marker == null)
            {
                return null;
            }

            if (marker.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized attempt to upload image for marker {id} by user {userId}");
                return null;
            }

            if (image == null || image.Length == 0)
            {
                _logger.LogWarning("No image file provided");
                return null;
            }

            // Make sure WebRootPath is available and create it if needed
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                _logger.LogWarning($"WebRootPath was null. Creating directory at: {webRootPath}");
                Directory.CreateDirectory(webRootPath);
                
                // Use reflection to set the WebRootPath property since it might be read-only
                typeof(IWebHostEnvironment)
                    .GetProperty("WebRootPath")
                    ?.SetValue(_environment, webRootPath);
            }
            
            // Create the uploads directory structure
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "markers");
            Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
            
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            
            var imageUrl = $"/uploads/markers/{uniqueFileName}";
            
            if (isMainImage)
            {
                // Delete previous main image if exists
                if (!string.IsNullOrEmpty(marker.ImageUrl))
                {
                    DeleteImageFile(marker.ImageUrl);
                }
                
                marker.ImageUrl = imageUrl;
            }
            else
            {
                // Add to additional images
                var newImage = new MarkerImage
                {
                    MarkerId = marker.Id,
                    ImageUrl = imageUrl
                };
                
                marker.AdditionalImages.Add(newImage);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Image uploaded for marker {id}, isMainImage: {isMainImage}");
            return MapToMarkerResponse(marker);
        }

        public async Task<MarkerResponse?> DeleteImageAsync(int id, string imageUrl, string userId)
        {
            var marker = await _unitOfWork.Markers.GetByIdAsync(id);
            
            if (marker == null)
            {
                return null;
            }

            if (marker.UserId != userId)
            {
                _logger.LogWarning($"Unauthorized attempt to delete image for marker {id} by user {userId}");
                return null;
            }
            
            if (marker.ImageUrl == imageUrl)
            {
                // Delete main image
                DeleteImageFile(imageUrl);
                marker.ImageUrl = null;
            }
            else
            {
                // Delete from additional images
                var imageToDelete = marker.AdditionalImages.FirstOrDefault(i => i.ImageUrl == imageUrl);
                if (imageToDelete != null)
                {
                    marker.AdditionalImages.Remove(imageToDelete);
                    DeleteImageFile(imageUrl);
                }
                else
                {
                    _logger.LogWarning($"Image URL {imageUrl} not found for marker {id}");
                    return null;
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Image {imageUrl} deleted for marker {id}");
            return MapToMarkerResponse(marker);
        }

        private static MarkerResponse MapToMarkerResponse(Marker marker)
        {
            return new MarkerResponse(
                marker.Id,
                marker.Name,
                marker.Position,
                marker.Type,
                marker.UserId ?? string.Empty,
                marker.User?.Name,
                marker.Description,
                marker.ImageUrl,
                marker.Rating,
                marker.AdditionalImages?.Select(img => img.ImageUrl).ToList()
            );
        }
        
        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;
                
            try
            {
                // Extract file path from URL
                var fileName = Path.GetFileName(imageUrl);
                
                // Make sure WebRootPath is available 
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    _logger.LogWarning("WebRootPath is null when trying to delete file");
                    return;
                }
                
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "markers", fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Deleted image file: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"File not found when trying to delete: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image file: {imageUrl}");
            }
        }
    }
}