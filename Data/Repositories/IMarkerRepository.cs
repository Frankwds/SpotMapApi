using Microsoft.EntityFrameworkCore.Query;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data.Repositories
{
    public interface IMarkerRepository : IRepository<Marker>
    {
        Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId, Func<IQueryable<Marker>, IIncludableQueryable<Marker, object>>? include = null);
    }
}