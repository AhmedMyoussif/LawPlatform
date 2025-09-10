using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.Models
{
    public class ConsultationCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }

}
