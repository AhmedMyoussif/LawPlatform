using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class UpdateLawyerAccountStatusRequest
    {
        public string LawyerId { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }  
    }
}
