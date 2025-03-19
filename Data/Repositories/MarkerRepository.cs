using Microsoft.EntityFrameworkCore;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data.Repositories
{
    public class MarkerRepository : Repository<Marker>, IMarkerRepository
    {
        public MarkerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId)
        {
            return await _dbSet.Where(m => m.UserId == userId).ToListAsync();
        }
    }
}