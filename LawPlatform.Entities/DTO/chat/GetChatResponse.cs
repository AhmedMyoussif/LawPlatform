using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.chat
{
    public class GetChatResponse
    {
        public string UserAId { get; set; } = null!;
        public string UserBId { get; set; } = null!;
        public Guid ConsultationId { get; set; }
        public Guid ChatId { get; set; }
    }
}
