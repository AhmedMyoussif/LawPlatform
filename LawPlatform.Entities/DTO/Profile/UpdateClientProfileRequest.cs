using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Profile
{
    public class UpdateClientProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
