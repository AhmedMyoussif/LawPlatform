using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = null!;
        public string? ReceiverId { get; set; }
        public string? Content { get; set; } = null!;
        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsRead { get; set; } = false;
        public Guid ConsultationId { get; set; }
        public Consultation Consultation { get; set; }
        public Guid ChatId { get; set; }        
        public Chat Chat { get; set; } = null!;
    }
}
