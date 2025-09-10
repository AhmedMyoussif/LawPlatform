using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class UpdateLawyerAccountStatusResponse
    {
        public string LawyerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ApprovalStatus Status { get; set; }
    }
}
