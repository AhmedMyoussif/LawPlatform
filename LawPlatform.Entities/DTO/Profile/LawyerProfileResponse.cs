using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Profile
{
    public class LawyerProfileResponse
    {
        public string Id { get; set; } 
        public string Role { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }
        public Specialization Specialization { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
