using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.chat;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Shared.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LawPlatform.DataAccess.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;

        public ChatService(ILogger<ChatService> logger, LawPlatformContext context, ResponseHandler responseHandler)
        {
            _logger = logger;
            _context = context;
            _responseHandler = responseHandler;
        }
        public async Task CreateChatAsync(LawPlatform.Entities.Models.Chat chat)
        {
            _context.chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<Response<List<ChatMessageDto>>> GetConversationAsync(Guid chatId , int take = 50)
        {
            _logger.LogInformation($"Fetching conversation For Chat With Id : {chatId}");

            if (chatId == Guid.Empty)
            {
                _logger.LogWarning("Invalid Chat Id provided.");
                return _responseHandler.NotFound<List<ChatMessageDto>>("Invalid Chat Id provided.");
            }
            else 
           {
                var messages = await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.SentAt.DateTime,
                    IsRead = m.IsRead,
                    ChatId = m.ChatId,
                    ConsultationId = m.ConsultationId
                    

                })
                .ToListAsync();
                return _responseHandler.Success<List<ChatMessageDto>>(messages, "Conversation Retrieved Successfully");
            }
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

        public async Task<Response<GetChatResponse>> GetChatAsync(string userA, string userB, Guid consultaionID)
        {
            var chatId = await _context.chats
            .Where(c => c.ConsultationId == consultaionID &&
                        ((c.UserAId == userA && c.UserBId == userB) ||
                            (c.UserAId == userB && c.UserBId == userA)))
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

            if (chatId == Guid.Empty)
            {
                return _responseHandler.NotFound<GetChatResponse>($"Chat with Id {chatId}: Not Found.");
            }
            else
            {
                var newChat = new GetChatResponse
                {
                    UserAId = userA,
                    UserBId = userB,
                    ConsultationId = consultaionID,
                    ChatId = chatId
                };
                return _responseHandler.Success<GetChatResponse>(newChat, "ChatId Retrieved Succussfully");
            }
        }

        public async Task<ChatMessageDto> SendPrivateMessageAsync(string senderId, string receiverId, string content, Guid consultationId)
        {
            if (string.IsNullOrWhiteSpace(receiverId))
                throw new ArgumentException("ReceiverId is required.");
            var consultationExists = await _context.consultations.AnyAsync(c => c.Id == consultationId);
            if (!consultationExists)
            {
                throw new ArgumentException("Invalid ConsultationId.");
            }


            var canChat = await CanUsersChatAsync(senderId, receiverId);
            if (!canChat)
                throw new InvalidOperationException("You are not allowed to message this user.");

            var chatId = await CreateOrGetChatAsync(senderId, receiverId, consultationId);

            var msg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ConsultationId = consultationId,
                SentAt = DateTimeOffset.UtcNow,
                IsRead = false,
                ChatId = chatId
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();
            var dto = new ChatMessageDto
            {
                Id = msg.Id,
                SenderId = msg.SenderId,
                ReceiverId = msg.ReceiverId,
                Content = msg.Content,
                SentAt = msg.SentAt.DateTime,
                IsRead = msg.IsRead,
                ChatId = msg.ChatId,
                ConsultationId = msg.ConsultationId
            };
            return dto;
        }


        private async Task<Guid> CreateOrGetChatAsync(string userAId, string userBId, Guid consultationId)
        {
            var chat = await _context.chats
                .FirstOrDefaultAsync(c => c.ConsultationId == consultationId &&
                    ((c.UserAId == userAId && c.UserBId == userBId) ||
                     (c.UserAId == userBId && c.UserBId == userAId)));

            if (chat != null)
            {
                return chat.Id;
            }

            var newChat = new LawPlatform.Entities.Models.Chat
            {
                Id = Guid.NewGuid(),
                UserAId = userAId,
                UserBId = userBId,
                ConsultationId = consultationId
            };

            _context.chats.Add(newChat);
            await _context.SaveChangesAsync();

            return newChat.Id;
        }


    }
}
