using SpotMapApi.Data.UnitOfWork;
using SpotMapApi.Models.DTOs;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Services.Markers
{
    public class MarkerService : IMarkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkerService> _logger;

        public MarkerService(IUnitOfWork unitOfWork, ILogger<MarkerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Marker>> GetAllMarkersAsync()
        {
            _logger.LogInformation("Getting all markers");
            return await _unitOfWork.Markers.GetAllAsync();
        }

        public async Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId)
        {
            return await _unitOfWork.Markers.GetMarkersByUserIdAsync(userId);
        }

        public async Task<Marker?> GetMarkerByIdAsync(int id)
        {
            return await _unitOfWork.Markers.GetByIdAsync(id);
        }

        public async Task<Marker> CreateMarkerAsync(MarkerPost markerPost, string userId)
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
            return newMarker;
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

            await _unitOfWork.Markers.DeleteAsync(marker);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"Marker was deleted. id: {id}");
            return true;
        }
    }
}