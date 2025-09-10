namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class CustomerRegisterResponse
    {
        public string Id { get; set; }
        public string AccessToken { get; set; }
        public string Role { get; set; }
        public string RefreshToken { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
