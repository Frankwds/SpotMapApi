using Microsoft.EntityFrameworkCore;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Marker entity
            modelBuilder.Entity<Marker>().OwnsOne(m => m.Position);
            
            modelBuilder.Entity<User>()
                .HasMany(u => u.Markers)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId);
                
            modelBuilder.Entity<Marker>()
                .HasMany(m => m.AdditionalImages)
                .WithOne(img => img.Marker)
                .HasForeignKey(img => img.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Marker> Markers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<MarkerImage> MarkerImages { get; set; } = null!;
    }
}