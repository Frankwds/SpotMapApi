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
    }
}