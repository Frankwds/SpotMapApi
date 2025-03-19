using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data.Repositories
{
    public interface IMarkerRepository : IRepository<Marker>
    {
        Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId);
    }
}