namespace LawPlatform.Entities.DTO.Account.Auth.ResetPassword
{
    public class ResetPasswordResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }
}
