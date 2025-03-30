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
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public double? Rating { get; set; }
        public List<MarkerImage> AdditionalImages { get; set; } = new List<MarkerImage>();
        public List<MarkerRating> Ratings { get; set; } = new List<MarkerRating>();
    }

    public class MarkerImage
    {
        public int Id { get; set; }
        public int MarkerId { get; set; }
        public Marker? Marker { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public User? User { get; set; }
    }

    public class MarkerRating
    {
        public int Id { get; set; }
        public int MarkerId { get; set; }
        public Marker? Marker { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public int Value { get; set; }
    }

    public record Coordinates(double Lat, double Lng);
}