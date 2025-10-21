using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.chat
{
    public class GetChatIdResponse
    {
        public string SenderId { get; set; } = null!;
        public string? ReceiverId { get; set; }

        public Guid ConsultationId { get; set; }
        public Guid ChatId { get; set; }
    }
}
