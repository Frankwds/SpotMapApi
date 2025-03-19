using SpotMapApi.Data.Repositories;

namespace SpotMapApi.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IMarkerRepository? _markerRepository;
        private IUserRepository? _userRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IMarkerRepository Markers => _markerRepository ??= new MarkerRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}