namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class GoogleRegisterResponse
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public string Email { get; set; }
    }
}
