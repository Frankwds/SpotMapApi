using Microsoft.EntityFrameworkCore;

public class MarkerContext : DbContext
{
    public MarkerContext(DbContextOptions<MarkerContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marker>().OwnsOne(m => m.Position);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Marker> Markers { get; set; }
}

public record Coordinates(double Lat, double Lng);

public class Marker
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Coordinates Position { get; set; } = new Coordinates(0, 0);
    public string Type { get; set; } = string.Empty;
}

public record MarkerPost(string Name, Coordinates Position, string Type);