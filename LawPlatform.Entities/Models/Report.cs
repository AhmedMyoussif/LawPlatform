using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;

namespace LawPlatform.Entities.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Reason { get; set; }
        public string? Description { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }
        public Consultation Consultation { get; set; }

        [Required]
        public string ReporterId { get; set; }
        public User Reporter { get; set; }

        [Required]
        public string ReportedLawyerId { get; set; }
        public Lawyer ReportedLawyer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
