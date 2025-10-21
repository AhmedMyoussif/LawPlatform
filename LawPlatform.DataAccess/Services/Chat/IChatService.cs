using LawPlatform.Entities.DTO.chat;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Shared;
using LawPlatform.Entities.Shared.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.DataAccess.Services.Chat
{
    public interface IChatService
    {
        Task<ChatMessageDto> SendPrivateMessageAsync(string senderId, string receiverId, string content, Guid consultationId);
        Task CreateChatAsync(LawPlatform.Entities.Models.Chat chat);
        Task SaveMessageAsync(ChatMessage msg);
        Task<Response<PaginatedList<ChatMessageDto>>> GetConversationAsync(Guid chatId, int pageNumber = 1, int pageSize = 50);
        Task MarkConversationAsReadAsync(string readerId, string otherUserId);
        Task<bool> CanUsersChatAsync(string senderId, string receiverId);
        Task<Response<GetChatResponse>> GetChatAsync(string userA, string userB, Guid consultaionID);
    }
}
