using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class LawyerRegisterResponse
    {
        public string Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }

        public string Status { get; set; }
        public string QualificationDocumentUrl { get; set; } 
    }
}
