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
            
            // User -> Markers relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Markers)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId);
                
            // Marker -> AdditionalImages relationship
            modelBuilder.Entity<Marker>()
                .HasMany(m => m.AdditionalImages)
                .WithOne(img => img.Marker)
                .HasForeignKey(img => img.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // User -> MarkerImages relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Images)
                .WithOne(img => img.User)
                .HasForeignKey(img => img.UserId);
                
            // Marker -> Ratings relationship
            modelBuilder.Entity<Marker>()
                .HasMany(m => m.Ratings)
                .WithOne(r => r.Marker)
                .HasForeignKey(r => r.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // User -> Ratings relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Ratings)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);
                
            // Ensure one rating per user per marker
            modelBuilder.Entity<MarkerRating>()
                .HasIndex(r => new { r.UserId, r.MarkerId })
                .IsUnique();
                
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Marker> Markers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<MarkerImage> MarkerImages { get; set; } = null!;
        public DbSet<MarkerRating> MarkerRatings { get; set; } = null!;
    }
}