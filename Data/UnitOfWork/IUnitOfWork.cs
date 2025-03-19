using SpotMapApi.Data.Repositories;

namespace SpotMapApi.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMarkerRepository Markers { get; }
        IUserRepository Users { get; }
        Task SaveChangesAsync();
    }
}