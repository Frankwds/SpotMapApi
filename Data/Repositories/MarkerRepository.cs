using Microsoft.EntityFrameworkCore;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data.Repositories
{
    public class MarkerRepository : Repository<Marker>, IMarkerRepository
    {
        public MarkerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Marker>> GetAllAsync()
        {
            return await _dbSet.Include(m => m.User).ToListAsync();
        }

        public override async Task<Marker?> GetByIdAsync(object id)
        {
            return await _dbSet.Include(m => m.User).FirstOrDefaultAsync(m => m.Id.Equals(id));
        }

        public async Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId)
        {
            return await _dbSet.Include(m => m.User).Where(m => m.UserId == userId).ToListAsync();
        }
    }
}