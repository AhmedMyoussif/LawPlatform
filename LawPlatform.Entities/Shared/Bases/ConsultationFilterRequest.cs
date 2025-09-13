using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.Entities.Shared.Bases
{
    public class ConsultationFilterRequest
    {
        public Specialization Specialization { get; set; }
        public string Sort { get; set; } // "Newest" or "Oldest"
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
    }
}
