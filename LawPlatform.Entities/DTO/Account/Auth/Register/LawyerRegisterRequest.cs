using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class LawyerRegisterRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }

        public string Bio { get; set; }
        public string Experiences { get; set; }
        public string Qualifications { get; set; }
        public int YearsOfExperience { get; set; }


        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string Country { get; set; }


        public IFormFile QualificationDocument { get; set; }
    }
}
