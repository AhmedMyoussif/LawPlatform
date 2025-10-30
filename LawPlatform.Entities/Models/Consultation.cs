using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string ClientId { get; set; }
        public Client Client { get; set; }

        public string? LawyerId { get; set; }  
        public Lawyer Lawyer { get; set; }        
        public decimal Budget { get; set; }
        
        public int Duration { get; set; }
        public ConsultationStatus Status { get; set; } = ConsultationStatus.Active;     
        public Specialization Specialization { get; set; }
        public ICollection<ConsultationFile> Files { get; set; } = new List<ConsultationFile>();
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<Report> Reports { get; set; }


    }
}
