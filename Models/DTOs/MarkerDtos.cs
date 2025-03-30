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
        List<MarkerImageDto>? AdditionalImages = null,
        List<MarkerRatingDto>? Ratings = null);
        
    public record MarkerUpdateRequest(
        string? Name = null,
        Coordinates? Position = null,
        string? Type = null,
        string? Description = null);
        
    public record MarkerRatingRequest(double Rating);
    
    public record ImageUploadResponse(string ImageUrl);
    
    public record MarkerImageDto(
        int Id,
        string ImageUrl,
        string? UserId = null,
        string? UserName = null);
        
    public record MarkerRatingDto(
        int Id,
        int Value,
        string UserId,
        string? UserName = null);
}