using SpotMapApi.Models.Entities;

namespace SpotMapApi.Models.DTOs
{
    public record MarkerPost(string Name, Coordinates Position, string Type, string UserId);
    
    public record MarkerResponse(
        int Id, 
        string Name, 
        Coordinates Position, 
        string Type, 
        string UserId, 
        string? UserName = null);
}