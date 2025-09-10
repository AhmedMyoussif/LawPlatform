using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.Models
{
    public class ConsultationFile
    {
        public string Id { get; set; }
        public string ConsultationId { get; set; }
        public Consultation Consultation { get; set; }

        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

}
