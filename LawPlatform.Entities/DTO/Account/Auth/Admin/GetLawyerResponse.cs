using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class GetLawyerResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Specialization { get; set; }
        public string Experiences { get; set; }
        public string? QualificationDocumentUrl { get; set; }
        public string Bio { get; set; }
        public string Address { get; set; }
        public string BankName { get; set; }
        public int YearsOfExperience { get; set; }
        public string LicenseNumber { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string Country { get; set; }
        public int Age { get; set; }
        public string ProfileImage { get; set; }
        public string IBAN { get; set; }
        public string BankAccountNumber { get; set; }


    }
}
