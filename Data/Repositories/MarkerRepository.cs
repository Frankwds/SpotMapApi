using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SpotMapApi.Models.Entities;
using System.Linq.Expressions;

namespace SpotMapApi.Data.Repositories
{
    public class MarkerRepository : Repository<Marker>, IMarkerRepository
    {
        public MarkerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Marker>> GetAllAsync(Func<IQueryable<Marker>, IIncludableQueryable<Marker, object>>? include = null)
        {
            if (include == null)
            {
                // Default includes if none specified
                include = query => query
                    .Include(m => m.User)
                    .Include(m => m.AdditionalImages)
                    .Include(m => m.Ratings);
            }
            
            return await base.GetAllAsync(include);
        }

        public override async Task<Marker?> GetByIdAsync(object id, Func<IQueryable<Marker>, IIncludableQueryable<Marker, object>>? include = null)
        {
            if (include == null)
            {
                // Default includes if none specified
                include = query => query
                    .Include(m => m.User)
                    .Include(m => m.AdditionalImages)
                    .Include(m => m.Ratings);
            }
            
            return await base.GetByIdAsync(id, include);
        }

        public async Task<IEnumerable<Marker>> GetMarkersByUserIdAsync(string userId, Func<IQueryable<Marker>, IIncludableQueryable<Marker, object>>? include = null)
        {
            if (include == null)
            {
                // Default includes if none specified
                include = query => query
                    .Include(m => m.User)
                    .Include(m => m.AdditionalImages)
                    .Include(m => m.Ratings);
            }
            
            Expression<Func<Marker, bool>> predicate = m => m.UserId == userId;
            return await FindAsync(predicate, include);
        }
    }
}