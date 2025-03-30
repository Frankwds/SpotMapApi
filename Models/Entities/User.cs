using System;
using System.Collections.Generic;

namespace SpotMapApi.Models.Entities
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public List<Marker> Markers { get; set; } = new List<Marker>();
        public List<MarkerImage> Images { get; set; } = new List<MarkerImage>();
        public List<MarkerRating> Ratings { get; set; } = new List<MarkerRating>();
    }
}