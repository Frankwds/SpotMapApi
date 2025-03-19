using Microsoft.EntityFrameworkCore;
using SpotMapApi.Models.Entities;

namespace SpotMapApi.Data
{
    public static class DataMigration
    {
        public static async Task MigrateMarkersToUserIds(ApplicationDbContext context)
        {
            // Create a default user for existing markers
            var defaultUser = new User
            {
                Email = "default@example.com",
                Name = "Default User"
            };

            context.Users.Add(defaultUser);
            await context.SaveChangesAsync();

            // Get all markers with integer user IDs
            var markers = await context.Markers.ToListAsync();
            
            // Update markers to use the default user ID
            foreach (var marker in markers)
            {
                if (marker.UserId == "0" || string.IsNullOrEmpty(marker.UserId))
                {
                    marker.UserId = defaultUser.Id;
                }
            }

            await context.SaveChangesAsync();
            
            Console.WriteLine($"Migrated {markers.Count} markers to use the default user ID: {defaultUser.Id}");
        }
    }
}