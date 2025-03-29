using SpotMapApi.Models.Entities;
using System.Collections.Generic;

namespace SpotMapApi.Models.DTOs
{
    public record MarkerPost(string Name, Coordinates Position, string Type, string UserId);
    
    public record MarkerResponse(
        int Id, 
        string Name, 
        Coordinates Position, 
        string Type, 
        string UserId, 
        string? UserName = null,
        string? Description = null,
        string? ImageUrl = null,
        double? Rating = null,
        List<string>? AdditionalImages = null);
        
    public record MarkerUpdateRequest(
        string? Name = null,
        Coordinates? Position = null,
        string? Type = null,
        string? Description = null);
        
    public record MarkerRatingRequest(double Rating);
    
    public record ImageUploadResponse(string ImageUrl);
}