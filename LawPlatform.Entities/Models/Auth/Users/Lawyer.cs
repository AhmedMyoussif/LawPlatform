using LawPlatform.Entities.Models.Auth.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Lawyer
    {
        [Key]
        public string Id { get; set; }
       
        public string UserId { get; set; }
        public User User { get; set; }

        public string Bio { get; set; }
        public string Experiences { get; set; } 
        public string Qualifications { get; set; }
        public int YearsOfExperience { get; set; } 
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string Country { get; set; }

        public string QualificationDocumentPath { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
