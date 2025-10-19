using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.chat
{


    public class SendMessageRequest
    {
        public string ReceiverId { get; set; }
        public string? Content { get; set; } = null!;
        public Guid ConsultationId { get; set; }
    }
}
