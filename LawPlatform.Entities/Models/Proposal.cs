using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.Models
{
    public class Proposal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Offer amount must be positive")]
        public decimal OfferAmount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string LawyerId { get; set; }
        public Lawyer Lawyer { get; set; }

        public Guid ConsultationId { get; set; }
        public Consultation Consultation { get; set; }


        public OfferStatus Status { get; set; }


    }
}
