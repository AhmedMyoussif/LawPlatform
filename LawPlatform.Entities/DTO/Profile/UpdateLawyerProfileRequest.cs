using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Profile
{
    public class UpdateLawyerProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Bio { get; set; }
        public string? Experiences { get; set; }
        public string? Qualifications { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public Specialization? Specialization { get; set; }
        public string? Country { get; set; }
        public string? IBAN { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
