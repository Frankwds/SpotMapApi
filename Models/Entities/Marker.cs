using System;
using System.Collections.Generic;

namespace SpotMapApi.Models.Entities
{
    public class Marker
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Coordinates Position { get; set; } = new Coordinates(0, 0);
        public string Type { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public User? User { get; set; }
        // New properties to support future functionality
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public double? Rating { get; set; }
    }

    public record Coordinates(double Lat, double Lng);
}