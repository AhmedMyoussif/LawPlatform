using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Proposal
{
    public class SubmitPropsalRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string DurationTime { get; set; }
        public Guid ConsultationId { get; set; }

    }
}
