using Microsoft.AspNetCore.Http;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Services.Markers
{
    public interface IMarkerService
    {
        Task<IEnumerable<MarkerResponse>> GetAllMarkersAsync();
        Task<IEnumerable<MarkerResponse>> GetMarkersByUserIdAsync(string userId);
        Task<MarkerResponse?> GetMarkerByIdAsync(int id);
        Task<MarkerResponse> CreateMarkerAsync(MarkerPost markerPost, string userId);
        Task<bool> DeleteMarkerAsync(int id, string userId);
        Task<MarkerResponse?> UpdateMarkerAsync(int id, MarkerUpdateRequest request, string userId);
        Task<MarkerResponse?> RateMarkerAsync(int id, double rating, string userId);
        Task<MarkerResponse?> UploadImageAsync(int id, IFormFile image, bool isMainImage, string userId);
        Task<MarkerResponse?> DeleteImageAsync(int id, string imageUrl, string userId);
    }
}