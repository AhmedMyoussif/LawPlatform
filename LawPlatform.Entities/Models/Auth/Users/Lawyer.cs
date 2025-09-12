using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Utilities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Lawyer
    {
        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
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

        public int Age { get; set; }

        public string Address { get; set; }

        [Required, MaxLength(50)]
        public string LicenseNumber { get; set; }  

        public string LicenseDocumentPath { get; set; }

        public string QualificationDocumentPath { get; set; }

        [Required, MaxLength(100)]
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
