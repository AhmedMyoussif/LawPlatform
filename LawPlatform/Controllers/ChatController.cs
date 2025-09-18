// LawPlatform.API/Controllers/ChatController.cs
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using LawPlatform.DataAccess.Services.Chat;
using LawPlatform.Entities.Models;
using LawPlatform.API.Hubs;

namespace LawPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User?.FindFirst("sub")?.Value;
        }

        [HttpGet("private/{otherUserId}")]
        public async Task<IActionResult> GetConversation(string otherUserId)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            var msgs = await _chatService.GetConversationAsync(me, otherUserId);
            var ordered = msgs.OrderBy(m => m.SentAt);
            return Ok(ordered);
        }

     
        [HttpPost("private/send")]
        public async Task<IActionResult> SendPrivate([FromBody] SendMessageRequest model)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            if (model == null || string.IsNullOrWhiteSpace(model.ReceiverId) || string.IsNullOrWhiteSpace(model.Content))
                return BadRequest("ReceiverId and Content are required.");

            var canChat = await _chatService.CanUsersChatAsync(me, model.ReceiverId);
            if (!canChat) return Forbid("You are not allowed to message this user.");

            var msg = new ChatMessage
            {
                SenderId = me,
                ReceiverId = model.ReceiverId,
                Content = model.Content,
                SentAt = DateTimeOffset.UtcNow,
                IsRead = false
            };

            try
            {
                await _chatService.SaveMessageAsync(msg);

                await _hubContext.Clients.User(model.ReceiverId)
                    .SendAsync("ReceivePrivateMessage", me, model.Content, msg.SentAt);

                await _hubContext.Clients.User(me)
                    .SendAsync("MessageSent", model.ReceiverId, model.Content, msg.SentAt);

                return Ok(new { success = true, sentAt = msg.SentAt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save/send chat message from {Sender} to {Receiver}", me, model.ReceiverId);
                return StatusCode(500, "Failed to send message.");
            }
        }

     
        [HttpPost("private/mark-read/{otherUserId}")]
        public async Task<IActionResult> MarkAsRead(string otherUserId)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            try
            {
                await _chatService.MarkConversationAsReadAsync(me, otherUserId);

                await _hubContext.Clients.User(otherUserId).SendAsync("MessagesReadBy", me);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark conversation as read for {Reader} vs {Other}", me, otherUserId);
                return StatusCode(500, "Failed to mark as read.");
            }
        }

        #region DTOs
        public class SendMessageRequest
        {
            public string ReceiverId { get; set; } = null!;
            public string Content { get; set; } = null!;
        }
        #endregion
    }
}
