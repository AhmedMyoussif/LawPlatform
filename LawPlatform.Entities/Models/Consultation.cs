using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.Models
{
    public class Consultation
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string ClientId { get; set; }
        public Client Client { get; set; }
        
        public decimal budget { get; set; }
        
        public int duration { get; set; }
        public ConsultationStatus Status { get; set; } = ConsultationStatus.Active;
        public string CategoryId { get; set; }
        public ConsultationCategory Category { get; set; }
        public ICollection<ConsultationFile> Files { get; set; } = new List<ConsultationFile>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    }
}
