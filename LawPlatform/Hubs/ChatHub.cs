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

    
        public async Task SendPrivateMessage(string receiverId, string content)
        {
            var senderId = GetUserId();

            if (string.IsNullOrWhiteSpace(receiverId) || string.IsNullOrWhiteSpace(content))
            {
                await Clients.Caller.SendAsync("Error", "ReceiverId and content are required.");
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

                var msg = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content,
                    SentAt = DateTimeOffset.UtcNow,
                    IsRead = false
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
    
        public async Task<List<ChatMessage>> GetConversation(string otherUserId, int take = 50)
        {
            var me = GetUserId();
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(otherUserId))
            {
                return new List<ChatMessage>();
            }

            var msgs = await _chatService.GetConversationAsync(me, otherUserId, take);
            return msgs.OrderBy(m => m.SentAt).ToList();
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
