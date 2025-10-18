using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.chat;
using LawPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly LawPlatformContext _context;

        public ChatService(ILogger<ChatService> logger, LawPlatformContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<ChatMessageDto>> GetConversationAsync(string userA, string userB, Guid consultaionId, int take = 50)
        {
            return await _context.ChatMessages
          .Where(m =>
              m.ConsultationId == consultaionId &&
              ((m.SenderId == userA && m.ReceiverId == userB) ||
               (m.SenderId == userB && m.ReceiverId == userA))
          )
          .OrderByDescending(m => m.SentAt)
          .Take(take)
          .Select(m => new ChatMessageDto
          {
              Id = m.Id,
              SenderId = m.SenderId,
              ReceiverId = m.ReceiverId,
              Content = m.Content,
              SentAt = m.SentAt.DateTime,
              IsRead = m.IsRead
          })
          .ToListAsync();
        }

        public async Task MarkConversationAsReadAsync(string readerId, string otherUserId)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == readerId && !m.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var m in unread) m.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CanUsersChatAsync(string senderId, string receiverId)
        {
            return await Task.FromResult(true);
        }

        public async Task SaveMessageAsync(ChatMessage msg)
        {
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();
        }
    }
}
