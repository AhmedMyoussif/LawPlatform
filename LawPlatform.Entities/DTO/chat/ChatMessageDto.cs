using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = null!;
        public string? ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public DateTimeOffset SentAt { get; set; }
        public bool IsRead { get; set; }
        public Guid ConsultationId { get; set; }
        public Guid ChatId { get; set; }
    }
}
