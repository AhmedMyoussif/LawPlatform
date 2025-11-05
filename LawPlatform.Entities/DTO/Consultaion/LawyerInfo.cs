using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Consultation
{
    public class LawyerInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Specialization { get; set; }
        public string Experiences { get; set; }
        public string Bio { get; set; }
        public string Address { get; set; }
        public int YearsOfExperience { get; set; }
        public string Country { get; set; }
        public int Age { get; set; }
        public string ProfileImage { get; set; }

    }
}
