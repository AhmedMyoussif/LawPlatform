namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class CustomerRegisterRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
    }
}
