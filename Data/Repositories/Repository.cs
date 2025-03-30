using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SpotMapApi.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }
            
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }
            
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            if (include == null)
            {
                return await _dbSet.FindAsync(id);
            }
            
            // Use the primary key expression to find the entity
            var keyProperties = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties;
            if (keyProperties == null || keyProperties.Count != 1)
            {
                throw new InvalidOperationException("Entity must have a single primary key");
            }
            
            var keyProperty = keyProperties[0];
            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyAccess = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
            
            IQueryable<T> query = _dbSet;
            query = include(query);
            
            return await query.FirstOrDefaultAsync(lambda);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}