using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LawPlatform.Entities.DTO.Profile
{
    public class UpdateClientProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
