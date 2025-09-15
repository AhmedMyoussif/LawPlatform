using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Proposal
{
    public class GetProposalResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string DurationTime { get; set; }
        public string LawyerId { get; set; }
        public Guid ConsultationId { get; set; }
        public ProposalStatus Status { get; set; }

    }
}
