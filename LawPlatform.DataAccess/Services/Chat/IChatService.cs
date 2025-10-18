using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.chat;
using LawPlatform.Entities.Models;

namespace LawPlatform.DataAccess.Services.Chat
{
    public interface IChatService
    {
        Task SaveMessageAsync(ChatMessage msg);
        Task<List<ChatMessageDto>> GetConversationAsync(string userA, string userB, Guid consultaionId, int take = 50);
        Task MarkConversationAsReadAsync(string readerId, string otherUserId);
        Task<bool> CanUsersChatAsync(string senderId, string receiverId);
    }
}
