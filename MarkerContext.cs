using Microsoft.EntityFrameworkCore;

public class MarkerContext : DbContext
{
    public MarkerContext(DbContextOptions<MarkerContext> options)
        : base(options)
    {
    }

    public DbSet<Marker> Markers { get; set; }
}

public record Marker(int Id, string Name, double Latitude, double Longitude);
public record MarkerPost(string Name, double Latitude, double Longitude);