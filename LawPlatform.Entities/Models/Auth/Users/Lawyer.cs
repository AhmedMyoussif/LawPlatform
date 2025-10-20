using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Lawyer
    {
        public string Id { get; set; }
        [ForeignKey(nameof(Id))]
        public User User { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(500)]
        public string Experiences { get; set; }

        [MaxLength(500)]
        public string Qualifications { get; set; }

        public int YearsOfExperience { get; set; }
        public double? Rating { get; set; }

        public int? TotalReviews { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }

        [Required, MaxLength(50)]
        public string LicenseNumber { get; set; }  

        public string LicenseDocumentPath { get; set; }

        public string QualificationDocumentPath { get; set; }

        public Specialization Specialization { get; set; }

        [Required, MaxLength(100)]
        public string Country { get; set; }

        [Required, MaxLength(34)]
        public string IBAN { get; set; }

        [Required, MaxLength(50)]
        public string BankAccountNumber { get; set; }

        [Required, MaxLength(150)]
        public string BankName { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ProfileImage ProfileImage { get; set; }
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }
}
