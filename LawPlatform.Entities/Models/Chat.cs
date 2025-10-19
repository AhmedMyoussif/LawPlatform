using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.Models
{
    public class Chat
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserAId { get; set; } = null!;
        public string UserBId { get; set; } = null!;
        public Guid ConsultationId { get; set; }

        // Navigation
        public ICollection<ChatMessage>?Messages { get; set; } = new List<ChatMessage>();
    }
}
