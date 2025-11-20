using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Report
{
    public class AddReportRequest
    {
        public Guid ConsultationId { get; set; }
        public string Reason { get; set; }
        public string? Description { get; set; }
    }
}
