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
        public string UserName { get; set; }
        public string FullName { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }
        public string Specialization { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseDocumentPath { get; set; }
        public string QualificationDocumentPath { get; set; }

        public ApprovalStatus Status { get; set; }
    }
}
