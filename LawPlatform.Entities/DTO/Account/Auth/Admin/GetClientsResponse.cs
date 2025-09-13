using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Users;

namespace LawPlatform.Entities.DTO.Account.Auth.Admin
{
    public class GetClientsResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ConsultationCount { get; set; }
    }
}
