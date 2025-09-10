namespace LawPlatform.Entities.DTO.Account.Auth.ResetPassword
{
    public class ResetPasswordRequest
    {
        public string UserId { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
