using LawPlatform.Entities.DTO.Profile;
using LawPlatform.Entities.Models.Auth.Identity;

namespace LawPlatform.Entities.DTO.Account.Auth.Login
{
    public class LoginResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserInfoResponse User { get; set; }
    }
}
