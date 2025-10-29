// LawPlatform.API/Hubs/ChatHub.cs
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using LawPlatform.DataAccess.Services.Chat;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.DTO.chat;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        private string GetUserId() =>
            Context.UserIdentifier ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                _connections[Context.ConnectionId] = userId;

                await Clients.Caller.SendAsync("Connected", userId);

                _logger.LogDebug("User connected: {UserId} (connectionId: {ConnectionId})", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnConnectedAsync error for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (_connections.TryRemove(Context.ConnectionId, out var userId))
                {
                    _logger.LogDebug("User disconnected: {UserId} (connectionId: {ConnectionId})", userId, Context.ConnectionId);
                    await Clients.All.SendAsync("UserDisconnected", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnDisconnectedAsync error for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

    
        public async Task SendPrivateMessage(string receiverId, string content , Guid consultationId)
        {
            var senderId = GetUserId();

            if (string.IsNullOrWhiteSpace(receiverId))
            {
                await Clients.Caller.SendAsync("Error", "ReceiverId  is required.");
                return;
            }
            
            if (consultationId == Guid.Empty)
            {
                await Clients.Caller.SendAsync("Error", "ConsultationId is required.");
                return;
            }


            try
            {
                var canChat = await _chatService.CanUsersChatAsync(senderId, receiverId);
                if (!canChat)
                {
                    await Clients.Caller.SendAsync("Error", "You are not allowed to message this user.");
                    return;
                }
                var chatResponse = await _chatService.GetChatAsync(senderId, receiverId, consultationId);
                Guid chatId;

                if (!chatResponse.Succeeded || chatResponse.Data == null)
                {
                    var newChat = new Chat
                    {
                        Id = Guid.NewGuid(),
                        UserAId = senderId,
                        UserBId = receiverId,
                        ConsultationId = consultationId
                    };
                    await _chatService.CreateChatAsync(newChat);
                    chatId = newChat.Id; 
                }
                else
                {
                    chatId = chatResponse.Data.ChatId;
                }

                var msg = new ChatMessage
                {
                    
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    ConsultationId = consultationId,
                    SentAt = DateTimeOffset.UtcNow,
                    IsRead = false,
                    ChatId = chatId,
                    Content = content
                };

                await _chatService.SaveMessageAsync(msg);

                await _hubSendToUser(receiverId, "ReceivePrivateMessage", senderId, content, msg.SentAt);

                await Clients.Caller.SendAsync("MessageSent", receiverId, content, msg.SentAt);

                var receiverOnline = _connections.Any(kv => kv.Value == receiverId);
                if (!receiverOnline)
                {
                    _logger.LogInformation("Receiver {ReceiverId} is offline. Message saved for later delivery.", receiverId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send private message from {Sender} to {Receiver}", senderId, receiverId);
                await Clients.Caller.SendAsync("Error", "Failed to send message.");
            }
        }


        public async Task MarkAsRead(string otherUserId)
        {
            var me = GetUserId();
            if (string.IsNullOrWhiteSpace(otherUserId))
            {
                await Clients.Caller.SendAsync("Error", "otherUserId is required.");
                return;
            }

            try
            {
                await _chatService.MarkConversationAsReadAsync(me, otherUserId);

                await _hubSendToUser(otherUserId, "MessagesReadBy", me);

                await Clients.Caller.SendAsync("MarkedAsRead", otherUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark conversation as read for {Reader} vs {Other}", me, otherUserId);
                await Clients.Caller.SendAsync("Error", "Failed to mark as read.");
            }
        }

        public async Task<List<ChatMessageDto>> GetConversation(Guid chatId, int take = 50)
        {
            var me = GetUserId();
            if (string.IsNullOrWhiteSpace(me))
            {
                return new List<ChatMessageDto>();
            }

            var response = await _chatService.GetConversationAsync(chatId, take);

            if (!response.Succeeded || response.Data == null)
            {
                return new List<ChatMessageDto>();
            }

            return response.Data.Items
                         
                          .ToList();
        }


        private async Task _hubSendToUser(string userId, string method, params object[] args)
        {
            try
            {
                await Clients.User(userId).SendAsync(method, args);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deliver hub message '{Method}' to user {UserId}", method, userId);
            }
        }
    }
}
