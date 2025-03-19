using SpotMapApi.Models.DTOs;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Services.Markers
{
    public interface IMarkerService
    {
        Task<IEnumerable<Marker>> GetAllMarkersAsync();
        Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId);
        Task<Marker?> GetMarkerByIdAsync(int id);
        Task<Marker> CreateMarkerAsync(MarkerPost markerPost, string userId);
        Task<bool> DeleteMarkerAsync(int id, string userId);
    }
}