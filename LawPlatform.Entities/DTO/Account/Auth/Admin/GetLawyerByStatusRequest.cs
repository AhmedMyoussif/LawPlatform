using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class GetLawyerByStatusRequest
    {
        public ApprovalStatus? Status { get; set; } // Pending, Approved, Rejected
    }
}
