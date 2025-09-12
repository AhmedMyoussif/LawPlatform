using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LawPlatform.Entities.DTO.Account.Auth.Register
{
    public class LawyerRegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(500)]
        public string Experiences { get; set; }

        [MaxLength(500)]
        public string Qualifications { get; set; }

        public int YearsOfExperience { get; set; }

        [Required, MaxLength(50)]
        public string LicenseNumber { get; set; }

        [Required, MaxLength(100)]
        public Specialization Specialization { get; set; }

        [Required, MaxLength(100)]
        public string Country { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }

        [Required, MaxLength(34)]
        public string IBAN { get; set; }

        public string BankAccountNumber { get; set; }

        [Required, MaxLength(150)]
        public string BankName { get; set; }

        public IFormFile LicenseDocument { get; set; }
        public IFormFile QualificationDocument { get; set; }

    }
}
