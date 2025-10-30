using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Report
{
    public class GetReportResponse
    {
        public Guid Id { get; set; }
        public Guid ConsultationId { get; set; }
        public string? Description { get; set; }

        public string Reason { get; set; }
        public string ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReportedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
