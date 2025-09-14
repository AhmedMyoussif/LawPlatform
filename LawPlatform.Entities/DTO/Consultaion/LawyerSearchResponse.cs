using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.DTO.Consultaion
{
    public class LawyerSearchResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string Experiences { get; set; }
        public string Qualifications { get; set; }
        public int YearsOfExperience { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public Specialization Specialization { get; set; }
        public string Country { get; set; }
    }

}
