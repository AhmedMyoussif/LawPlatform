using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Consultaion
{
    public class ShowAllConsultaionWithoutDetails
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ClientId { get; set; }
        
        public decimal Budget { get; set; }
        public string Specialization { get; set; }

    }
}
