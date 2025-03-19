using Microsoft.EntityFrameworkCore;
using System;

public class MarkerContext : DbContext
{
    public MarkerContext(DbContextOptions<MarkerContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marker>().OwnsOne(m => m.Position);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Marker> Markers { get; set; }
    public DbSet<User> Users { get; set; }
}

public record Coordinates(double Lat, double Lng);

public class Marker
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Coordinates Position { get; set; } = new Coordinates(0, 0);
    public string Type { get; set; } = string.Empty;
    public string? UserId { get; set; }
    
    public User? User { get; set; }
}

public record MarkerPost(string Name, Coordinates Position, string Type, string UserId);

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public List<Marker> Markers { get; set; } = new List<Marker>();
}

public record UserProfileResponse(string Id, string Email, string Name, string? Picture);


