using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class UpdateLawyerAccountStatusRequest
    {
        public string LawyerId { get; set; }
        public bool IsApproved { get; set; } // true = approve, false = reject
        public string? RejectionReason { get; set; } // optional
    }
}
